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
        private SubscriberDatasetManager _subscriberDatasetManager;

        [SetUp]
        public void Setup()
        {
            _subscriberDatasetManager = new SubscriberDatasetManager();
        }

        [TearDown]
        public void Teardown()
        {
            _subscriberDatasetManager = null;
        }

        [Test]
        public void TestGetDataset()
        {
            var dataset = _subscriberDatasetManager.GetDataset(1);
            Assert.AreEqual(dataset.Name,"Flytebrygge");
        }
    }
}
