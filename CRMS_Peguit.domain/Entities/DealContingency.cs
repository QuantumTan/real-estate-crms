using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CRMS_Peguit.domain.entities
{
    /// <summary>
    /// Represents a real-estate closing condition / contingency precedent
    /// (e.g. Clean Title Check, Bank Loan Approval, Turnover Inspection)
    /// that must be resolved before a deal can close.
    /// </summary>
    public class DealContingency
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // "Pending", "Satisfied", "Waived", "Failed"
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Notes { get; set; }

        public static string SerializeList(List<DealContingency>? list)
        {
            if (list == null || list.Count == 0) return string.Empty;
            try
            {
                return JsonSerializer.Serialize(list);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<DealContingency> DeserializeList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<DealContingency>();
            try
            {
                return JsonSerializer.Deserialize<List<DealContingency>>(json) ?? new List<DealContingency>();
            }
            catch
            {
                return new List<DealContingency>();
            }
        }
    }
}
