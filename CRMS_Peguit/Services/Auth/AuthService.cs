using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.infrastructure.Security;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Auth
{
    public class AuthResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public bool WasOffline { get; init; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalAuthCache _localCache;

        // Point this at your monsterASP-hosted API, e.g. "https://your-app.runasp.net/"
        public AuthService(string apiBaseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(6) // fail fast so offline fallback doesn't hang the UI
            };
            _localCache = new LocalAuthCache();
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // User emails are globally unique; tenant is discovered automatically
                // on the server and returned inside the JWT claims.
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    email = email.Trim(),
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
                    if (result is null)
                        return new AuthResult { Success = false, ErrorMessage = "Unexpected response from server." };

                    // Cache this success for offline use later. We hash the
                    // password ourselves right here (never send the server's
                    // hash back to the client) so offline login can verify
                    // against it next time.
                    var localHash = PasswordHasher.Hash(password);
                    _localCache.SaveSuccessfulLogin(result.TenantId, result.UserId, result.FullName, result.Email, localHash, result.RoleName);

                    int effectiveUserId = EnsureLocalUser(result.UserId, result.TenantId, result.FullName, result.Email, localHash, result.RoleName);

                    CurrentSession.Start(
                        effectiveUserId,
                        result.TenantId,
                        result.FullName,
                        result.Email,
                        result.RoleName,
                        result.Token,
                        isOffline: false);
                    return new AuthResult { Success = true };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new AuthResult { Success = false, ErrorMessage = "Invalid email or password." };

                return new AuthResult { Success = false, ErrorMessage = $"Server error ({(int)response.StatusCode})." };
            }
            catch (Exception) // network unreachable, monsterASP down, timeout, etc.
            {
                return TryLocalDbLogin(email, password);
            }
        }

        private AuthResult TryLocalDbLogin(string email, string password)
        {
            try
            {
                using var db = LocalDb.CreateContext(1);
                var user = db.Users
                    .IgnoreQueryFilters()
                    .Include(u => u.Person)
                    .Include(u => u.Role)
                    .AsNoTracking()
                    .Where(u => u.Person != null && u.Person.Email != null && u.Person.Email.ToLower() == email.Trim().ToLower())
                    .FirstOrDefault();

                if (user != null)
                {
                    bool verify = PasswordHasher.Verify(password, user.PasswordHash);
                    if (verify)
                    {
                        var role = user.Role ?? db.Roles.IgnoreQueryFilters().AsNoTracking().FirstOrDefault(r => r.RoleId == user.RoleId);
                        string roleName = role?.RoleName ?? "Agent";
                        int tenantId = role?.TenantId ?? 1;
                        string displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;

                        _localCache.SaveSuccessfulLogin(tenantId, user.UserId, displayName, user.Email, user.PasswordHash, roleName);

                        CurrentSession.Start(
                            user.UserId,
                            tenantId,
                            displayName,
                            user.Email,
                            roleName,
                            jwtToken: null,
                            isOffline: false);

                        return new AuthResult { Success = true, WasOffline = false };
                    }
                    else
                    {
                        return new AuthResult { Success = false, ErrorMessage = "Invalid email or password." };
                    }
                }
            }
            catch
            {
                // Fallback to cached credentials
            }

            return TryOfflineLogin(email, password);
        }

        private AuthResult TryOfflineLogin(string email, string password)
        {
            var cached = _localCache.TryGetCachedLogin(email.Trim());

            if (cached is null)
            {
                return new AuthResult
                {
                    Success = false,
                    WasOffline = true,
                    ErrorMessage = "No internet connection, and no previous login found on this device."
                };
            }

            bool passwordMatches =
                !string.IsNullOrWhiteSpace(cached.PasswordHash)
                && PasswordHasher.Verify(password, cached.PasswordHash);

            if (!passwordMatches)
            {
                return new AuthResult
                {
                    Success = false,
                    WasOffline = true,
                    ErrorMessage = "No internet connection, and offline credentials didn't match."
                };
            }

            int tenantId = cached.TenantId > 0 ? cached.TenantId : 1;

            int effectiveUserId = EnsureLocalUser(cached.UserId, tenantId, cached.FullName, cached.Email, cached.PasswordHash, cached.RoleName);

            CurrentSession.Start(
                effectiveUserId,
                tenantId,
                cached.FullName,
                cached.Email,
                cached.RoleName,
                jwtToken: null,
                isOffline: true);

            return new AuthResult
            {
                Success = true,
                WasOffline = true
            };
        }

        private int EnsureLocalUser(int userId, int tenantId, string fullName, string email, string? passwordHash, string roleName)
        {
            try
            {
                if (tenantId <= 0) tenantId = 1;
                using var db = LocalDb.CreateContext(tenantId);

                // 1. Ensure Role exists
                var role = db.Roles.FirstOrDefault(r => r.RoleName.ToLower() == roleName.Trim().ToLower());
                if (role == null)
                {
                    role = new domain.entities.Role { TenantId = tenantId, RoleName = roleName.Trim() };
                    db.Roles.Add(role);
                    db.SaveChanges();
                }

                // 2. Check if user exists by UserId
                var userById = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (userById != null)
                {
                    if (!string.IsNullOrWhiteSpace(passwordHash))
                        userById.PasswordHash = passwordHash;
                    userById.RoleId = role.RoleId;
                    userById.Status = "active";
                    db.SaveChanges();
                    return userId;
                }

                // 3. Check if user exists by Email
                var userByEmail = db.Users.FirstOrDefault(u => u.Person != null && u.Person.Email != null && u.Person.Email.ToLower() == email.Trim().ToLower());
                if (userByEmail != null)
                {
                    if (!string.IsNullOrWhiteSpace(passwordHash))
                        userByEmail.PasswordHash = passwordHash;
                    userByEmail.RoleId = role.RoleId;
                    userByEmail.Status = "active";
                    db.SaveChanges();
                    return userByEmail.UserId;
                }

                // 4. User does not exist locally; insert using IDENTITY_INSERT
                var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string firstName = parts.Length > 0 ? parts[0] : "User";
                string lastName = parts.Length > 1 ? parts[1] : "";

                db.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = {0})
                    BEGIN
                        DECLARE @PersonId INT;
                        SELECT TOP 1 @PersonId = PersonId FROM Persons WHERE Email = {3};
                        IF @PersonId IS NULL
                        BEGIN
                            INSERT INTO Persons (FirstName, LastName, Email, CreatedAt)
                            VALUES ({1}, {2}, {3}, GETUTCDATE());
                            SET @PersonId = SCOPE_IDENTITY();
                        END

                        SET IDENTITY_INSERT Users ON;
                        INSERT INTO Users (UserId, PersonId, PasswordHash, RoleId, Status, CreatedAt)
                        VALUES ({0}, @PersonId, {4}, {5}, 'active', GETUTCDATE());
                        SET IDENTITY_INSERT Users OFF;
                    END",
                    userId, firstName, lastName, email.Trim(), passwordHash ?? "", role.RoleId);

                return userId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureLocalUser error: {ex.Message}");
                return userId;
            }
        }

        private record LoginApiResponse(string Token, int UserId, int TenantId, string FullName, string Email, string RoleName);
    }
}