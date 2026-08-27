using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrackerPro.Models
{
    public class WorkoutSession
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } 

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime WorkoutDate { get; set; }
        public int TotalExercises { get; set; }
        public decimal TotalWeight { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<WorkoutExercise> Exercises { get; set; }

        public WorkoutSession()
        {
            Exercises = new List<WorkoutExercise>();
        }
    }
}