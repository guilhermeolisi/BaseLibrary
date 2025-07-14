using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class ProjectNotificationEventArgs
    {
        public ProjectInfoBase Sender { get; private set; }
        public bool TitleChanged { get; set; }
        public string AlertMessage { get; set; }
        /// <summary>
        /// 0 Export, 1 Make a Copy, 2 Move, 3 Delete
        /// </summary>
        public ProjectNotificationEventArgs(ProjectInfoBase sender)
        {
            Sender = sender;
        }
    }
}
