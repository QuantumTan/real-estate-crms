using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Controllers;
using CRMS_Peguit.winforms.Controls;
using CRMS_Peguit.winforms.Models.Services;
using CRMS_Peguit.winforms.Views.Customers;
using CRMS_Peguit.winforms.Views.Leads;
using CRMS_Peguit.winforms.Views.Properties;
using CRMS_Peguit.winforms.Views.Shared;

namespace CRMS_Peguit.winforms.Views.Management
{
    public partial class ApprovalsView : UserControl
    {
        private readonly LeadController _leadController;
        private readonly CustomerController _customerController;
        private readonly PropertyController _propertyController;

        private string _filterType = "All";
        private List<PendingApprovalItem> _allItems = new();

        public ApprovalsView()
        {
            InitializeComponent();

            _leadController = new LeadController();
            _customerController = new CustomerController();
            _propertyController = new PropertyController();

            BindEvents();
            UpdateFilterPillStyles();
            RefreshGrid();
        }

        private void BindEvents()
        {
            btnRefresh.Click += (_, _) => RefreshGrid();
            txtSearch.TextChanged += (_, _) => ApplyFilterAndDisplay();

            btnFilterAll.Click += (_, _) => SetFilter("All");
            btnFilterLeads.Click += (_, _) => SetFilter("Leads");
            btnFilterCustomers.Click += (_, _) => SetFilter("Customers");
            btnFilterProperties.Click += (_, _) => SetFilter("Properties");

            // Modern Grid Styling matching Nexa CRM
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
            grid.CellContentClick += Grid_CellContentClick;
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var item = GetItemAtRow(e.RowIndex);
                if (item is not null) OpenAssignDialog(item);
            };
        }

        private void SetFilter(string type)
        {
            _filterType = type;
            UpdateFilterPillStyles();
            ApplyFilterAndDisplay();
        }

