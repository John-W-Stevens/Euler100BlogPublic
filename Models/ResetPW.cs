using System;
using System.ComponentModel.DataAnnotations;

namespace EulerBlog.Models
{
    public class ResetPW
    {
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }

    }
}

