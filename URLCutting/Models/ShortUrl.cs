using System.ComponentModel.DataAnnotations;

namespace URLCutting.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }

        [Required]
        public string OriginalUrl { get; set; }

        public string ShortCode { get; set; }

        public int Clicks { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
