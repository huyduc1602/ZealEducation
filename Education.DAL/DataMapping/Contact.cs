using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Education.DAL
{
    public class Contact
    {
        [Required(ErrorMessage = "Name is not left blank")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is not left blank")]
        [RegularExpression("^[\\w-.]+@([\\w-]+.)+[\\w-]{2,4}$", ErrorMessage = "Email invalidate")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Subject is not left blank")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Message is not left blank")]
        [MaxLength(500)]
        public string Message { get; set; }
        [Required(ErrorMessage = "You have not accepted the terms and conditions")]
        public bool EAgree { get; set; }
    }
}
