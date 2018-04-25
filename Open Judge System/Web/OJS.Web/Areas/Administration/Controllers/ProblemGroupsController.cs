﻿namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.ProblemGroups;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ProblemGroup;
    using OJS.Web.Common.Extensions;

    using DetailViewModelType = OJS.Web.Areas.Administration.ViewModels.Problem.ProblemViewModel;
    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.ProblemGroups.ProblemGroupsControllers;

    [RouteArea(GlobalConstants.AdministrationAreaName, AreaPrefix = GlobalConstants.AdministrationAreaName)]
    [RoutePrefix("ProblemGroups")]
    public class ProblemGroupsController : LecturerBaseGridController
    {
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemGroupsBusinessService problemGroupsBusiness;
        private readonly IContestsDataService contestsData;

        public ProblemGroupsController(
            IOjsData data,
            IProblemGroupsDataService problemGroupsData,
            IProblemGroupsBusinessService problemGroupsBusiness,
            IContestsDataService contestsData)
            : base(data)
        {
            this.problemGroupsData = problemGroupsData;
            this.problemGroupsBusiness = problemGroupsBusiness;
            this.contestsData = contestsData;
        }

        public override IEnumerable GetData() =>
            this.problemGroupsData
                .GetAll()
                .Select(ProblemGroupAdministrationViewModel.FromProblemGroup);

        public override object GetById(object id) => this.GetByIdAsNoTracking((int)id);

        public override string GetEntityKeyName() => this.GetEntityKeyNameByType(typeof(ProblemGroup));

        public ActionResult Index() => this.View();

        [Route("Contest/{contestId:int}")]
        public ActionResult Index(int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ContestsController>(
                    c => c.Index(),
                    new { area = GlobalConstants.AdministrationAreaName });
            }

            this.ViewBag.ContestId = contestId;

            return this.View();
        }

        [HttpPost]
        public ActionResult Create(
            [DataSourceRequest] DataSourceRequest request,
            ProblemGroupAdministrationViewModel model)
        {
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            if (!this.contestsData.IsOnlineById(model.ContestId))
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    string.Format(Resource.Can_create_only_in_online_contest, nameof(ContestType.OnlinePracticalExam)));

                return this.GridOperation(request, model);
            }

            if (this.contestsData.IsActiveById(model.ContestId))
            {
                this.ModelState.AddModelError(string.Empty, Resource.Active_contest_cannot_add_problem_group);
                return this.GridOperation(request, model);
            }

            model.Id = (int)this.BaseCreate(model.GetEntityModel());
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, ProblemGroupAdministrationViewModel model)
        {
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            if (!this.contestsData.IsOnlineById(model.ContestId))
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    string.Format(Resource.Can_edit_only_in_online_contest, nameof(ContestType.OnlinePracticalExam)));

                return this.GridOperation(request, model);
            }

            var problemGroup = model.GetEntityModel(this.GetByIdAsNoTracking(model.Id));

            this.BaseUpdate(problemGroup);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, ProblemGroupAdministrationViewModel model)
        {
            if (!this.IsModelAndContestValid(model))
            {
                return this.GridOperation(request, model);
            }

            if (this.contestsData.IsActiveById(model.ContestId))
            {
                this.ModelState.AddModelError(string.Empty, Resource.Active_contest_cannot_delete_problem_group);
                return this.GridOperation(request, model);
            }

            var result = this.problemGroupsBusiness.DeleteById(model.Id);

            if (result.IsError)
            {
                this.ModelState.AddModelError(string.Empty, result.Error);
            }

            return this.GridOperation(request, model);
        }

        [HttpPost]
        public JsonResult GetProblemsInProblemGroup([DataSourceRequest]DataSourceRequest request, int id)
        {
            var problems = this.problemGroupsData
                .GetProblemsById(id)
                .Select(DetailViewModelType.FromProblem);

            return this.Json(problems.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private bool IsModelAndContestValid(ProblemGroupAdministrationViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ContestName))
            {
                this.ModelState.AddModelError(nameof(model.ContestName), Resource.Contest_required);
                return false;
            }

            if (!this.contestsData.ExistsById(model.ContestId))
            {
                this.ModelState.AddModelError(nameof(model.ContestName), Resource.Contest_does_not_exist);
                return false;
            }

            if (!this.CheckIfUserHasContestPermissions(model.ContestId))
            {
                this.ModelState.AddModelError(string.Empty, GeneralResource.No_privileges_message);
            }

            return this.ModelState.IsValid;
        }

        private ProblemGroup GetByIdAsNoTracking(int id) =>
            this.problemGroupsData.GetByIdQuery(id).AsNoTracking().SingleOrDefault();
    }
}