using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Education.Areas.Admin.Data.BusinessModel.Interface
{
    interface IUserService
    {
        string GetMD5(string Password);
    }
}
