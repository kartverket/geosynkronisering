using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            public int TotalNumberOfOrders;
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
                this.NewLogListItem = info;

                UpdateLogList.BeginInvoke(this, new EventArgs(),
                          new AsyncCallback(UpdateLogListCompleted), null);
            }

            private void UpdateLogListCompleted(IAsyncResult ar)
            {
                if (UpdateLogList == null) { return; }
                UpdateLogList.EndInvoke(ar);
            }

            // OrderProcessingStart
            public void OnOrderProcessingStart(int totalNumberOfOrders)
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
