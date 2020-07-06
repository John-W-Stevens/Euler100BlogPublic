using System;
using System.ComponentModel.DataAnnotations;

namespace EulerBlog.Models
{
    public class ModuleAssociation
    {
        [Key]
        public int ModuleAssociationId { get; set; }

        [Display(Name = "Euler Post")]
        public int EulerPostId { get; set; }
        [Display(Name = "Module")]
        public int ModuleId { get; set; }
        public EulerPost EulerPost { get; set; }
        public Module Module { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

