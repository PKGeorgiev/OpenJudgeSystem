using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OJS.Common.SignalR
{
    public class SrServerNotification
    {
        public SrServerNotificationType Type { get; set; }

        public int SubmissionId { get; set; }

        public string Message { get; set; }
    }
}
