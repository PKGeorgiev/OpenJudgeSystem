namespace OJS.LocalWorker
{
    using System;

    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;

    internal class LocalWorkerService : LocalWorkerServiceBase<int>
    {
        protected override IDependencyContainer GetDependencyContainer() => Bootstrap.Container;

        protected override void BeforeStartingThreads()
        {
            try
            {
                EncryptionHelper.EncryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                this.Logger.Error("Cannot encrypt App.config", ex);
            }

            try
            {
                using (this.DependencyContainer.BeginDefaultScope())
                {
                    var submissionsForProcessingBusiness =
                        this.DependencyContainer.GetInstance<ISubmissionsForProcessingBusinessService>();

                    submissionsForProcessingBusiness.ResetAllProcessingSubmissions();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Resetting Processing submissions failed", ex);
                throw;
            }

            try
            {
                this.StartMonitoringService();
            }
            catch (Exception ex)
            {
                this.Logger.Error(
                    "An exception was thrown while attempting to start the {Service}", 
                    Constants.LocalWorkerMonitoringServiceName, 
                    ex);
            }

            base.BeforeStartingThreads();
        }

        protected override void BeforeAbortingThreads()
        {
            try
            {
                EncryptionHelper.DecryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                this.Logger.Error("Cannot decrypt App.config", ex);
            }

            base.BeforeAbortingThreads();
        }

        private void StartMonitoringService()
        {
            const string monitoringServiceName = Constants.LocalWorkerMonitoringServiceName;

            var serviceState = ServicesHelper.GetServiceState(monitoringServiceName);
            if (serviceState.Equals(ServiceState.Running))
            {
                this.Logger.Information("{Service} is running.", monitoringServiceName);
                return;
            }

            this.Logger.Information("Attempting to start the {Service}...", monitoringServiceName);

            if (serviceState.Equals(ServiceState.NotFound))
            {
                ServicesHelper.InstallService(monitoringServiceName, Settings.MonitoringServiceExecutablePath);
            }

            ServicesHelper.StartService(monitoringServiceName);

            this.Logger.Information("{Service} started successfully.", monitoringServiceName);
        }
    }
}