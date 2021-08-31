using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class LearningInfoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tuition Paid")]
        public double? TuitionPaid { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double? Tuition { get; set; }
        [Range(1, 100, ErrorMessage = "Enter the test score from 1 to 100")]
        public int? Point { get; set; }
        [Range(1, 10, ErrorMessage = "Enter the number of exam times from 1 to 10")]
        public int? Number { get; set; }
        [Required(ErrorMessage = "You must choose the Status")]
        public int Status { get; set; }
        public int? ExamId { get; set; }
        public int? BatchId { get; set; }
        [Display(Name = "Class Room")]
        public string BatchName { get; set; }
        [Display(Name = "Candicate")]
        public string CandicateName { get; set; }
        [Required(ErrorMessage = "Candicate is not left blank")]
        public int CandicateId { get; set; }
    }
}