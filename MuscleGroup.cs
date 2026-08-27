using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitTrackerPro.Models
{
    public class MuscleGroup
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public virtual ICollection<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }

        public virtual ICollection<RecoveryTip> RecoveryTips { get; set; }

        public MuscleGroup()
        {
            ExerciseMuscleGroups = new List<ExerciseMuscleGroup>();
            RecoveryTips = new List<RecoveryTip>();
        }
    }
}