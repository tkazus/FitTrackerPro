using System.ComponentModel.DataAnnotations;

namespace FitTrackerPro.Models
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Technique { get; set; }
        public string VideoPath { get; set; }
    }
}