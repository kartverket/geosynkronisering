using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class FeedbackController
    {
        public class LogListEventArgs : EventArgs
        {
            public string Item { get; }
            public LogListEventArgs(string item)
            {
                Item = item;
            }
        }

        public class Progress
        {
            public delegate void LogListEventHandler(object sender, LogListEventArgs e);  

            public event System.EventHandler NewSynchMilestoneReached;
            public event System.EventHandler OrderProcessingStart;
            public event System.EventHandler OrderProcessingChange;
            public event LogListEventHandler UpdateLogList;

            public string MilestoneDescription;
            public long TotalNumberOfOrders;
            public int OrdersProcessedCount;

            // NewSynchMilestoneReached
            public void OnNewSynchMilestoneReached(string description)
            {
                if (NewSynchMilestoneReached == null) { return; }
                this.MilestoneDescription = description;

                NewSynchMilestoneReached.BeginInvoke(this, new EventArgs(),
                          new AsyncCallback(NewSynchMilestoneReachedCompleted), null);
            }

            private void NewSynchMilestoneReachedCompleted(IAsyncResult ar)
            {
                if (NewSynchMilestoneReached == null) { return; }
                NewSynchMilestoneReached.EndInvoke(ar);
            }

            // UpdateLogList
            public void OnUpdateLogList(string info)
            {
                if (UpdateLogList == null) { return; }

                UpdateLogList.Invoke(this, new LogListEventArgs(info));
            }

            // OrderProcessingStart
            public void OnOrderProcessingStart(long totalNumberOfOrders)
            {
                if (OrderProcessingStart == null) { return; }
                this.TotalNumberOfOrders = totalNumberOfOrders;
                this.OrdersProcessedCount = 0;

                OrderProcessingStart.BeginInvoke(this, new EventArgs(),
                          new AsyncCallback(OrderProcessingStartCompleted), null);
            }

            private void OrderProcessingStartCompleted(IAsyncResult ar)
            {
                if (OrderProcessingStart == null) { return; }
                OrderProcessingStart.EndInvoke(ar);
            }

            // OrderProcessingChange
            public void OnOrderProcessingChange(int count)
            {
                if (OrderProcessingChange == null) { return; }
                this.OrdersProcessedCount = count;

                OrderProcessingChange.BeginInvoke(this, new EventArgs(),
                          new AsyncCallback(OrderProcessingChangeCompleted), null);
            }

            private void OrderProcessingChangeCompleted(IAsyncResult ar)
            {
                if (OrderProcessingChange == null) { return; }
                OrderProcessingChange.EndInvoke(ar);
            }
        }
    }
}
