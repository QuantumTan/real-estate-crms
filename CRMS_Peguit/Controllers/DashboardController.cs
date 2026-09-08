using System;
using System.Linq;
using CRMS_Peguit.winforms.Models.ViewModels;

namespace CRMS_Peguit.winforms.Controllers
{
    public class DashboardController : IDisposable
    {
        private readonly CustomerController _customerController;
        private readonly PropertyController _propertyController;
        private readonly LeadController _leadController;
        private readonly DealController _dealController;

        public DashboardController()
        {
            _customerController = new CustomerController();
            _propertyController = new PropertyController();
            _leadController = new LeadController();
            _dealController = new DealController();
        }

        public DashboardSummaryViewModel GetSummary()
        {
            var customers = _customerController.GetAll();
            var properties = _propertyController.GetAll();
            var leads = _leadController.GetAll();
            var deals = _dealController.GetAll();
            var agents = _propertyController.GetAgents();

            int activeProps = properties.Count(p => string.Equals(p.Status, "available", StringComparison.OrdinalIgnoreCase));
            int qualifiedLeads = leads.Count(l => string.Equals(l.Stage, "qualified", StringComparison.OrdinalIgnoreCase));
            decimal pipelineSum = deals.Sum(d => d.Value);

            var recentLeads = leads
                .Take(10)
                .Select(l => new RecentLeadItemViewModel
                {
                    Lead = l.FullName,
                    Email = string.IsNullOrWhiteSpace(l.Email) ? "-" : l.Email,
                    Source = string.IsNullOrWhiteSpace(l.Source) ? "Website" : l.Source,
                    Value = l.ExpectedValue.HasValue ? $"${l.ExpectedValue.Value:N0}" : "-",
                    Stage = (l.Stage ?? string.Empty).ToUpperInvariant()
                })
                .ToList();

            return new DashboardSummaryViewModel
            {
                TotalCustomers = customers.Count,
                ActiveProperties = activeProps,
                QualifiedLeads = qualifiedLeads,
                TotalDeals = deals.Count,
                PipelineValue = pipelineSum,
                TotalAgents = agents.Count,
                RecentLeads = recentLeads
            };
        }

        public void Dispose()
        {
            _customerController.Dispose();
            _propertyController.Dispose();
            _leadController.Dispose();
            _dealController.Dispose();
        }
    }
}
