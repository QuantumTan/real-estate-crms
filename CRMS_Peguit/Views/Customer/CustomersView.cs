using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Customers
{
    public partial class CustomersView : UserControl
    {
        private readonly CustomerController _controller;
        private KpiCard? _activeKpi; // which KPI card (if any) is currently filtering the grid

        public CustomersView()
        {
            InitializeComponent();
            _controller = new CustomerController();

            BindEvents();
            LayoutControls();
            RefreshKpis();
            RefreshGrid();

            Resize += (_, _) => LayoutControls();
        }

        private void BindEvents()
        {
            foreach (var kpi in new[] { kpiTotal, kpiActive, kpiInactive, kpiThisMonth })
            {
                kpi.Click += KpiCard_Click;
            }

            txtSearch.TextChanged += (_, _) => RefreshGrid();

            cmbFilter.Items.Clear();
            cmbFilter.Items.AddRange(new[] { "All Status", "active", "inactive", "prospect" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (_, _) =>
            {
                SetActiveKpi(null);
                RefreshGrid();
            };

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
                var customer = GetCustomerAtRow(e.RowIndex);
                if (customer is not null) ViewCustomer(customer);
            };
        }

        private void LayoutControls()
        {
            int availableWidth = Math.Max(0, Width - 60);
            int x = 30;

            // KPI row
            int kpiY = 70;
            int kpiWidth = Math.Max(150, (availableWidth - 3 * 16) / 4);
            int kx = x;
            foreach (var kpi in new[] { kpiTotal, kpiActive, kpiInactive, kpiThisMonth })
            {
                kpi.Location = new Point(kx, kpiY);
                kpi.Size = new Size(kpiWidth, 90);
                kx += kpiWidth + 16;
            }

            int toolbarY = kpiY + 90 + 20;
            txtSearch.Location = new Point(x, toolbarY);
            txtSearch.Size = new Size(Math.Max(200, (int)(availableWidth * 0.35)), 36);

            cmbFilter.Location = new Point(x + txtSearch.Width + 15, toolbarY);
            cmbFilter.Size = new Size(150, 36);

            btnAdd.Location = new Point(x + txtSearch.Width + cmbFilter.Width + 30, toolbarY - 2);
            btnAdd.Size = new Size(150, 38);

            int gridY = toolbarY + 55;
            grid.Location = new Point(x, gridY);
            grid.Size = new Size(availableWidth, Math.Max(0, Height - gridY - 30));
        }

        private void RefreshKpis()
        {
            var counts = _controller.GetKpiCounts();
            kpiTotal.SetValue(counts.Total);
            kpiActive.SetValue(counts.Active);
            kpiInactive.SetValue(counts.Inactive);
            kpiThisMonth.SetValue(counts.ThisMonth);
        }

        private void KpiCard_Click(object? sender, EventArgs e)
        {
            if (sender is not KpiCard clicked) return;

            SetActiveKpi(_activeKpi == clicked ? null : clicked);
            cmbFilter.SelectedIndex = 0;
            RefreshGrid();
        }

        private void SetActiveKpi(KpiCard? kpi)
        {
            foreach (var card in new[] { kpiTotal, kpiActive, kpiInactive, kpiThisMonth })
                card.SetSelected(card == kpi);

            _activeKpi = kpi;
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();

            IEnumerable<Customer> query = _controller.GetAll();

            if (_activeKpi is not null)
            {
                var now = DateTime.UtcNow;
                query = _activeKpi.FilterKey switch
                {
                    "active" => query.Where(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase)),
                    "inactive" => query.Where(c => string.Equals(c.Status, "inactive", StringComparison.OrdinalIgnoreCase)),
                    "this_month" => query.Where(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month),
                    _ => query
                };
            }
            else
            {
                string? filter = cmbFilter.SelectedItem?.ToString();
                if (!string.IsNullOrWhiteSpace(filter) && filter != "All Status")
                {
                    query = query.Where(c => string.Equals(c.Status, filter, StringComparison.OrdinalIgnoreCase));
                }
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    ContainsText(c.FirstName, search) ||
                    ContainsText(c.MiddleName, search) ||
                    ContainsText(c.LastName, search) ||
                    ContainsText(c.Suffix, search) ||
                    ContainsText(c.FullName, search) ||
                    ContainsText(c.Email, search) ||
                    ContainsText(c.Phone, search));
            }

            grid.DataSource = query
                .Select(c => new
                {
                    c.CustomerId,
                    Name = c.FullName,
                    c.Email,
                    Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone,
                    Type = c.Type.ToUpper(),
                    Status = c.Status.ToUpper(),
                    Assignment = c.AssignmentStatus.ToUpper(),
                    AssignedTo = _controller.GetAssignedAgentName(c.AssignedAgentId) ?? "Unassigned"
                })
                .ToList();

            var idCol = grid.Columns["CustomerId"];
            if (idCol is not null) idCol.Visible = false;

            AddActionButton("View", "View");
            AddActionButton("Edit", "Edit");
            AddActionButton("Archive", "Archive");
        }

        private static bool ContainsText(string? value, string search) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private void AddActionButton(string columnName, string text)
        {
            var btn = new DataGridViewButtonColumn
            {
                Name = columnName,
                HeaderText = columnName,
                Text = text,
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            grid.Columns.Add(btn);
        }

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var customer = GetCustomerAtRow(e.RowIndex);
            if (customer is null) return;

            string colName = grid.Columns[e.ColumnIndex].Name;

            if (colName == "View")
            {
                ViewCustomer(customer);
            }
            else if (colName == "Edit")
            {
                EditCustomer(customer);
            }
            else if (colName == "Archive")
            {
                ArchiveCustomer(customer);
            }
        }

        private Customer? GetCustomerAtRow(int rowIndex)
        {
            if (grid.Rows[rowIndex].Cells["CustomerId"].Value is int id)
            {
                return _controller.GetById(id);
            }
            return null;
        }

        private void BtnAddClick(object? sender, EventArgs e)
        {
            using var form = new CustomerInputForm();
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Add(form.Result);
                RefreshKpis();
                RefreshGrid();
            }
        }

        private void ViewCustomer(Customer customer)
        {
            using var form = new CustomerDetailForm(customer, _controller);
            form.ShowDialog();
            RefreshKpis();
            RefreshGrid();
        }

        private void EditCustomer(Customer customer)
        {
            using var form = new CustomerInputForm(customer);
            if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
            {
                _controller.Update(form.Result);
                RefreshKpis();
                RefreshGrid();
            }
        }

        private void ArchiveCustomer(Customer customer)
        {
            var confirm = MessageBox.Show(
                $"Are you sure you want to archive '{customer.FullName}'?",
                "Confirm Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _controller.SoftDelete(customer);
                RefreshKpis();
                RefreshGrid();
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
