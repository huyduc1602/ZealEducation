using Education.Areas.Admin.Data.DataModel;
using Education.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.BusinessModel.Interface
{
    interface ILearningInfoService
    {
        LearningInfo convertLearningInfo(LearningInfoModel model);
        LearningInfoModel convertLearningModel(LearningInfo learning);
        LearningInfo convertEditLearningInfo(LearningInfoModel model);
    }
}