using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Education.Areas.Admin.Data.DataModel
{
    public class CourseModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Course name is not left blank")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Course slug is not left blank")]
        public string Slug { get; set; }
        [Required(ErrorMessage = "Study Time is not left blank")]
        [Display(Name = "Study Time (month)")]
        public int StudyTime { get; set; }
        [Display(Name = "Course Code")]
        [Required(ErrorMessage = "Course code is not left blank")]
        [RegularExpression("^[A-Z][A-Z0-9]{2,9}$", ErrorMessage = "Code starts with flower print, including flower printing and numbers. From 3 to 10 characters")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Price is not left blank")]
        public double Price { get; set; }
        [Required(ErrorMessage = "Sale Price is not left blank")]
        [Display(Name = "Sale Price")]
        public double SalePrice { get; set; }
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Detail { get; set; }
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessage = "The maximum number of candicates is not left blank")]
        [Display(Name = "Maximum Candicate")]
        public int MaximumCandicate { get; set; }
        public int? UserId { get; set; }
    }
}