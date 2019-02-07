using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using OJS.Common.Extensions;
using OJS.Common.SignalR;
using Serilog;

namespace OJS.Services.SignalR
{
    public class OjsHubService : IOjsHubService
    {
        private readonly HubConnection hubConnection;
        private readonly IHubProxy hubProxy;
        private readonly string hubUrl;
        private readonly ILogger logger;
        private readonly object syncRoot = new object();
        private Task connectTask;
        private CancellationTokenSource cts;

        public OjsHubService(string hubUrl, ILogger logger)
        {
            this.hubUrl = hubUrl;
            this.logger = logger;

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;

            this.hubConnection = new HubConnection(this.hubUrl);
            this.hubProxy = hubConnection.CreateHubProxy("OjsHub");

            this.hubConnection.Closed += TryToConnect;

            this.hubConnection.Reconnected += () =>
            {
                this.logger.Warning("Reconnected to hub {Hub}", hubUrl);
            };

        }

        protected void TryToConnect()
        {
            lock (syncRoot)
            {
                if (connectTask != null || this.cts.IsCancellationRequested == true)
                {
                    return;
                }

                connectTask = Task.Factory.StartNew(async () =>
                    {
                        while (hubConnection.State != ConnectionState.Connected && this.cts.IsCancellationRequested == false)
                        {
                            try
                            {
                                this.logger.Information("Connecting to hub {Hub}...", hubUrl);
                                await hubConnection.Start();
                                lock (syncRoot)
                                {
                                    this.connectTask = null;
                                }

                                this.logger.Information("Connected to hub {Hub}", hubUrl);
                            }
                            catch (Exception ex)
                            {
                                this.logger.Error("Unable to contact hub {Hub}...", hubUrl, ex);
                                try
                                {
                                    await Task.Delay(15000, this.cts.Token);
                                }
                                catch (OperationCanceledException)
                                {
                                    this.logger.Warning("Connection to hub {Hub} was cancelled!", hubUrl, ex);
                                }
                            }
                        }
                    });
            }
        }

        public void Start()
        {
            this.cts = new CancellationTokenSource();
            this.TryToConnect();
        }

        public void Stop()
        {
            this.cts.Cancel();
            this.logger.Information("Disconnecting from hub {Hub}...", hubUrl);
            this.hubConnection.Stop();
        }

        public void NotifyServer(SrServerNotification packet)
        {
            // TODO: allow this method only from localhost


            lock (syncRoot)
            {
                if (connectTask != null)
                {
                    this.logger.Warning("Not connected to {Hub}...", hubUrl);
                    return;
                }

            }

            try
            {
                this.logger.Information("Submission {Submission}: Reporting submission status using packet {@Packet}...", packet.SubmissionId, packet);

                // This will be executed async
                this.hubProxy.Invoke("ReportSubmissionStatus", packet);
            }
            catch (Exception ex)
            {
                // TODO
                this.logger.Error("Exception during hub method {HubMethod}", nameof(this.NotifyServer), ex);
            }

        }
    }
}
