using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.winforms.Models.ViewModels;

namespace CRMS_Peguit.winforms.Controllers
{
    public class CampaignController : IDisposable
    {
        private readonly LeadController _leadController;
        private readonly HashSet<string> _customSources = new(StringComparer.OrdinalIgnoreCase);

        public CampaignController()
        {
            _leadController = new LeadController();
        }

        public CampaignController(LeadController leadController)
        {
            _leadController = leadController;
        }

        public void AddCustomChannel(string channelName)
        {
            if (!string.IsNullOrWhiteSpace(channelName))
            {
                _customSources.Add(channelName.Trim());
            }
        }

        public CampaignSummaryViewModel GetCampaignSummary(string selectedChannel = "All")
        {
            var leads = _leadController.GetAll();

            var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Facebook Ad",
                "Referral",
                "Walk-in",
                "Website",
                "Property Portal"
            };

            foreach (var l in leads)
            {
                if (!string.IsNullOrWhiteSpace(l.Source))
                {
                    sources.Add(l.Source.Trim());
                }
            }

            foreach (var s in _customSources)
            {
                sources.Add(s);
            }

            var orderedChannels = sources.OrderBy(s => s).ToList();

            var channelCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in orderedChannels)
            {
                channelCounts[s] = leads.Count(l => string.Equals(l.Source?.Trim(), s, StringComparison.OrdinalIgnoreCase));
            }

            var topSource = leads
                .Where(l => !string.IsNullOrWhiteSpace(l.Source))
                .GroupBy(l => l.Source!.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            string topText = topSource != null
                ? $"Top Channel: {topSource.Key} ({topSource.Count()} leads)"
                : "Top Channel: None";

            var filtered = string.Equals(selectedChannel, "All", StringComparison.OrdinalIgnoreCase)
                ? leads
                : leads.Where(l => string.Equals(l.Source?.Trim(), selectedChannel, StringComparison.OrdinalIgnoreCase)).ToList();

            return new CampaignSummaryViewModel
            {
                TotalLeads = leads.Count,
                TopChannelText = topText,
                Channels = orderedChannels,
                ChannelCounts = channelCounts,
                FilteredLeads = filtered
            };
        }

        public void Dispose()
        {
            _leadController.Dispose();
        }
    }
}
