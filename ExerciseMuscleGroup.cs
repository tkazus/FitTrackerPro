using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrackerPro.Models
{
    public class ExerciseMuscleGroup
    {
        public int ExerciseId { get; set; }

        public int MuscleGroupId { get; set; }

        [ForeignKey("ExerciseId")]
        public virtual Exercise Exercise { get; set; }

        [ForeignKey("MuscleGroupId")]
        public virtual MuscleGroup MuscleGroup { get; set; }
    }
}