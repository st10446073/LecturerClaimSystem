using System;

namespace LecturerClaimSystem.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = string.Empty;  // non-nullable
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";  // default status
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        // Make sure there is ONLY ONE DocumentPath property
        public string? DocumentPath { get; set; }
    }
}
