using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrackerPro.Models
{
    public class PersonalRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; } 
        public int ExerciseId { get; set; }
        public decimal MaxWeight { get; set; }
        public int MaxReps { get; set; }
        public DateTime RecordDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ExerciseId")]
        public virtual Exercise Exercise { get; set; }
    }
}