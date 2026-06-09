using System.ComponentModel.DataAnnotations.Schema;

namespace todostodo.api.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public EntryStatus Status { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
    }
}