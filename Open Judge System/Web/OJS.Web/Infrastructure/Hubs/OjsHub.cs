namespace OJS.Web.Infrastructure.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.AspNet.SignalR;
    using OJS.Common.SignalR;
    using OJS.Services.Data.Submissions;
    using SimpleInjector.Lifestyles;

    //using SimpleInjector.Lifestyles;

    public class OjsHub : Hub
    {
        //private readonly ISubmissionsDataService sds;
        private readonly SimpleInjector.Container container;

        public OjsHub(SimpleInjector.Container container)
        {
            this.container = container;
        }

        public override Task OnConnected()
        {
            this.Clients.All.addNewMessageToPage(this.Context.ConnectionId, $"Client connected: {this.Context.User.Identity.Name}");

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this.Clients.All.addNewMessageToPage(this.Context.ConnectionId, $"Client disconnected: {this.Context.User.Identity.Name}");

            return base.OnDisconnected(stopCalled);
        }

        public void ReportSubmissionStatus(SrServerNotification packet)
        {
            var rid = (string)this.Context.Request.Environment["server.RemoteIpAddress"];

            if (rid != "127.0.0.1" && rid != "localhost" && rid != "::1")
            {
                throw new HubException($"Forbidden for {rid}!");
            }

            if (packet.Type == SrServerNotificationType.FinishedProcessing)
            {
                using (AsyncScopedLifestyle.BeginScope(this.container))
                {
                    ISubmissionsDataService sds = this.container.GetInstance<ISubmissionsDataService>();
                    var tmp = sds.GetById(packet.SubmissionId);

                    this.Clients.User(tmp.Participant.User.UserName).refreshGrid(0, tmp.ProblemId, tmp.Id);
                }
            }
        }

        public void Hello()
        {
            this.Clients.All.hello();
        }

        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            this.Clients.All.addNewMessageToPage(name, message);
        }
    }
}