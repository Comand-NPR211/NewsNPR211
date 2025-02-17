using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.News
{
    public class NewsEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IFormFile? ImageUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public string? Slug { get; set; }
    }
}
