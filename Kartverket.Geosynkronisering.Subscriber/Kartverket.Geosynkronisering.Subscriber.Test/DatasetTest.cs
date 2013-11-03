using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NUnit.Framework;

namespace Kartverket.Geosynkronisering.Subscriber.Test
{
    [TestFixture]
    public class DatasetTest
    {
        [Test]
        public void TestGetDataset()
        {
            var dataset = SubscriberDatasetManager.GetDataset(1); 
            Assert.AreEqual(dataset.Name,"Flytebrygge");
        }

        [Test]
        public void TestGetTargetNamespace()
        {
            var dataset = SubscriberDatasetManager.GetDataset(1);
            Assert.IsNull(dataset.TargetNamespace);
        }

        [Test]
        public void TestUpdateDataset()
        {
            SubscriberDataset geoClientDataset = new SubscriberDataset {DatasetId = 1, LastIndex = 0, MaxCount = 1000};

            var res = SubscriberDatasetManager.UpdateDataset(geoClientDataset);
            Assert.IsTrue(res);
        }
    }
}
