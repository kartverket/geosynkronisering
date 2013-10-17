using System.Collections.Generic;
using System.Xml.Linq;

namespace Kartverket.Geosynkronisering.Subscriber.BL.SchemaMapping
{
    /// <summary>
    /// Schema transformation mapping interface.
    /// </summary>
    interface ITypeMappings
    {
        //string NamespacePrefix { get; set; }
        string NamespaceUri { get; set; }
        bool SetXmlMappingFile(string mappingFileName);
        XElement Simplify(string changelogFilename);
        bool SetCsvMappingFiles(List<string>csvMappingFiles );
    }
}
