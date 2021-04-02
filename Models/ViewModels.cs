using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiscussionForum.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email or Username")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class PostViewModel
    {
        //public IList<Comment> comments;
        public Comment postComment { get; set; }
        public Post post { get; set; }
    }
}