using Habit_Tracker_Backend.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("habit_log")]
    public class HabitLog
    {
        [Key]
        [Column("log_id")]
        public long LogId { get; set; }

        [Required]
        [Column("habit_id")]
        public long HabitId { get; set; }

        [Required]
        [Column("log_date")]
        public DateOnly LogDate { get; set; }

        [Required]
        [Column("status")]
        public HabitLogStatus Status { get; set; }

        [MaxLength(255)]
        [Column("remarks")]
        public string? Remarks { get; set; }
        //[ForeignKey(nameof(HabitId))]
        //public Habit? Habit { get; set; }
    }
}
