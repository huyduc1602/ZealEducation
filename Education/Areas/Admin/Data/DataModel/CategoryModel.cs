using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class CategoryModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Category Name is not left blank")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int? UserId { get; set; }
    }
}