using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("habit_categories")]
    public class HabitCategory
    {
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("category_name")]
        public string CategoryName { get; set; } = null!;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Habit> Habits { get; set; }
            = new List<Habit>();
    }
}
