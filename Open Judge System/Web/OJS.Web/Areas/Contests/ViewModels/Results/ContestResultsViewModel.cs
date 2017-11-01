﻿namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;

    using X.PagedList;

    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ContestResultsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentPage { get; set; }
        
        public IEnumerable<ContestProblemSimpleViewModel> Problems { get; set; }

        public IPagedList<ParticipantResultViewModel> Results { get; set; }

        public bool ContestCanBeCompeted { get; set; }

        public bool ContestCanBePracticed { get; set; }

        public bool UserIsLecturerInContest { get; set; }
    }
}