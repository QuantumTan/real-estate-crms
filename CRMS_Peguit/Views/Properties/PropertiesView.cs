using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Properties
{
    public partial class PropertiesView : UserControl
    {
        private readonly PropertyController _controller;

        public PropertiesView()
        {
            InitializeComponent();
            _controller = new PropertyController();

            BindEvents();
            LayoutControls();
            RefreshGrid();

            Resize += (_, _) => LayoutControls();
        }

        private void BindEvents()
        {
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            cmbFilter.Items.Clear();
            cmbFilter.Items.AddRange(new[] { "All Status", "available", "reserved", "sold", "inactive" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (_, _) => RefreshGrid();

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
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var property = GetPropertyAtRow(e.RowIndex);
                if (property is not null) ViewProperty(property);
            };
        }

        private void LayoutControls()
        {
            int availableWidth = Math.Max(0, Width - 60);
            int x = 30;

            txtSearch.Location = new Point(x, 75);
            txtSearch.Size = new Size(Math.Max(200, (int)(availableWidth * 0.35)), 36);

            cmbFilter.Location = new Point(x + txtSearch.Width + 15, 75);
            cmbFilter.Size = new Size(150, 36);

            btnAdd.Location = new Point(x + txtSearch.Width + cmbFilter.Width + 30, 73);
            btnAdd.Size = new Size(150, 38);

            grid.Location = new Point(x, 130);
            grid.Size = new Size(availableWidth, Math.Max(0, Height - 160));
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();

            var owners = _controller.GetOwnerCustomers()
                .ToDictionary(x => x.CustomerId, x => x.FullName);

            var agents = _controller.GetAgents()
                .ToDictionary(x => x.UserId, x => x.FullName);

            IEnumerable<Property> query = _controller.GetAll();

            string? filter = cmbFilter.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(filter) && filter != "All Status")
            {
                query = query.Where(property =>
                    string.Equals(property.Status, filter, StringComparison.OrdinalIgnoreCase));
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
                    property.Address,
                    Type = string.IsNullOrWhiteSpace(property.PropertyType) ? "-" : property.PropertyType,
                    Price = property.Price.ToString("N2"),
                    property.Status,
                    Assignment = property.AssignmentStatus,
                    Owner = GetName(owners, property.OwnerCustomerId),
                    ListedBy = GetName(agents, property.ListedByAgentId)
                })
                .ToList();

            var propertyIdColumn = grid.Columns["PropertyId"];
            if (propertyIdColumn is not null)
            {
                propertyIdColumn.Visible = false;
            }

            var addressColumn = grid.Columns["Address"];
            if (addressColumn is not null)
            {
                addressColumn.FillWeight = 180;
            }

            var listedByColumn = grid.Columns["ListedBy"];
            if (listedByColumn is not null)
            {
                listedByColumn.HeaderText = "Listed By";
            }

            grid.Columns.Add(new ActionsColumn());
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

            if (RbacService.CanEditAssignedRecord(property.ListedByAgentId))
            {
                var editItem = new ToolStripMenuItem("Edit");
                editItem.Click += (_, _) => EditProperty(property);
                menu.Items.Add(editItem);

                var deleteItem = new ToolStripMenuItem("Remove");
                deleteItem.Click += (_, _) => DeleteProperty(property);
                menu.Items.Add(deleteItem);
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
