using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.News
{
    public class NewsItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public bool IsPublished { get; set; } = true;
        public int Views { get; set; } = 0;
        public string? Source { get; set; }
        public string? Slug { get; set; }
    }
}
