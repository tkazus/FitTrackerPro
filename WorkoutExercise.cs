using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrackerPro.Models
{
    public class WorkoutExercise
    {
        [Key]
        public int Id { get; set; }

        public int WorkoutSessionId { get; set; }
        public int ExerciseId { get; set; }
        public decimal Weight { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }

        [ForeignKey("WorkoutSessionId")]
        public virtual WorkoutSession WorkoutSession { get; set; }

        [ForeignKey("ExerciseId")]
        public virtual Exercise Exercise { get; set; }
    }
}