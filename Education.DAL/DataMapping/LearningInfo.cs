using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Education.DAL
{
    public class LearningInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        [Display(Name = "Tuition Paid")]
        public double? TuitionPaid { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double? Tuition { get; set; }
        [Display(Name = "Test marks")]
        public int? Point { get; set; }
        [Display(Name = "Number of exam times")]
        public int? Number { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public bool Status { get; set; }
        public int? ExamId { get; set; }
        [ForeignKey("ExamId")]
        public Exam Exam { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public ClassRoom ClassRoom { get; set; }
        public int CandicateId { get; set; }
        [ForeignKey("CandicateId")]
        public Candicate Candicate { get; set; }
        [Display(Name = "Created At")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Updated At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? UpdatedAt { get; set; }
    }
}
