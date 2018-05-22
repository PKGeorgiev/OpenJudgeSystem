﻿namespace OJS.Services.Data.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Models;

    public class ArchivedSubmissionsDataService : IArchivedSubmissionsDataService
    {
        private readonly IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions;

        public ArchivedSubmissionsDataService(
            IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions) =>
                this.archivedSubmissions = archivedSubmissions;


        public IQueryable<ArchivedSubmission> GetAllBySubmissionIds(IEnumerable<int> submissionIds) =>
            this.archivedSubmissions
                .All()
                .Where(s => submissionIds.Contains(s.Id));

        public void Add(IEnumerable<ArchivedSubmission> entities) =>
            this.archivedSubmissions.Add(entities);
    }
}