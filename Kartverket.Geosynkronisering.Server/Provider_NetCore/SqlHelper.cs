using System;
using System.Collections.Generic;
using System.Linq;
using ChangelogManager;

namespace Provider_NetCore
{
    internal class SqlHelper
    {
        internal static List<Datasets_NgisSubscriber> GetSubscribers(int datasetId)
        {
            var entities = new geosyncEntities();

            var subscribed = entities.Datasets_Subscribers.Where(d => d.datasetid == datasetId).ToList();

            subscribed.ForEach(
                d => d.subscriber = entities.Subscribers.FirstOrDefault(s => s.id == d.subscriberid)
            );

            return subscribed;
        }
    }
}