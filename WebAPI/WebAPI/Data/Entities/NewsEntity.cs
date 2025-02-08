﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Entities
{
    [Table("news_entities")] //  назва таблиці
    public class NewsEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(400)]
        public string Description { get; set; } = string.Empty; // Короткий анонс

        [Required] // Повний текст новини (може бути у форматі HTML)
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Author { get; set; } = string.Empty;

        public string? Tags { get; set; }

        public bool IsPublished { get; set; } = true;

        public int Views { get; set; } = 0;

        public string? Source { get; set; }

        [MaxLength(255)]
        public string? Slug { get; set; }
    }
}
