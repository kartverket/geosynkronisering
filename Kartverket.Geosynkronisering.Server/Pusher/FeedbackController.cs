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
            public string NewLogListItem;

            public event System.EventHandler UpdateLogListAsync;
            
            public async Task OnUpdateLogListAsync(string info)
            {
                if (UpdateLogListAsync == null) { return; }
                this.NewLogListItem = info;

                //UpdateLogList.BeginInvoke(this, new EventArgs(),
                //    new AsyncCallback(UpdateLogListCompleted), null);

                var workTask = System.Threading.Tasks.Task.Run(() => UpdateLogListAsync.Invoke(this, EventArgs.Empty));

                // We await the task instead of calling EndInvoke.
                await workTask;
            }

            public event EventHandler UpdateLogListSync;
            public void OnUpdateLogListSync(string info)
            {
                // Works best for Blazor Server
                if (UpdateLogListSync == null) { return; }
                this.NewLogListItem = info;
                UpdateLogListSync.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
