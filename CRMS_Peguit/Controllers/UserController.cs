using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.infrastructure.Security;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Controllers
{
    public class UserController : IDisposable
    {
        private readonly RealEstateDbContext _db;
        public int TenantId => CurrentSession.TenantId;
        private readonly NotificationController _notifCtrl;

        public UserController()
        {
            _db = LocalDb.CreateContext(tenantId: TenantId);
            _notifCtrl = new NotificationController(_db);
        }

        private void EnsureAdmin()
        {
            if (CurrentSession.CurrentUser is not CRMS_Peguit.winforms.Models.Roles.Admin &&
                CurrentSession.CurrentUser is not CRMS_Peguit.winforms.Models.Roles.SuperAdmin)
            {
                throw new UnauthorizedAccessException("Only Admins can perform this action.");
            }
        }

        public async Task<List<User>> GetAllAsync(bool includeInactive = false)
        {
            EnsureAdmin();
            var query = _db.Users.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(u => u.Status == "active");
            }

            // Exclude Admins and SuperAdmins from the managed list
            var managedRoleIds = await _db.Roles
                .Where(r => r.RoleName == "Manager" || r.RoleName == "Agent")
                .Select(r => r.RoleId)
                .ToListAsync();

            query = query.Where(u => managedRoleIds.Contains(u.RoleId));

            return await query.OrderBy(u => u.Person.FirstName).ThenBy(u => u.Person.LastName).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            EnsureAdmin();
            return await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<List<Role>> GetManagedRolesAsync()
        {
            return await _db.Roles
                .AsNoTracking()
                .Where(r => r.RoleName == "Manager" || r.RoleName == "Agent")
                .ToListAsync();
        }

        public async Task CreateAsync(User user, string plainTextPassword)
        {
            EnsureAdmin();

            if (await _db.Users.AnyAsync(u => u.Person.Email == user.Email))
            {
                throw new InvalidOperationException("A user with this email already exists in your tenant.");
            }

            var validRoles = await GetManagedRolesAsync();
            if (!validRoles.Any(r => r.RoleId == user.RoleId))
            {
                throw new InvalidOperationException("Invalid role selected. You can only create Managers and Agents.");
            }

            if (user.PersonId <= 0 && user.Person == null)
            {
                user.Person = new Person
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Suffix = user.Suffix,
                    Email = user.Email
                };
            }

            user.PasswordHash = PasswordHasher.Hash(plainTextPassword);
            user.Status = "active";
            user.CreatedAt = DateTime.UtcNow;

            _db.Users.Add(user);
            try
            {
                await _db.SaveChangesAsync();

                _notifCtrl.NotifyAdmins(
                    TenantId,
                    NotificationType.AdminAccountCreated,
                    "New User Account Created",
                    $"User account '{user.FullName}' was created under Tenant #{TenantId}.",
                    "User",
                    user.UserId);
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Database error: {inner}", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            EnsureAdmin();

            if (await _db.Users.AnyAsync(u => u.Person.Email == user.Email && u.UserId != user.UserId))
            {
                throw new InvalidOperationException("A user with this email already exists in your tenant.");
            }

            var validRoles = await GetManagedRolesAsync();
            if (!validRoles.Any(r => r.RoleId == user.RoleId))
            {
                throw new InvalidOperationException("Invalid role selected. You can only assign Manager or Agent roles.");
            }

            var existing = await _db.Users.Include(u => u.Person).SingleOrDefaultAsync(u => u.UserId == user.UserId);
            if (existing == null) throw new InvalidOperationException("User not found.");

            existing.FirstName = user.FirstName;
            existing.MiddleName = user.MiddleName;
            existing.LastName = user.LastName;
            existing.Suffix = user.Suffix;
            existing.Email = user.Email;
            existing.RoleId = user.RoleId;

            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            EnsureAdmin();

            if (id == CurrentSession.UserId)
            {
                throw new InvalidOperationException("You cannot deactivate your own account.");
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserId == id);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.Status = "inactive";
            await _db.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int id)
        {
            EnsureAdmin();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserId == id);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.Status = "active";
            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int id, string newPassword)
        {
            EnsureAdmin();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserId == id);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            await _db.SaveChangesAsync();
        }

        public int GetActiveUsersCount()
        {
            try
            {
                return _db.Users.AsNoTracking().Count(u => u.Status.ToLower() == "active");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserController.GetActiveUsersCount] Error: {ex.Message}");
                return 0;
            }
        }

        public void Dispose() => _db.Dispose();
    }
}
