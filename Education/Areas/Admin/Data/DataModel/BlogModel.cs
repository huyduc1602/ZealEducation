using Education.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Education.Areas.Admin.Data.DataModel
{
    public class BlogModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Blog name is not left blank")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Blog slug is not left blank")]
        public string Slug { get; set; }
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessage = "Blog detail is not left blank")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Detail { get; set; }
        public string Sumary { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        [Required(ErrorMessage = "Blog tag is not left blank")]
        public string Tag { get; set; }
    }
}