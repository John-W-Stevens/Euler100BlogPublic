using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EulerBlog.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "GitHub Link")]
        public string CodeLink { get; set; }

        public string PythonCode { get; set; }

        public string JavaScriptCode { get; set; }

        public string CSharpCode { get; set; }

        public List<ModuleAssociation> EulerPosts { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
