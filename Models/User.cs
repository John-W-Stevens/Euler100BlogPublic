using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EulerBlog.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public Boolean IsConfirmed { get; set; } = false;
        public string PasswordReset { get; set; }
        public string EmailConfirmation { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        public string Password { get; set; }

        public Boolean IsAdmin { get; set; }

        public List<EulerPost> EulerPosts { get; set; }
        public List<Comment> Comments { get; set; }
        // One-To-Many (One-side) nav property goes here <<

        // One-To-Many (Many-side) nav property goes here <<

        // Many-To-Many nav property goes here <<

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Will not be mapped to your users table!
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }
    }
}

