using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRMS_Peguit.domain.entities
{
    public class DealContingency
    {
        public int DealContingencyId { get; set; }
        public int DealId { get; set; }
        public string ContingencyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsSatisfied { get; set; }
        public DateTime? SatisfiedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string Name
        {
            get => ContingencyName;
            set => ContingencyName = value ?? string.Empty;
        }

        [NotMapped]
        public string? Notes
        {
            get => Description;
            set => Description = value;
        }

        [NotMapped]
        public DateTime? ResolvedAt
        {
            get => SatisfiedAt;
            set
            {
                SatisfiedAt = value;
                if (value.HasValue) IsSatisfied = true;
            }
        }

        [NotMapped]
        public string Status
        {
            get => IsSatisfied ? "Satisfied" : "Pending";
            set => IsSatisfied = string.Equals(value, "Satisfied", StringComparison.OrdinalIgnoreCase);
        }

        [JsonIgnore]
        public virtual Deal? Deal { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static List<DealContingency> DeserializeList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<DealContingency>();
            try
            {
                return JsonSerializer.Deserialize<List<DealContingency>>(json, _jsonOptions) ?? new List<DealContingency>();
            }
            catch
            {
                return new List<DealContingency>();
            }
        }

        public static string SerializeList(IEnumerable<DealContingency> list)
        {
            if (list == null) return "[]";
            return JsonSerializer.Serialize(list, _jsonOptions);
        }
    }
}

