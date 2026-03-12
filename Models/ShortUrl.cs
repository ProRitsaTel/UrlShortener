using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }

        [Required]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        public string ShortCode { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public int ClickCount { get; set; }
    }
}