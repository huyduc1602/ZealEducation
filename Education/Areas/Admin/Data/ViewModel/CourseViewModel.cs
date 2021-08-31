using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.ViewModel
{
    public class CourseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StudyTime { get; set; }
        public string Code { get; set; }
        public double Price { get; set; }
        public double SalePrice { get; set; }
        public string Detail { get; set; }
        public string Image { get; set; }
        public int MaximumCandicate { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}