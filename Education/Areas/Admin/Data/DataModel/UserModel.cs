using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "User Name is not left blank")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "User Password is not left blank")]
        [RegularExpression("^[a-zA-Z]([a-zA-Z0-9]){4,14}$", ErrorMessage = "Enter the password that does not contain special characters, lengths from 5 to 15 characters!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "User Email is not left blank")]
        [RegularExpression("^[\\w-.]+@([\\w-]+.)+[\\w-]{2,4}$", ErrorMessage = "Email invalidate")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Full Name is not left blank")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Group User is not left blank")]
        public int GroupUserId { get; set; }
    }
}