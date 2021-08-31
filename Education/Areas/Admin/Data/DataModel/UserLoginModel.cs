using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class UserLoginModel
    {
        [Required(ErrorMessage = "User Name is not left blank")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is not left blank")]
        public string Password { get; set; }
    }
}