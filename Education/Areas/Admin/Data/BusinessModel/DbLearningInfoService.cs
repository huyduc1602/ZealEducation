using Education.Areas.Admin.Data.BusinessModel.Interface;
using Education.Areas.Admin.Data.DataModel;
using Education.BLL;
using Education.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.BusinessModel
{
    public class DbLearningInfoService : ILearningInfoService
    {
        private IRepository<LearningInfo> learningInfoRepository;
        public DbLearningInfoService()
        {
            learningInfoRepository = new DbRepository<LearningInfo>();
        }
        public LearningInfo convertEditLearningInfo(LearningInfoModel model)
        {
            LearningInfo learning = learningInfoRepository.FindById(model.Id);
            learning.TuitionPaid = model.TuitionPaid;
            learning.Tuition = model.Tuition;
            learning.Point = model.Point;
            learning.RoomId = model.BatchId;
            learning.Number = model.Number;
            learning.Status = model.Status == 0 ? false : true;
            learning.CandicateId = model.CandicateId;
            learning.ExamId = model.ExamId;
            learning.RoomId = model.BatchId;
            learning.UpdatedAt = DateTime.Today;
            return learning;
        }

        public LearningInfo convertLearningInfo(LearningInfoModel model)
        {
            LearningInfo learning = new LearningInfo
            {
                TuitionPaid = model.TuitionPaid,
                Tuition = model.Tuition,
                Point = model.Point,
                Number = model.Number,
                Status = model.Status == 0 ? false : true,
                CandicateId = model.CandicateId,
                ExamId = model.ExamId,
                RoomId = model.BatchId,
                CreatedAt = DateTime.Today
            };
            return learning;
        }

        public LearningInfoModel convertLearningModel(LearningInfo learning)
        {
            LearningInfoModel infoModel = new LearningInfoModel
            {
                Id = learning.Id,
                TuitionPaid = learning.TuitionPaid,
                Tuition = learning.Tuition,
                ExamId = learning.ExamId,
                BatchId = learning.RoomId,
                CandicateId = learning.CandicateId,
                Number = learning.Number,
                Point = learning.Point,
                Status = learning.Status ? 1 : 2,
            };
            return infoModel;
           
        }
    }
}