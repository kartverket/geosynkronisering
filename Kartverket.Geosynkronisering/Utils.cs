using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geosynkronisering
{
    public static class Utils
    {
        public static string BaseVirtualAppPath
        {
            get
            {
                HttpContext context = HttpContext.Current;
                string url = context.Request.PhysicalApplicationPath;
                if (url.EndsWith("/"))
                    return url;
                else
                    return url + "/";
            }
        }

        public static string BaseSiteUrl
        {
            get
            {
                //Return variable declaration
                string appPath = null;

                //Getting the current context of HTTP request
                HttpContext context = HttpContext.Current;

                //Checking the current context content
                if (context != null)
                {
                    //Formatting the fully qualified website url/name
                    appPath = string.Format("{0}://{1}{2}{3}",
                      context.Request.Url.Scheme,
                      context.Request.Url.Host,
                      context.Request.Url.Port == 80
                        ? string.Empty : ":" + context.Request.Url.Port,
                      context.Request.ApplicationPath);
                }
                if (!appPath.EndsWith("/"))
                    appPath += "/";
                return appPath;

            }
        }


        public static string App_DataPath
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    // does not work in a wcf service library:  AppDomain.CurrentDomain.GetData("DataDirectory").ToString();

                    string referencePath = AppDomain.CurrentDomain.GetData("APPBASE").ToString();
                    string relativePath = @"..\..\App_Data";
                    string dataDict = System.IO.Path.GetFullPath(System.IO.Path.Combine(referencePath, relativePath));

                    if (!System.IO.Directory.Exists(dataDict))
                    {
                        // Then we need to find the folder the hard way:
                        dataDict = "";
                        relativePath = @"..\..\..\Kartverket.Geosynkronisering\App_Data";
                        dataDict = System.IO.Path.GetFullPath(System.IO.Path.Combine(referencePath, relativePath));
                        // TODO: Test-program bør sette denne i sin .config-fil.
                    }
                    //System.IO.Path.Combine()
                    return dataDict;
                }
                else
                {
                    return HttpContext.Current.Server.MapPath("~/App_Data");
                }
            }
        }
    }
}