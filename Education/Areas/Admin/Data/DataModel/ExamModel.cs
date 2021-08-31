using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class ExamModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Exam name is not left blank")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Exam Code is not left blank")]
        [RegularExpression("^[A-Z][A-Z0-9]{2,9}$", ErrorMessage = "Code starts with flower print, including flower printing and numbers. From 3 to 10 characters")]
        public string Code { get; set; }
        public string Note { get; set; }
        [Required(ErrorMessage = "Exam date is not left blank")]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }
        [Required(ErrorMessage = "Exam start time is not left blank")]
        [Display(Name = "Start Time")]
        [RegularExpression("^[0-9]{2}:[0-9]{2}$", ErrorMessage = "The exam start time is in the wrong format")]
        public string StartTime { get; set; }
        [Required(ErrorMessage = "Exam end time is not left blank")]
        [Display(Name = "End Time")]
        [RegularExpression("^[0-9]{2}:[0-9]{2}$", ErrorMessage = "The exam end time is in the wrong format")]
        public string EndTime { get; set; }
        public int? UserId { get; set; }
    }
}