using System;
using System.ComponentModel.DataAnnotations; 

namespace EDzienik.Models 
{
    public class ScheduleSlotViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Dzień tygodnia")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [Display(Name = "Rozpoczęcie")]
        public DateTime StartTime { get; set; } 

        [Required]
        [Display(Name = "Zakończenie")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Sala")]
        public string Room { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Klasa")]
        public int SchoolClassId { get; set; }

        [Required]
        [Display(Name = "Przedmiot")]
        public int SubjectId { get; set; }

    }
}