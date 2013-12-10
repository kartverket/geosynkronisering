using Kartverket.Geosynkronisering.ChangelogProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Xml;
using Kartverket.Geosynkronisering;


namespace TestProject
{
    
    
    /// <summary>
    ///This is a test class for MockChangelogTest and is intended
    ///to contain all MockChangelogTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MockChangelogTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        
        [TestMethod()]
        public void FullProcessTest()
        {

            // Set DataDirectory to match wcf service library (Kartverket.Geosynkronisering.dll) App_Data\ folder
            
            string referencePath = AppDomain.CurrentDomain.GetData("APPBASE").ToString();
            string relativePath = @"..\..\..\Kartverket.Geosynkronisering\App_Data";
            string dataDict = System.IO.Path.GetFullPath(System.IO.Path.Combine(referencePath, relativePath));
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDict);
    
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target2 = new ChangelogManager(db);

            MockChangelog target = new MockChangelog(); // TODO: Initialize to an appropriate value
            target.SetDb(db);
            int changelogid = 0; // TODO: Initialize to an appropriate value
            int count = 0;
            //From next changenumber

            //First time sync, get predefined syncset
            int lastindex = 0;
            //TODO define filters

            var xmlcap = target2.GetCapabilities();

            //TODO Test url/operations, filters, conformance...

            var xmldesc = target2.DescribeFeatureType(1);
            //TODO Test if schema is the same as expected

            //Get a list of predefined changelogs
            var xmllist = target2.ListStoredChangelogs(1);
            //TODO is changelog filter in list?

            //If no one matches in list -> order new changelog
            var ord = target.OrderChangelog(lastindex, count, "filter",1);
            //Assert.AreEqual(41, changelogid);

            //Check if job is finished?
            var status = target2.GetChangelogStatus(changelogid);
            //Assert.AreEqual("finished", status.ToString());
            //if (status.ToString() == "finished")
            {
                var xmllog = target2.GetChangelog(changelogid);
                Assert.IsNotNull(xmllog);
                //TODO Get file and check if exist...
                target2.AcknowledgeChangelogDownloaded(changelogid);
                //TODO check if file is deleted
            }

            //Continue syncing
            var respi = target.GetLastIndex(1);
            
            //Assert.AreEqual(2, lastindex);
            var respor = target.OrderChangelog(lastindex, count, "filter", 1);
            //Assert.AreEqual(42, changelogid);
            status = target2.GetChangelogStatus(changelogid);
            //Assert.AreEqual("finished", status);
            //if (status.ToString() == "finished")
            {
                var xmllog = target2.GetChangelog(changelogid);
                Assert.IsNotNull(xmllog);
                //TODO Get file and check if exist...
                target2.AcknowledgeChangelogDownloaded(changelogid);
                //TODO check if file is deleted
            }

        }
        

        /// <summary>
        ///A test for AcknowledgeChangelogDownloaded
        ///</summary>
        [TestMethod()]
        public void AcknowledgeChangelogDownloadedTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
                        
            int changelogid = 0; // TODO: Initialize to an appropriate value
            target.AcknowledgeChangelogDownloaded(changelogid);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");

        }

        /// <summary>
        ///A test for CancelChangelog
        ///</summary>
        [TestMethod()]
        public void CancelChangelogTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
            int changelogid = 42; // TODO: Initialize to an appropriate value
            target.CancelChangelog(changelogid);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DescribeFeatureType
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void DescribeFeatureTypeTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
           
            
            var actual = target.DescribeFeatureType(1);
            Assert.IsNotNull(actual);
            //XmlNamespaceManager nsMgr = new XmlNamespaceManager(actual.NameTable);
            //nsMgr.AddNamespace("rrr", "http://www.w3.org/2001/XMLSchema");
            //XmlNode featureTypeListNode = actual.SelectSingleNode("//rrr:complexType", nsMgr);
            //Assert.IsNotNull(featureTypeListNode);            
        }

        /// <summary>
        ///A test for GetCapabilities
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void GetCapabilitiesTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);

            XmlDocument expected = null; // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.GetCapabilities();
            //Assert.AreEqual(expected, actual);
            Assert.IsNotNull(actual);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(actual.NameTable);
            nsMgr.AddNamespace("rrr", "http://www.opengis.net/wfs/2.0");
            XmlNode featureTypeListNode = actual.SelectSingleNode("//rrr:FeatureTypeList", nsMgr);
            Assert.IsNotNull(featureTypeListNode);
        }

        /// <summary>
        ///A test for GetChangelog-
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void GetChangelogTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
            int changelogid = 42; // TODO: Initialize to an appropriate value
            //XmlDocument expected = null; // TODO: Initialize to an appropriate value
          
            var actual = target.GetChangelog(changelogid);
            Assert.IsNotNull(actual);
            //XmlNode changelogidNode = actual.SelectSingleNode("//changelogId");
            //Assert.IsNotNull(actual.);
        }

        /// <summary>
        ///A test for GetChangelogStatus
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void GetChangelogStatusTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
            int changelogid = 42; // TODO: Initialize to an appropriate value
            string expected = "finished"; // TODO: Initialize to an appropriate value
            
            var actual = target.GetChangelogStatus(changelogid);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for GetLastIndex
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void GetLastIndexTest()
        {
            geosyncEntities db = new geosyncEntities();
            
            MockChangelog target = new MockChangelog(); // TODO: Initialize to an appropriate value
            target.SetDb(db);
            string expected = "20"; // TODO: Initialize to an appropriate value
            
            var actual = target.GetLastIndex(1);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ListStoredChangelogs
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void ListStoredChangelogsTest()
        {
            geosyncEntities db = new geosyncEntities();
            ChangelogManager target = new ChangelogManager(db);
           
            var actual = target.ListStoredChangelogs(1);
            Assert.IsNotNull(actual);
            //XmlNode storedchangelogNode = actual.SelectSingleNode("//storedchangelog");
            //Assert.IsNotNull(storedchangelogNode);
        }

        /// <summary>
        ///A test for OrderChangelog
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void OrderChangelogTest()
        {
            geosyncEntities db = new geosyncEntities();
            MockChangelog target = new MockChangelog(); // TODO: Initialize to an appropriate value
            target.SetDb(db);
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            string todo_filter = string.Empty; // TODO: Initialize to an appropriate value
            //int expected = 41; // TODO: Initialize to an appropriate value
            
            var actual = target.OrderChangelog(startIndex, count, todo_filter, 1);
            Assert.IsNotNull(actual);
            //Assert.AreEqual(expected, actual);
        }
    }
}
