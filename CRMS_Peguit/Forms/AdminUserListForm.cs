using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Forms
{
    public partial class AdminUserListForm : UserControl
    {
        private readonly UserController _controller;

        public AdminUserListForm(string initialRoleFilter = "All Roles")
        {
            InitializeComponent();
            _controller = new UserController();
            BindEvents(initialRoleFilter);
            LayoutControls();
            
            this.Load += async (s, e) => await RefreshGridAsync();
            this.Resize += (s, e) => LayoutControls();
        }
        
        private void BindEvents(string initialRoleFilter)
        {
            txtSearch.TextChanged += async (s, e) => await RefreshGridAsync();

            cmbRoleFilter.Items.Clear();
            cmbRoleFilter.Items.AddRange(new[] { "All Roles", "Manager", "Agent" });
            cmbRoleFilter.SelectedItem = initialRoleFilter;
            cmbRoleFilter.SelectedIndexChanged += async (s, e) => await RefreshGridAsync();

            chkIncludeInactive.CheckedChanged += async (s, e) => await RefreshGridAsync();
            btnAdd.Click += BtnAddClick;

            grid.GridColor = Theme.Border;
            grid.RowTemplate.Height = 45;
            grid.DefaultCellStyle.BackColor = Theme.Surface;
            grid.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Border;
            grid.DefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Background;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            grid.CellContentClick += GridCellContentClick;
        }

        private void LayoutControls()
        {
            int availableWidth = Math.Max(0, Width - 60);
            int x = 30;
            int y = 90;

            txtSearch.Location = new Point(x, y);
            txtSearch.Size = new Size(300, 36);

            cmbRoleFilter.Location = new Point(x + 315, y);
            cmbRoleFilter.Size = new Size(150, 36);

            chkIncludeInactive.Location = new Point(x + 480, y + 4);

            btnAdd.Location = new Point(x + availableWidth - 150, y - 2);
            btnAdd.Size = new Size(150, 38);

            int gridY = y + 50;
            grid.Location = new Point(x, gridY);
            grid.Size = new Size(availableWidth, Math.Max(0, Height - gridY - 30));
        }

        private async Task RefreshGridAsync()
        {
            if (this.IsDisposed) return;

            try
            {
                var users = await _controller.GetAllAsync(chkIncludeInactive.Checked);

                string search = txtSearch.Text.Trim();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    users = users.Where(u => 
                        (u.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                        (u.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                    ).ToList();
                }

                var rolesDict = (await _controller.GetManagedRolesAsync()).ToDictionary(r => r.RoleId, r => r.RoleName);

                string roleFilter = cmbRoleFilter.SelectedItem?.ToString() ?? "All Roles";
                if (roleFilter != "All Roles")
                {
                    var targetRole = rolesDict.Values.FirstOrDefault(r => r.Equals(roleFilter, StringComparison.OrdinalIgnoreCase));
                    if (targetRole != null)
                    {
                        users = users.Where(u => rolesDict.ContainsKey(u.RoleId) && rolesDict[u.RoleId] == targetRole).ToList();
                    }
                }

                grid.DataSource = users.Select(u => new
                {
                    u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    Role = rolesDict.ContainsKey(u.RoleId) ? rolesDict[u.RoleId] : "Unknown",
                    Status = u.Status
                }).ToList();

                if (grid.Columns["UserId"] != null) grid.Columns["UserId"].Visible = false;
                
                if (grid.Columns["Actions"] == null)
                {
                    var actionCol = new DataGridViewButtonColumn
                    {
                        Name = "Actions",
                        HeaderText = "Actions",
                        Text = "Options",
                        UseColumnTextForButtonValue = true,
                        FillWeight = 50,
                        FlatStyle = FlatStyle.Flat
                    };
                    grid.Columns.Add(actionCol);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Users", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAddClick(object? sender, EventArgs e)
        {
            using var form = new AdminUserEditForm(_controller, null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await RefreshGridAsync();
            }
        }

        private async void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex].Name != "Actions") return;

            var idValue = grid.Rows[e.RowIndex].Cells["UserId"].Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out int userId)) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View Details");
            viewItem.Click += async (_, _) => await ViewUserAsync(userId);
            menu.Items.Add(viewItem);

            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += async (_, _) => await EditUserAsync(userId);
            menu.Items.Add(editItem);

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private async Task ViewUserAsync(int userId)
        {
            var user = await _controller.GetByIdAsync(userId);
            if (user == null) return;
            
            var roles = (await _controller.GetManagedRolesAsync()).ToDictionary(r => r.RoleId, r => r.RoleName);

            using var form = new AdminUserDetailsForm(_controller, user, roles);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await RefreshGridAsync();
            }
        }

        private async Task EditUserAsync(int userId)
        {
            var user = await _controller.GetByIdAsync(userId);
            if (user == null) return;

            using var form = new AdminUserEditForm(_controller, user);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await RefreshGridAsync();
            }
        }

    }
}
