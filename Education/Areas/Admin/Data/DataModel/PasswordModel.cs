using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class PasswordModel
    {
        [Required(ErrorMessage = "User Id is not left blank")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Password is not left blank")]
        [RegularExpression("^[a-zA-Z]([a-zA-Z0-9]){4,14}$", ErrorMessage = "Enter the password that does not contain special characters, lengths from 5 to 15 characters!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Repeat Password is not left blank")]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        [RegularExpression("^[a-zA-Z]([a-zA-Z0-9]){4,14}$", ErrorMessage = "Enter the password that does not contain special characters, lengths from 5 to 15 characters!")]
        public string RepeatPassword { get; set; }
    }
}