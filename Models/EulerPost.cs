using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EulerBlog.Models
{
    public class EulerPost
    {
        [Key]
        public int EulerPostId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Problem #")]
        public int ProblemNumber { get; set; }

        [Required]
        [Display(Name = "Problem Url")]
        public string Url { get; set; }

        [Required]
        public string Problem { get; set; }

        [Required]
        public string Observations { get; set; }

        [Required]
        public string Analysis { get; set; }

        [Display(Name = "Python Code")]
        public string PythonCode { get; set; }

        [Display(Name = "JavaScript Code")]
        public string JavaScriptCode { get; set; }

        [Display(Name = "C# Code")]
        public string CSharpCode { get; set; }

        [Display(Name = "Code Link")]
        public string CodeLink { get; set; }

        [Display(Name = "Time Complexity")]
        public string TimeComplexity { get; set; }

        [Display(Name = "Space Complexity")]
        public string SpaceComplexity { get; set; }

        [Display(Name = "Big-O Complexity")]
        public string BigOComplexity { get; set; }

        [Display(Name = "Final Thoughts")]
        public string FinalThoughts { get; set; }

        [Display(Name = "Image Url")]
        public string ImageUrl { get; set; }

	public List<ModuleAssociation> Modules { get; set; }
        public List<Comment> Comments { get; set; }
        // One-To-Many (One-side) nav property goes here <<

        public int UserId { get; set; }
        public User Author { get; set; }
        // One-To-Many (Many-side) nav property goes here <<


        // Many-To-Many nav property goes here <<

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

