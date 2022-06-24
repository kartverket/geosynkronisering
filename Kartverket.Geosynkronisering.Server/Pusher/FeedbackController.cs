using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pusher
{
    public class FeedbackController
    {
        public class Progress
        {
            public event System.EventHandler UpdateLogList;
            public string NewLogListItem;
            public async Task OnUpdateLogList(string info)
            {
                if (UpdateLogList == null) { return; }
                this.NewLogListItem = info;

                //UpdateLogList.BeginInvoke(this, new EventArgs(),
                //    new AsyncCallback(UpdateLogListCompleted), null);

                var workTask = System.Threading.Tasks.Task.Run(() => UpdateLogList.Invoke(this, new EventArgs()));

                // We await the task instead of calling EndInvoke.
                await workTask;
                
            }

        }
    }
}
