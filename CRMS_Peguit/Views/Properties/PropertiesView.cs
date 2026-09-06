using System.Drawing.Drawing2D;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.Properties
{
    public partial class PropertiesView : UserControl
    {
        private readonly PropertyController _controller;
        private string _filterStatus = "All";

        public PropertiesView()
        {
            InitializeComponent();
            _controller = new PropertyController();

            BindEvents();
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void BindEvents()
        {
            btnAdd.Click += BtnAddClick;
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterAvailable.Click += (_, _) => SetFilter("Available");
            btnFilterPending.Click += (_, _) => SetFilter("Pending");
            btnFilterSold.Click += (_, _) => SetFilter("Sold");

            // Modern Grid Styling
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(241, 245, 249);
            grid.RowTemplate.Height = 52;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 44;

            grid.CellPainting += Grid_CellPainting;
            grid.CellContentClick += GridCellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var property = GetPropertyAtRow(e.RowIndex);
                if (property is not null) ViewProperty(property);
            };
        }

        private void SetFilter(string filter)
        {
            _filterStatus = filter;
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void UpdateFilterPillStyles()
        {
            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterAvailable, "Available"),
                (btnFilterPending, "Pending"),
                (btnFilterSold, "Sold")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterStatus, name, StringComparison.OrdinalIgnoreCase);
                if (isSelected)
                {
                    btn.BackColor = Color.FromArgb(15, 91, 158);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.FromArgb(71, 85, 105);
                    btn.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                }
            }
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();

            var allList = _controller.GetAll().ToList();
            int total = allList.Count;
            int available = allList.Count(p => string.Equals(p.Status, "available", StringComparison.OrdinalIgnoreCase));
            lblSubtitle.Text = $"{total} total · {available} available";

            var owners = _controller.GetOwnerCustomers()
                .ToDictionary(x => x.CustomerId, x => x.FullName);

            var agents = _controller.GetAgents()
                .ToDictionary(x => x.UserId, x => x.FullName);

            IEnumerable<Property> query = allList;

            if (string.Equals(_filterStatus, "Available", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(p.Status, "available", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(p.Status, "pending", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(p.Status, "reserved", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(_filterStatus, "Sold", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(p.Status, "sold", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(p.Status, "inactive", StringComparison.OrdinalIgnoreCase));
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(property =>
                    ContainsText(property.Address, search) ||
                    ContainsText(property.PropertyType, search) ||
                    ContainsText(property.Status, search) ||
                    ContainsText(GetName(owners, property.OwnerCustomerId), search) ||
                    ContainsText(GetName(agents, property.ListedByAgentId), search));
            }

            grid.DataSource = query
                .Select(property => new
                {
                    property.PropertyId,
                    Address = property.Address,
                    Type = string.IsNullOrWhiteSpace(property.PropertyType) ? "-" : property.PropertyType,
                    Price = $"${property.Price:N0}",
                    Status = property.Status.ToUpper(),
                    Assignment = string.IsNullOrWhiteSpace(property.AssignmentStatus) ? "-" : property.AssignmentStatus,
                    Owner = GetName(owners, property.OwnerCustomerId),
                    ListedBy = GetName(agents, property.ListedByAgentId)
                })
                .ToList();

            var propertyIdColumn = grid.Columns["PropertyId"];
            if (propertyIdColumn is not null)
            {
                propertyIdColumn.Visible = false;
            }

            if (grid.Columns["Address"] is DataGridViewColumn addressCol)
            {
                addressCol.HeaderText = "ADDRESS";
                addressCol.FillWeight = 180;
            }

            if (grid.Columns["Type"] is DataGridViewColumn typeCol)
            {
                typeCol.HeaderText = "TYPE";
                typeCol.FillWeight = 90;
            }

            if (grid.Columns["Price"] is DataGridViewColumn priceCol)
            {
                priceCol.HeaderText = "PRICE";
                priceCol.FillWeight = 100;
            }

            if (grid.Columns["Status"] is DataGridViewColumn statusCol)
            {
                statusCol.HeaderText = "STATUS";
                statusCol.FillWeight = 90;
            }

            if (grid.Columns["Assignment"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNMENT";
                assignCol.FillWeight = 100;
            }

            if (grid.Columns["Owner"] is DataGridViewColumn ownerCol)
            {
                ownerCol.HeaderText = "OWNER";
                ownerCol.FillWeight = 120;
            }

            if (grid.Columns["ListedBy"] is DataGridViewColumn listedByCol)
            {
                listedByCol.HeaderText = "LISTED BY";
                listedByCol.FillWeight = 120;
            }

            grid.Columns.Add(new ActionsColumn());
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            // Custom render Status pill badge
            if (grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value.ToString() ?? "";
                Color bgColor;
                Color textColor;

                if (status.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(220, 252, 231);
                    textColor = Color.FromArgb(22, 101, 52);
                }
                else if (status.Equals("PENDING", StringComparison.OrdinalIgnoreCase) ||
                         status.Equals("RESERVED", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(254, 243, 199);
                    textColor = Color.FromArgb(180, 83, 9);
                }
                else if (status.Equals("SOLD", StringComparison.OrdinalIgnoreCase))
                {
                    bgColor = Color.FromArgb(219, 234, 254);
                    textColor = Color.FromArgb(30, 64, 175);
                }
                else
                {
                    bgColor = Color.FromArgb(254, 226, 226);
                    textColor = Color.FromArgb(153, 27, 27);
                }

                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    var size = TextRenderer.MeasureText(status, font);
                    int pillWidth = size.Width + 16;
                    int pillHeight = 22;
                    int pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;
                    var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

                    using (var brush = new SolidBrush(bgColor))
                    using (var path = GetRoundedRectangle(pillRect, 8))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, status, font, pillRect, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            // Style address with primary bold text
            else if (grid.Columns[e.ColumnIndex].Name == "Address" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string address = e.Value.ToString() ?? "";

                var textRect = new Rectangle(e.CellBounds.X + 12, e.CellBounds.Y,
                    e.CellBounds.Width - 20, e.CellBounds.Height);

                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, address, font, textRect, Color.FromArgb(15, 23, 42),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                e.Handled = true;
            }
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Property? GetPropertyAtRow(int rowIndex)
        {
            object? idValue = grid.Rows[rowIndex].Cells["PropertyId"].Value;
            if (idValue is null || !int.TryParse(idValue.ToString(), out int propertyId))
            {
                return null;
            }

            return _controller.GetAll().FirstOrDefault(property => property.PropertyId == propertyId);
        }

        private Property? GetSelectedProperty()
        {
            if (grid.CurrentRow is null) return null;
            return GetPropertyAtRow(grid.CurrentRow.Index);
        }

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            Property? property = GetSelectedProperty();
            if (property is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View");
            viewItem.Click += (_, _) => ViewProperty(property);
            menu.Items.Add(viewItem);

            if (RbacService.CanEditRecord(property.ListedByAgentId, property.CreatedByUserId))
            {
                var editItem = new ToolStripMenuItem("Edit");
                editItem.Click += (_, _) => EditProperty(property);
                menu.Items.Add(editItem);

                var deleteItem = new ToolStripMenuItem("Remove");
                deleteItem.Click += (_, _) => DeleteProperty(property);
                menu.Items.Add(deleteItem);
            }

            if (RbacService.CanAssignRecords)
            {
                var assignItem = new ToolStripMenuItem("Assign Agent");
                assignItem.Click += (_, _) =>
                {
                    using var dlg = new AssignAgentDialog(property.Address, _controller.GetAgents(), property.ListedByAgentId);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _controller.AssignAgent(property, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                        RefreshGrid();
                    }
                };
                menu.Items.Add(assignItem);
            }

            if (RbacService.CanApproveAssignments &&
                string.Equals(property.AssignmentStatus, "pending_review", StringComparison.OrdinalIgnoreCase))
            {
                var approveItem = new ToolStripMenuItem("Approve Assignment");
                approveItem.Click += (_, _) => ApprovePropertyAssignment(property);
                menu.Items.Add(approveItem);
            }

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void ViewProperty(Property property)
        {
            using var form = new PropertyDetailForm(property, _controller);
            form.ShowDialog();
        }

        private void EditProperty(Property property)
        {
            var owners = _controller.GetOwnerCustomers();
            var agents = _controller.GetAgents();

            EnsureExistingOwnerAndAgent(property, owners, agents);

            using var form = new PropertyInputForm(owners, agents, property);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Update(form.Result);
                RefreshGrid();
            }
        }

        private void DeleteProperty(Property property)
        {
            var confirmation = MessageBox.Show(
                $"Remove property at '{property.Address}'?",
                "Remove Property",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            _controller.Delete(property);
            RefreshGrid();
        }

        private void ApprovePropertyAssignment(Property property)
        {
            _controller.ApproveAssignment(property, "Reviewed from Properties module.");
            MessageBox.Show(
                $"Assignment for property #{property.PropertyId} has been approved.",
                "Assignment Approved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            RefreshGrid();
        }

        private void BtnAddClick(object? sender, EventArgs e)
        {
            var owners = _controller.GetOwnerCustomers();
            if (owners.Count == 0)
            {
                MessageBox.Show(
                    "Add a seller or both-type customer before creating a property listing.",
                    "Owner Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            var agents = _controller.GetAgents();
            if (agents.Count == 0)
            {
                MessageBox.Show(
                    "Add an active agent before creating a property listing.",
                    "Agent Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var form = new PropertyInputForm(owners, agents);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Add(form.Result);
                RefreshGrid();
            }
        }

        private void EnsureExistingOwnerAndAgent(
            Property property,
            List<CustomerPickerItem> owners,
            List<AgentPickerItem> agents)
        {
            if (!owners.Any(x => x.CustomerId == property.OwnerCustomerId))
            {
                owners.Add(new CustomerPickerItem(
                    property.OwnerCustomerId,
                    _controller.GetOwnerName(property.OwnerCustomerId) ?? $"Customer #{property.OwnerCustomerId}",
                    null));
            }

            if (property.ListedByAgentId.HasValue && !agents.Any(x => x.UserId == property.ListedByAgentId.Value))
            {
                agents.Add(new AgentPickerItem(
                    property.ListedByAgentId.Value,
                    _controller.GetListedAgentName(property.ListedByAgentId.Value) ?? $"User #{property.ListedByAgentId.Value}",
                    string.Empty));
            }
        }

        private static string GetName(Dictionary<int, string> names, int? id)
        {
            if (id is null) return "Unassigned";
            return names.TryGetValue(id.Value, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : "-";
        }

        private static bool ContainsText(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }
    }
}
