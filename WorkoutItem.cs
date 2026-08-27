namespace FitTrackerPro.Models
{
    public class WorkoutItem
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public decimal Weight { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal TotalWeight => Weight * Sets * Reps;
    }
}