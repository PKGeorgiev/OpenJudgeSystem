using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OJS.Common.SignalR;

namespace OJS.Services.SignalR
{
    public interface IOjsHubService
    {
        void Start();
        void Stop();
        void NotifyServer(SrServerNotification packet);
    }
}