        private void UpdateFilterPillStyles()
        {
            var pills = new[]
            {
                (btnFilterAll, "All"),
                (btnFilterLeads, "Leads"),
                (btnFilterCustomers, "Customers"),
                (btnFilterProperties, "Properties")
            };

            foreach (var (btn, name) in pills)
            {
                bool isSelected = string.Equals(_filterType, name, StringComparison.OrdinalIgnoreCase);
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

        public void RefreshGrid()
        {
            _allItems.Clear();

            // 1. Pending Leads
            var leads = _leadController.GetPendingReview();
            foreach (var lead in leads)
            {
                string submitter = GetUserName(lead.CreatedByUserId);
                string assigned = _leadController.GetAssignedAgentName(lead.AssignedAgentId) ?? "Unassigned";

                _allItems.Add(new PendingApprovalItem
                {
                    Id = lead.LeadId,
                    Type = "Lead",
                    Title = lead.FullName,
                    SubmitterName = submitter,
                    CreatedAt = lead.CreatedAt,
                    AssignedTo = assigned,
                    AssignedAgentId = lead.AssignedAgentId,
                    Status = "PENDING REVIEW",
                    OriginalEntity = lead
                });
            }

            // 2. Pending Customers
            var customers = _customerController.GetPendingReview();
            foreach (var cust in customers)
            {
                string submitter = GetUserName(cust.CreatedByUserId);
                string assigned = _customerController.GetAssignedAgentName(cust.AssignedAgentId) ?? "Unassigned";

                _allItems.Add(new PendingApprovalItem
                {
                    Id = cust.CustomerId,
                    Type = "Customer",
                    Title = cust.FullName,
                    SubmitterName = submitter,
                    CreatedAt = cust.CreatedAt,
                    AssignedTo = assigned,
                    AssignedAgentId = cust.AssignedAgentId,
                    Status = "PENDING REVIEW",
                    OriginalEntity = cust
                });
            }

            // 3. Pending Properties
            var properties = _propertyController.GetPendingReview();
            foreach (var prop in properties)
            {
                string submitter = GetUserName(prop.CreatedByUserId);
                string assigned = _propertyController.GetListedAgentName(prop.ListedByAgentId) ?? "Unassigned";

                _allItems.Add(new PendingApprovalItem
                {
                    Id = prop.PropertyId,
                    Type = "Property",
                    Title = prop.Address,
                    SubmitterName = submitter,
                    CreatedAt = prop.CreatedAt,
                    AssignedTo = assigned,
                    AssignedAgentId = prop.ListedByAgentId,
                    Status = "PENDING REVIEW",
                    OriginalEntity = prop
                });
            }

            _allItems = _allItems.OrderByDescending(x => x.CreatedAt).ToList();

            // Update subtitle badge
            int total = _allItems.Count;
            int leadCount = _allItems.Count(x => x.Type == "Lead");
            int custCount = _allItems.Count(x => x.Type == "Customer");
            int propCount = _allItems.Count(x => x.Type == "Property");
            lblSubtitle.Text = $"{total} items pending review · {leadCount} leads, {custCount} customers, {propCount} properties";

            ApplyFilterAndDisplay();
        }

        private void ApplyFilterAndDisplay()
        {
            grid.Columns.Clear();

            IEnumerable<PendingApprovalItem> query = _allItems;

            if (string.Equals(_filterType, "Leads", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Type == "Lead");
            }
            else if (string.Equals(_filterType, "Customers", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Type == "Customer");
            }
            else if (string.Equals(_filterType, "Properties", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Type == "Property");
            }

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.SubmitterName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.AssignedTo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.Type.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var displayList = query.Select(x => new
            {
                x.Id,
                x.Type,
                Title = x.Title,
                SubmittedBy = x.SubmitterName,
                DateSubmitted = x.CreatedAt.ToString("MMM dd, yyyy"),
                x.AssignedTo,
                x.Status
            }).ToList();

            grid.DataSource = displayList;

            var idCol = grid.Columns["Id"];
            if (idCol is not null) idCol.Visible = false;

            if (grid.Columns["Type"] is DataGridViewColumn typeCol)
            {
                typeCol.HeaderText = "TYPE";
                typeCol.FillWeight = 85;
            }
            if (grid.Columns["Title"] is DataGridViewColumn titleCol)
            {
                titleCol.HeaderText = "TITLE / RECORD";
                titleCol.FillWeight = 190;
            }
            if (grid.Columns["SubmittedBy"] is DataGridViewColumn subCol)
            {
                subCol.HeaderText = "SUBMITTED BY";
                subCol.FillWeight = 115;
            }
            if (grid.Columns["DateSubmitted"] is DataGridViewColumn dateCol)
            {
                dateCol.HeaderText = "DATE SUBMITTED";
                dateCol.FillWeight = 95;
            }
            if (grid.Columns["AssignedTo"] is DataGridViewColumn assignCol)
            {
                assignCol.HeaderText = "ASSIGNED AGENT";
                assignCol.FillWeight = 120;
            }
            if (grid.Columns["Status"] is DataGridViewColumn statusCol)
            {
                statusCol.HeaderText = "STATUS";
                statusCol.FillWeight = 100;
            }

            grid.Columns.Add(new ActionsColumn());
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics is null) return;

            // Type Badge Column
            if (grid.Columns[e.ColumnIndex].Name == "Type" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string type = e.Value.ToString() ?? "";

                Color bgColor;
                Color textColor;

                switch (type)
                {
                    case "Lead":
                        bgColor = Color.FromArgb(224, 242, 254);
                        textColor = Color.FromArgb(3, 105, 161);
                        break;
                    case "Customer":
                        bgColor = Color.FromArgb(220, 252, 231);
                        textColor = Color.FromArgb(21, 128, 61);
                        break;
                    case "Property":
                        bgColor = Color.FromArgb(243, 232, 255);
                        textColor = Color.FromArgb(126, 34, 206);
                        break;
                    default:
                        bgColor = Color.FromArgb(241, 245, 249);
                        textColor = Color.FromArgb(71, 85, 105);
                        break;
                }

                DrawPillBadge(e.Graphics, e.CellBounds, type, bgColor, textColor);
                e.Handled = true;
            }
            // Status Badge Column
            else if (grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value.ToString() ?? "";

                Color bgColor = Color.FromArgb(254, 243, 199);
                Color textColor = Color.FromArgb(180, 83, 9);

                DrawPillBadge(e.Graphics, e.CellBounds, status, bgColor, textColor);
                e.Handled = true;
            }
            // AssignedTo styling: italic gray if Unassigned
            else if (grid.Columns[e.ColumnIndex].Name == "AssignedTo" && e.Value != null)
            {
                string assigned = e.Value.ToString() ?? "";
                if (string.Equals(assigned, "Unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    e.PaintBackground(e.CellBounds, true);
                    using var italicFont = new Font("Segoe UI", 9.5f, FontStyle.Italic);
                    TextRenderer.DrawText(e.Graphics, "Unassigned", italicFont, e.CellBounds, Color.FromArgb(148, 163, 184),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
            }
        }

        private void DrawPillBadge(Graphics g, Rectangle bounds, string text, Color bgColor, Color textColor)
        {
            using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var size = TextRenderer.MeasureText(text, font);
            int pillWidth = size.Width + 16;
            int pillHeight = 22;
            int pillX = bounds.X + (bounds.Width - pillWidth) / 2;
            int pillY = bounds.Y + (bounds.Height - pillHeight) / 2;
            var pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);

            using var brush = new SolidBrush(bgColor);
            using var path = GetRoundedRectangle(pillRect, 8);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(brush, path);

            TextRenderer.DrawText(g, text, font, pillRect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex] is not ActionsColumn) return;

            grid.Rows[e.RowIndex].Selected = true;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            var item = GetItemAtRow(e.RowIndex);
            if (item is null) return;

            var menu = new ContextMenuStrip
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };

            var assignItem = new ToolStripMenuItem("Assign Agent...");
            assignItem.Click += (_, _) => OpenAssignDialog(item);
            menu.Items.Add(assignItem);

            var approveItem = new ToolStripMenuItem("Approve Immediately");
            approveItem.Click += (_, _) => ApproveItemImmediately(item);
            menu.Items.Add(approveItem);

            menu.Items.Add(new ToolStripSeparator());

            var detailsItem = new ToolStripMenuItem("View Details");
            detailsItem.Click += (_, _) => OpenDetailsForm(item);
            menu.Items.Add(detailsItem);

            menu.Show(grid, grid.PointToClient(Cursor.Position));
        }

        private void OpenAssignDialog(PendingApprovalItem item)
        {
            var agents = _leadController.GetAgents();
            using var dlg = new AssignAgentDialog(item.Title, agents, item.AssignedAgentId);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (item.Type == "Lead" && item.OriginalEntity is Lead lead)
                {
                    _leadController.AssignAgent(lead, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                }
                else if (item.Type == "Customer" && item.OriginalEntity is Customer cust)
                {
                    _customerController.AssignAgent(cust, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                }
                else if (item.Type == "Property" && item.OriginalEntity is Property prop)
                {
                    _propertyController.AssignAgent(prop, dlg.SelectedAgentId, dlg.ApproveNow, dlg.ReviewNotes);
                }

                MessageBox.Show(
                    $"Record '{item.Title}' has been assigned successfully.",
                    "Assignment Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                RefreshGrid();
            }
        }

        private void ApproveItemImmediately(PendingApprovalItem item)
        {
            if (item.Type == "Lead" && item.OriginalEntity is Lead lead)
            {
                _leadController.ApproveAssignment(lead, "Approved directly from Approvals Center.");
            }
            else if (item.Type == "Customer" && item.OriginalEntity is Customer cust)
            {
                _customerController.ApproveAssignment(cust, "Approved directly from Approvals Center.");
            }
            else if (item.Type == "Property" && item.OriginalEntity is Property prop)
            {
                _propertyController.ApproveAssignment(prop, "Approved directly from Approvals Center.");
            }

            MessageBox.Show(
                $"Submission for '{item.Title}' has been approved.",
                "Approved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            RefreshGrid();
        }

        private void OpenDetailsForm(PendingApprovalItem item)
        {
            if (item.Type == "Lead" && item.OriginalEntity is Lead lead)
            {
                using var form = new LeadDetailForm(lead, _leadController);
                form.ShowDialog();
            }
            else if (item.Type == "Customer" && item.OriginalEntity is Customer cust)
            {
                using var form = new CustomerDetailForm(cust, _customerController);
                form.ShowDialog();
            }
            else if (item.Type == "Property" && item.OriginalEntity is Property prop)
            {
                using var form = new PropertyDetailForm(prop, _propertyController);
                form.ShowDialog();
            }

            RefreshGrid();
        }

        private PendingApprovalItem? GetItemAtRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return null;
            if (grid.Rows[rowIndex].Cells["Id"].Value is int id &&
                grid.Rows[rowIndex].Cells["Type"].Value is string type)
            {
                return _allItems.FirstOrDefault(x => x.Id == id && x.Type == type);
            }
            return null;
        }

        private string GetUserName(int? userId)
        {
            if (!userId.HasValue || userId.Value <= 0) return "—";
            return _customerController.GetAssignedAgentName(userId.Value) ?? $"User #{userId.Value}";
        }
    }

    public class PendingApprovalItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string SubmitterName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string AssignedTo { get; set; } = "Unassigned";
        public int? AssignedAgentId { get; set; }
        public string Status { get; set; } = "PENDING REVIEW";
        public object OriginalEntity { get; set; } = null!;
    }
}
