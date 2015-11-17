using Kartverket.Geosynkronisering.ChangelogProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    public class PostGISChangelogTest
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
            PostGISChangelog target = new PostGISChangelog(); // TODO: Initialize to an appropriate value
            target.Intitalize(1);
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int count = 100; // TODO: Initialize to an appropriate value
            string todo_filter = string.Empty; // TODO: Initialize to an appropriate value

            var actual = target.OrderChangelog(startIndex, count, todo_filter,3);
            Assert.IsNotNull(actual);
        }
    }
}

