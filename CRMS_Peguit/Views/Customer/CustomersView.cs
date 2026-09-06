using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms.Views.Customers
{
    public class CustomersView : UserControl
    {
        private readonly CustomerController _controller;

        private DataGridView grid = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbFilter = null!;
        private Button btnAdd = null!;

        private KpiCard kpiTotal = null!;
        private KpiCard kpiActive = null!;
        private KpiCard kpiInactive = null!;
        private KpiCard kpiThisMonth = null!;
        private KpiCard? _activeKpi; // which KPI card (if any) is currently filtering the grid

        public CustomersView()
        {
            _controller = new CustomerController();

            InitializeUI();
            LayoutControls();
            RefreshKpis();
            RefreshGrid();

            Resize += (_, _) => LayoutControls();
        }

        private void InitializeUI()
        {
            BackColor = Theme.Background;
            Padding = new Padding(30);

            Controls.Add(new Label
            {
                Text = "Customers",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                Location = new Point(30, 25),
                AutoSize = true
            });

            // ---- KPI cards ----
            kpiTotal = new KpiCard("Total Customers", "total", Theme.Primary);
            kpiActive = new KpiCard("Active Customers", "active", Color.MediumSeaGreen);
            kpiInactive = new KpiCard("Inactive Customers", "inactive", Color.IndianRed);
            kpiThisMonth = new KpiCard("New This Month", "this_month", Color.Goldenrod);

            foreach (var kpi in new[] { kpiTotal, kpiActive, kpiInactive, kpiThisMonth })
            {
                kpi.Click += KpiCard_Click;
                Controls.Add(kpi);
            }

            txtSearch = new TextBox
            {
                PlaceholderText = "Search first name, last name, email, phone...",
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
            cmbFilter.Items.AddRange(new[] { "All Status", "active", "inactive", "prospect" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (_, _) =>
            {
                // Manually changing the dropdown clears any KPI-driven filter,
                // so the two filter mechanisms don't fight each other.
                SetActiveKpi(null);
                RefreshGrid();
            };

            btnAdd = CreateButton("+ Add Customer", Theme.Primary, Theme.Surface);
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
            // Clicking the Name cell itself also opens the detail view
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var customer = GetCustomerAtRow(e.RowIndex);
                if (customer is not null) ViewCustomer(customer);
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

            // Clicking the already-active KPI toggles the filter off
            SetActiveKpi(_activeKpi == clicked ? null : clicked);

            // Clear the dropdown/search so the KPI filter is the obvious one in effect
            cmbFilter.SelectedIndexChanged -= null; // no-op guard, kept explicit for clarity
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

            // KPI-driven filter takes priority over the dropdown
            if (_activeKpi is not null)
            {
                var now = DateTime.UtcNow;
                query = _activeKpi.FilterKey switch
                {
                    "active" => query.Where(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase)),
                    "inactive" => query.Where(c => string.Equals(c.Status, "inactive", StringComparison.OrdinalIgnoreCase)),
                    "this_month" => query.Where(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month),
                    _ => query // "total" - no filter
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
                    Email = string.IsNullOrWhiteSpace(c.Email) ? "-" : c.Email,
                    Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone,
                    c.Type,
                    c.Status
                })
                .ToList();

            if (grid.Columns["CustomerId"] is not null)
                grid.Columns["CustomerId"].Visible = false;

            if (grid.Columns["Name"] is not null)
            {
                grid.Columns["Name"].HeaderText = "Full Name";
                grid.Columns["Name"].FillWeight = 160;
            }

            grid.Columns.Add(new ActionsColumn());
        }

        private Customer? GetCustomerAtRow(int rowIndex)
        {
            object? idValue = grid.Rows[rowIndex].Cells["CustomerId"].Value;
            if (idValue is null || !int.TryParse(idValue.ToString(), out int customerId))
                return null;

            return _controller.GetAll().FirstOrDefault(c => c.CustomerId == customerId);
        }

        private Customer? GetSelectedCustomer()
        {
            if (grid.CurrentRow is null) return null;
            return GetCustomerAtRow(grid.CurrentRow.Index);
        }

        private void GridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            Customer? customer = GetSelectedCustomer();
            if (customer is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var viewItem = new ToolStripMenuItem("View");
            viewItem.Click += (_, _) => ViewCustomer(customer);

            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (_, _) => EditCustomer(customer);

            var archiveItem = new ToolStripMenuItem("Archive");
            archiveItem.Click += (_, _) => ArchiveCustomer(customer);

            menu.Items.Add(viewItem);
            menu.Items.Add(editItem);
            menu.Items.Add(archiveItem);

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void ViewCustomer(Customer customer)
        {
            using var form = new CustomerDetailForm(customer, _controller);
            form.ShowDialog();
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
            var confirmation = MessageBox.Show(
                $"Archive '{customer.FullName}'?", "Archive Customer",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes) return;

            _controller.SoftDelete(customer);
            RefreshKpis();
            RefreshGrid();
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