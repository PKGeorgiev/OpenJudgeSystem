﻿namespace OJS.Web.Areas.Api.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data.Models;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ExamGroups;
    using OJS.Web.Areas.Api.Models;

    public class ExamGroupsController : Controller
    {
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ExamGroupsController(
            IExamGroupsDataService examGroupsData,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.examGroupsData = examGroupsData;
            this.backgroundJobs = backgroundJobs;
        }

        public ActionResult AddUsersToExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var externalExamGroup = model.ExamGroupInfoModel;

            var examGroupExists = true;
            var examGroup = this.examGroupsData.GetByExternalIdAndAppId(externalExamGroup.Id, model.AppId);

            if (examGroup == null)
            {
                examGroupExists = false;
                examGroup = new ExamGroup
                {
                    ExternalExamGroupId = externalExamGroup.Id,
                    ExternalAppId = model.AppId
                };
            }

            var startTime = externalExamGroup.StartTime?.ToString("g") ?? string.Empty;

            examGroup.Name = $"{externalExamGroup.ExamName} => {externalExamGroup.ExamGroupTrainingLabName} | {startTime}";

            if (examGroupExists)
            {
                this.examGroupsData.Update(examGroup);
            }
            else
            {
                this.examGroupsData.Add(examGroup);
            }

            var examGroupId = this.examGroupsData
                .GetIdByExternalIdAndAppId(externalExamGroup.Id, model.AppId);

            if (examGroupId.HasValue)
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.AddUsersByIdAndUserIds(examGroupId.Value, model.UserIds));
            }

            return this.Json(true);
        }

        public ActionResult RemoveUsersFromExamGroup(UsersExamGroupModel model)
        {
            if (!this.IsUsersExamGroupModelValid(model))
            {
                return this.Json(false);
            }

            var examGroupId = this.examGroupsData
                .GetIdByExternalIdAndAppId(model.ExamGroupInfoModel.Id, model.AppId);

            if (examGroupId.HasValue)
            {
                this.backgroundJobs.AddFireAndForgetJob<IExamGroupsBusinessService>(
                    x => x.RemoveUsersByIdAndUserIds(examGroupId.Value, model.UserIds));
            }

            return this.Json(true);
        }

        private bool IsUsersExamGroupModelValid(UsersExamGroupModel model)
        {
            var hasUsers = model.UserIds?.Any();
            var hasExamGroupId = model.ExamGroupInfoModel.Id != default(int);
            var hasAppId = !string.IsNullOrWhiteSpace(model.AppId);
            var hasRequiredNames = !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamName) &&
                !string.IsNullOrWhiteSpace(model.ExamGroupInfoModel.ExamGroupTrainingLabName);

            var isModelValid = (hasUsers ?? false) && hasExamGroupId && hasAppId && hasRequiredNames;

            return isModelValid;
        }
    }
}