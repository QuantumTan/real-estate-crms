using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Properties
{
    public class PropertiesView : UserControl
    {
        private readonly PropertyController _controller;

        private DataGridView grid = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbFilter = null!;
        private Button btnAdd = null!;

        public PropertiesView()
        {
            _controller = new PropertyController();

            InitializeUI();
            LayoutControls();
            RefreshGrid();

            Resize += (_, _) => LayoutControls();
        }

        private void InitializeUI()
        {
            BackColor = Theme.Background;
            Padding = new Padding(30);

            Controls.Add(new Label
            {
                Text = "Properties",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(30, 25),
                AutoSize = true
            });

            txtSearch = new TextBox
            {
                PlaceholderText = "Search address, type, status, owner, agent...",
                Font = new Font("Segoe UI", 11),
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            cmbFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary
            };
            cmbFilter.Items.AddRange(new[] { "All Status", "available", "reserved", "sold", "inactive" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (_, _) => RefreshGrid();

            btnAdd = CreateButton("+ Add Property", Theme.Primary, Theme.Surface);
            btnAdd.Click += BtnAddClick;

            grid = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                GridColor = Theme.Border,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 45,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };

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

            Controls.Add(txtSearch);
            Controls.Add(cmbFilter);
            Controls.Add(btnAdd);
            Controls.Add(grid);
        }

        private static Button CreateButton(string text, Color backColor, Color foregroundColor)
        {
            var button = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foregroundColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void LayoutControls()
        {
            int availableWidth = Math.Max(0, Width - 60);
            int x = 30;

            txtSearch.Location = new Point(x, 75);
            txtSearch.Size = new Size(Math.Max(240, (int)(availableWidth * 0.38)), 36);

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
                if (RbacService.ShouldAutoAssignCreatedRecord)
                {
                    form.Result.ListedByAgentId = CurrentSession.UserId;
                    MessageBox.Show(
                        "This property will be assigned to you. Manager approval workflow is marked as pending until the approval backend is added.",
                        "Assignment Approval Placeholder",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

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

            if (!agents.Any(x => x.UserId == property.ListedByAgentId))
            {
                agents.Add(new AgentPickerItem(
                    property.ListedByAgentId,
                    _controller.GetListedAgentName(property.ListedByAgentId) ?? $"User #{property.ListedByAgentId}",
                    string.Empty));
            }
        }

        private static string GetName(Dictionary<int, string> names, int id)
        {
            return names.TryGetValue(id, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : "-";
        }

        private static bool ContainsText(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _controller.Dispose();
            base.Dispose(disposing);
        }
    }
}
