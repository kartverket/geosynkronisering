using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class FeedbackController
    {

        public class Progress
        {
            public event System.EventHandler NewSynchMilestoneReached;
            public event System.EventHandler UpdateLogList;
            public event System.EventHandler OrderProcessingStart;
            public event System.EventHandler OrderProcessingChange;

            public string MilestoneDescription;
            public string NewLogListItem;
            public long TotalNumberOfOrders;
            public int OrdersProcessedCount;

            // NewSynchMilestoneReached
            public async System.Threading.Tasks.Task OnNewSynchMilestoneReachedAsync(string description)
            {
                if (NewSynchMilestoneReached == null) { return; }
                this.MilestoneDescription = description;

                //if (false)
                //{
                //    NewSynchMilestoneReached.BeginInvoke(this, new EventArgs(),
                //        new AsyncCallback(NewSynchMilestoneReachedCompleted), null);
                //}


                // Migrating Delegate.BeginInvoke Calls for .NET Core, works OK for .NET framework too.
                // Seee: https://devblogs.microsoft.com/dotnet/migrating-delegate-begininvoke-calls-for-net-core/
                // Schedule the work using a Task and 
                // NewSynchMilestoneReached.Invoke instead of NewSynchMilestoneReached.BeginInvoke.
                //Console.WriteLine("Starting with Task.Run");
                var workTask = System.Threading.Tasks.Task.Run(() => NewSynchMilestoneReached.Invoke(this, new EventArgs()));

                // We await the task instead of calling EndInvoke.
                await workTask;

            }

            //private void NewSynchMilestoneReachedCompleted(IAsyncResult ar)
            //{
            //    if (NewSynchMilestoneReached == null) { return; }
            //    NewSynchMilestoneReached.EndInvoke(ar);
            //}

            // UpdateLogList
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

            //private void UpdateLogListCompleted(IAsyncResult ar)
            //{
            //    if (UpdateLogList == null) { return; }
            //    UpdateLogList.EndInvoke(ar);
            //}

            // OrderProcessingStart
            public async Task OnOrderProcessingStart(long totalNumberOfOrders)
            {
                if (OrderProcessingStart == null) { return; }
                this.TotalNumberOfOrders = totalNumberOfOrders;
                this.OrdersProcessedCount = 0;

                //if (false)
                //{
                //    OrderProcessingStart.BeginInvoke(this, new EventArgs(),
                //        new AsyncCallback(OrderProcessingStartCompleted), null);
                //}

                var workTask = System.Threading.Tasks.Task.Run(() => OrderProcessingStart.Invoke(this, new EventArgs()));
                // We await the task instead of calling EndInvoke.
                await workTask;
            }

            //private void OrderProcessingStartCompleted(IAsyncResult ar)
            //{
            //    if (OrderProcessingStart == null) { return; }
            //    OrderProcessingStart.EndInvoke(ar);
            //}

            // OrderProcessingChange
            public async Task OnOrderProcessingChange(int count)
            {
                if (OrderProcessingChange == null) { return; }
                this.OrdersProcessedCount = count;

                //if (false)
                //{
                //    OrderProcessingChange.BeginInvoke(this, new EventArgs(),
                //        new AsyncCallback(OrderProcessingChangeCompleted), null);
                //}

                var workTask = System.Threading.Tasks.Task.Run(() => OrderProcessingChange.Invoke(this, new EventArgs()));
                // We await the task instead of calling EndInvoke.
                await workTask;

            }

            //private void OrderProcessingChangeCompleted(IAsyncResult ar)
            //{
            //    if (OrderProcessingChange == null) { return; }
            //    OrderProcessingChange.EndInvoke(ar);
            //}
        }
    }
}
