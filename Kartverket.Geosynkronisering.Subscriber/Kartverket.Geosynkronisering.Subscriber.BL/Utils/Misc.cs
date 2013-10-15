using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kartverket.Geosynkronisering.Subscriber.BL.Utils
{
    public class Misc
    {
        /// <summary>
        /// Create directory if missing
        /// </summary>
        /// <param name="path"></param>
        /// <returns> True if OK, false if exception </returns>
        public static bool CreateFolderIfMissing(string path)
        {
            try
            {
                bool folderExists = Directory.Exists((path));
                if (!folderExists)
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
