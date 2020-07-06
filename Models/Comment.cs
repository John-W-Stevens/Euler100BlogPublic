using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EulerBlog.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        [Display(Name = "Leave a comment")]
        public string Content { get; set; }

        // One-To-Many (One-side) nav property goes here <<

        public int UserId { get; set; }
        public User Author { get; set; }
        public int EulerPostId { get; set; }
        public EulerPost EulerPost { get; set; }
        // One-To-Many (Many-side) nav property goes here <<


        // Many-To-Many nav property goes here <<

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

