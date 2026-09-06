using CRMS_Peguit.winforms.Controllers;

namespace CRMS_Peguit.winforms.Views.Shared
{
    public partial class AssignAgentDialog : Form
    {
        private readonly List<AgentPickerItem> _agents;

        public int? SelectedAgentId { get; private set; }
        public string? ReviewNotes { get; private set; }
        public bool ShouldApprove { get; private set; }
        public bool ApproveNow => ShouldApprove;

        public AssignAgentDialog() : this("Record", new List<AgentPickerItem>(), null)
        {
        }

        public AssignAgentDialog(string recordTitle, List<AgentPickerItem> agents, int? currentAgentId = null)
        {
            _agents = agents ?? new List<AgentPickerItem>();
            InitializeComponent();

            lblRecord.Text = $"Record: {recordTitle}";
            PopulateAgents(currentAgentId);

            btnSave.Click += BtnSaveClick;
        }

        private void PopulateAgents(int? currentAgentId)
        {
            cmbAgents.Items.Clear();
            cmbAgents.Items.Add("-- Unassigned --");

            AgentPickerItem? selected = null;
            foreach (var agent in _agents)
            {
                cmbAgents.Items.Add(agent);
                if (currentAgentId.HasValue && agent.UserId == currentAgentId.Value)
                {
                    selected = agent;
                }
            }

            if (selected != null)
            {
                cmbAgents.SelectedItem = selected;
            }
            else
            {
                cmbAgents.SelectedIndex = 0;
            }
        }

        private void BtnSaveClick(object? sender, EventArgs e)
        {
            if (cmbAgents.SelectedItem is AgentPickerItem agent)
            {
                SelectedAgentId = agent.UserId;
            }
            else
            {
                SelectedAgentId = null;
            }

            ReviewNotes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
            ShouldApprove = chkApprove.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
