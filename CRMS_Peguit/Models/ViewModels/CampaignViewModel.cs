using System.Collections.Generic;
using CRMS_Peguit.domain.entities;

namespace CRMS_Peguit.winforms.Models.ViewModels
{
    public class CampaignSummaryViewModel
    {
        public int TotalLeads { get; set; }
        public int ActiveChannelCount => Channels.Count;
        public string TopChannelText { get; set; } = "Top Channel: None";
        public List<string> Channels { get; set; } = new();
        public Dictionary<string, int> ChannelCounts { get; set; } = new();
        public List<Lead> FilteredLeads { get; set; } = new();
    }
}
