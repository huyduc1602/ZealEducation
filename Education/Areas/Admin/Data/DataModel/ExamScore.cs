using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class ExamScore
    {
        public string CandicateCode { get; set; }
        public string CandicateName { get; set; }
        public double Score { get; set; }
        public string Note { get; set; }
    }
}