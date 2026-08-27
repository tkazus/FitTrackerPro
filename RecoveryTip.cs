using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrackerPro.Models
{
    public class RecoveryTip
    {
        public int Id { get; set; }

        public int MuscleGroupId { get; set; }

        public string Recommendation { get; set; }

        public string Foods { get; set; }

        [ForeignKey("MuscleGroupId")]
        public virtual MuscleGroup MuscleGroup { get; set; }
    }
}