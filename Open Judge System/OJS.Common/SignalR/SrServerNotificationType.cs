using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OJS.Common.SignalR
{
    public enum SrServerNotificationType
    {
        System = 1,
        StartedProcessing = 2,
        FinishedProcessing = 3,
        Message = 4
    }
}
