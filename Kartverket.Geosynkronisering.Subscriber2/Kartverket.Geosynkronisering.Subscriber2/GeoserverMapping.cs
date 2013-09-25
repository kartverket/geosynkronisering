using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using System.Xml.XPath;

namespace Kartverket.Geosynkronisering.Subscriber2
{
    /// <summary>
    /// Schema transformation with mapping from the nested structure of one or more simple features to the simple features for the Geoserver app-schema Mappingfile.
    /// Author: Lars Eggan, NOIS.
    /// </summary>
    public class GeoserverMapping : ITypeMappings
    {
        private string _namespacePrefix;
        private string _namespaceUri;

        private string _namespacePrefixMapping;

        private IEnumerable<XElement> _attributeMappings;
        private IEnumerable<XElement> _attributeMappingsGeom;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        //public string NamespacePrefix
        //{
        //    get { return _namespacePrefix; }
        //    set { _namespacePrefix = value; }
        //}

        public string NamespaceUri
        {
            get { return _namespaceUri; }
            set { _namespaceUri = value; }
        }

        /// <summary>
        /// Sets the XML mapping file.
        /// </summary>
        /// <param name="mappingFileName">Name of the mapping file.</param>
        /// <returns></returns>
        public bool SetXmlMappingFile(string mappingFileName)
        {
            bool setXmlMappingFile = false;

            try
            {
                XElement doc = XElement.Load(mappingFileName);


                // Get the associated prefix from the mapping file.
                var namespaces =
                    from item in doc.Descendants("Namespace")
                    where item.Element("uri").Value == _namespaceUri
                    select item;
                if (namespaces.Any())
                {
                    _namespacePrefixMapping = namespaces.ElementAt(0).Element("prefix").Value;

                }

                // Get all AttributeMapping nodes that contains <sourceExpression> and does NOT contain <targetAttributeNode>:
                // TODO: Problems in Update with område
                var attributeMappings =
                 from item in doc.Descendants("AttributeMapping")
                 where (from m in item.Elements("sourceExpression")
                        where (from n in item.Elements("targetAttributeNode") select n).Any() == false
                        select m).Any()
                 select item;

                // list All attributeMappings that contains <sourceExpression>, remove the ones starting with '
                foreach (var attrMapping in attributeMappings.ToList())
                {
                    //Console.WriteLine(attrMapping);
                    if (attrMapping.Element("sourceExpression").Element("OCQL").Value.StartsWith("'"))
                    {
                        // remove the ones starting with '
                        attrMapping.Remove();
                    }
                }

                foreach (var attrMapping in attributeMappings.ToList())
                {
                    //Console.WriteLine(attrMapping);
                    if (attrMapping.Element("sourceExpression").Element("OCQL").Value.StartsWith("\"") && attrMapping.Element("sourceExpression").Element("OCQL").Value.EndsWith("\""))
                    {
                        // Replace all " with blank
                        string newVal = attrMapping.Element("sourceExpression").Element("OCQL").Value;
                        newVal = newVal.Replace("\"", "");

                        attrMapping.Element("sourceExpression").Element("OCQL").Value = newVal;
                    }
                }

                _attributeMappings = attributeMappings;

                // Special handling geometries
                _attributeMappingsGeom =
                    from item in doc.Descendants("AttributeMapping")
                    where (from m in item.Elements("sourceExpression")
                           where (from n in item.Elements("targetAttributeNode") select n).Any() == true
                           select m).Any()
                    select item;


                setXmlMappingFile = true;

            }
            catch (Exception ex)
            {
                setXmlMappingFile = false;
                logger.ErrorException("SetXmlMappingFile:", ex);
                throw;
            }

            return setXmlMappingFile;
        }

        /// <summary>
        /// Simplifies the specified changelog filename.
        /// The input file may be a changelog file (with wfs-t inside), or a wfs-t file.
        /// </summary>
        /// <param name="changelogFilename">The changelog filename or wfs-t filename.</param>
        /// <returns>changed XML document</returns>
        public XElement Simplify(string changelogFilename)
        {
            //throw new NotImplementedException();
            XElement retVal = null;

            try
            {
                XElement docWfs = XElement.Load(changelogFilename);

                // Gets the prefix associated with a namespace for this XElement
                _namespacePrefix = docWfs.GetPrefixOfNamespace(_namespaceUri);

                // replace the Namespace prefix in the mapping file
                MappingfileReplaceNamespacePrefix(_attributeMappings);
                MappingfileReplaceNamespacePrefix(_attributeMappingsGeom);

                int countTransactions = 0;
                var transactions = GetWfsTransactions(docWfs) as IEnumerable<XElement>;
                if (transactions != null)
                {

                    // Namespace stuff
                    XNamespace nAr5 = _namespaceUri; // "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                    XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
                    mgr.AddNamespace(_namespacePrefix, _namespaceUri);
                    XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                    XNamespace nsFes = "http://www.opengis.net/fes/2.0";

                    foreach (var ele in transactions.ToList()) //foreach (var ele in docWfs.Descendants(nsWfs + "Insert")) //.Descendants("ArealressursGrense"))  //foreach (var ele in wfs.ToList())
                    {
                        countTransactions++;


                        // where item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" || item.Name == nsWfs + "Replace"
                        if (ele.Name == nsWfs + "Insert")
                        {
                            //string featureType = "";
                            //XName xNameFeaturetype = null;
                            Console.WriteLine(ele.Name);

                            // Get the objecttype name
                            Console.WriteLine(ele.Elements().ElementAt(0).Name.LocalName);
                            string featureType = ele.Elements().ElementAt(0).Name.LocalName; //Gets the local (unqualified) part of the name
                            var xNameFeaturetype = ele.Elements().ElementAt(0).Name; // Gets the full name of this element.

                            //var featureElems = ele.Elements(featureType); //Returns a collection of the child elements of this element 
                            var featureElems = ele.Elements(xNameFeaturetype);
                            Console.WriteLine("feature elems.Count():{0}", featureElems.Count());

                            foreach (var feature in featureElems.ToList())
                            {
                                //Console.WriteLine(feature);
                                foreach (var xEleAttributeMapping in _attributeMappings)
                                {
                                    //
                                    // Replace the nodes with complex types in the gml-file with simple types found
                                    // in the mapping file: 
                                    //

                                    string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                                    string[] targetAttrArr = targetAttrVal.Split('/');
                                    //Console.WriteLine("targetAttrArr[0]: {0}", targetAttrArr[0]);


                                    if (targetAttrArr[0] == _namespacePrefix + ":" + featureType)  //if (targetAttrArr[0] == ns + featureType) //featureType)
                                    {


                                        string targetAttr = String.Join("/", targetAttrArr, 1, targetAttrArr.Length - 1);
                                        // Mask of XPath expression, we want the second one
                                        string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                                        //Console.WriteLine("targetAttrFirstNode: {0}", targetAttrFirstNode);


                                        XElement xEleTargetAttr = feature.XPathSelectElement(targetAttr, mgr) as XElement;
                                        //XElement x = ele.XPathSelectElement(targetAttrFirstNode, mgr) as XElement;
                                        if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                                        {
                                            XElement xEleTargetAttrFirstNode = feature.XPathSelectElement(targetAttrFirstNode, mgr);

                                            //Console.WriteLine("targetAttribute: {0}", targetAttr);
                                            //Console.WriteLine("targetAttrFirstNode: {0}", targetAttrFirstNode);
                                            //Console.WriteLine("XElement found: {0}", xEleTargetAttr);
                                            //Console.WriteLine("XElement found: {0}", xEleTargetAttrFirstNode);
                                            //Console.WriteLine("xEleTargetAttr.Value:{0}", xEleTargetAttr.Value);

                                            //xEleTargetAttrFirstNode.ReplaceWith(new XElement(nAr5 + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value));

                                            // Create a new element where we use the element name from the mapping file
                                            // Add it before the first node, than remove the current node
                                            XElement newEle = new XElement(nAr5 + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value);
                                            xEleTargetAttrFirstNode.AddBeforeSelf(newEle);
                                            xEleTargetAttr.Remove();

                                        }
                                    }
                                }

                                foreach (var xEleAttributeMapping in _attributeMappingsGeom)
                                {
                                    // Special handling for geoms, e.g. Område should be omraade, chnage the xelement name
                                    string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                                    string[] targetAttrArr = targetAttrVal.Split('/');

                                    if (targetAttrArr[0] == _namespacePrefix + ":" + featureType)
                                    {
                                        string targetAttr = String.Join("/", targetAttrArr, 1, targetAttrArr.Length - 1);
                                        string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                                        XElement xEleTargetAttr = feature.XPathSelectElement(targetAttr, mgr) as XElement;
                                        if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                                        {
                                            XElement xEleTargetAttrFirstNode =
                                                feature.XPathSelectElement(targetAttrFirstNode, mgr);
                                            string strNewContent =
                                                xEleAttributeMapping.Element("sourceExpression")
                                                          .Element("OCQL")
                                                          .Value;
                                            // Replace all " with blank (æ,ø,å)
                                            strNewContent = strNewContent.Replace("\"", "");
                                            
                                            xEleTargetAttrFirstNode.Name = nAr5 + strNewContent;
                                        }
                                    }
                                }
                            }

                        }
                        else if (ele.Name == nsWfs + "Update" || ele.Name == nsWfs + "Delete")
                        {
                            // TODO: More here for Update and Delete
                            // Get the objecttype name
                            string featureType = ele.Attribute("typeName").Value; //Gets the local (unqualified) part of the name
                            var xNameFeaturetype = ele.Attribute("typeName").Name; ; // Gets the full name of this element.
                            Console.WriteLine("featureType: {0} wfs:{1}", featureType, ele.Name);

                            List<string> valueReferenceToRemove = new List<string>();

                            var values = ele.Descendants(nsWfs + "Value"); // Update only
                            var valueReferencesFilter = ele.DescendantsAndSelf(nsFes + "ValueReference"); // Delete and Update Filter part
                            IEnumerable<XElement> xElements = null;
                            for (int i = 0; i < 2; i++)
                            {
                                if (i == 0 && values.Any())
                                {
                                    xElements = values;
                                }
                                else if (i == 1 && valueReferencesFilter.Any())
                                {
                                    xElements = valueReferencesFilter;
                                }

                                else
                                {
                                    continue; // break;
                                }


                                foreach (var elePropOrFilter in xElements.ToList())
                                // foreach (var elePropOrFilter in ele.Descendants(nsWfs + "Value").ToList())
                                {
                                    foreach (var xEleAttributeMapping in _attributeMappings)
                                    {
                                        //
                                        // Replace the nodes with complex types in the gml-file with simple types found
                                        // in the mapping file: 
                                        //

                                        string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                                        string[] targetAttrArr = targetAttrVal.Split('/');
                                        //Console.WriteLine("targetAttrArr[0]: {0}", targetAttrArr[0]);


                                        if (targetAttrArr[0] == featureType) //_namespacePrefix + ":" + featureType)  //if (targetAttrArr[0] == ns + featureType) //featureType)
                                        {


                                            string targetAttr = String.Join("/", targetAttrArr, 1, targetAttrArr.Length - 1);
                                            // Mask of XPath expression, we want the second one
                                            string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                                            //Console.WriteLine("targetAttrFirstNode: {0}", targetAttrFirstNode);

                                            if (i == 1 && valueReferencesFilter.Any())
                                            {
                                                // Filter part - wfs:Update and wfs:Delete
                                                string[] targetAttrMinusNamespaceArr = targetAttrVal.Split('/');
                                                for (int j = 1; j < targetAttrArr.Length; j++)
                                                {
                                                    // mask of namespace prefix
                                                    targetAttrMinusNamespaceArr[j] = targetAttrArr[j].Replace(_namespacePrefix + ":", "");
                                                }
                                                string targetAttrMinusNamespacePrefix = String.Join("/", targetAttrMinusNamespaceArr, 1, targetAttrMinusNamespaceArr.Length - 1);

                                                if (elePropOrFilter.Value == targetAttrMinusNamespacePrefix)
                                                {
                                                    string strNewContent =
                                                        xEleAttributeMapping.Element("sourceExpression")
                                                                            .Element("OCQL")
                                                                            .Value;
                                                    elePropOrFilter.ReplaceNodes(strNewContent);
                                                }
                                            }

                                            else
                                            {
                                                // Value part - wfs:update only

                                                XElement xEleTargetAttr = elePropOrFilter.XPathSelectElement(targetAttr, mgr) as XElement;
                                                //XElement x = ele.XPathSelectElement(targetAttrFirstNode, mgr) as XElement;
                                                if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                                                {
                                                    XElement xEleTargetAttrFirstNode = elePropOrFilter.XPathSelectElement(targetAttrFirstNode, mgr);

                                                    // Create a new element where we use the element name from the mapping file
                                                    // Add it before the first node, than remove the current node

                                                    //XElement newEle = new XElement(nAr5 + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value);
                                                    //xEleTargetAttrFirstNode.AddBeforeSelf(newEle);

                                                    XElement newEle = new XElement(nsWfs + "Property",
                                                        new XElement("ValueReference", xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                        new XElement(nAr5 + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value));
                                                    xEleTargetAttrFirstNode.Parent.Parent.AddBeforeSelf(newEle);
                                                    xEleTargetAttr.Remove();

                                                    //xEleTargetAttrFirstNode.Remove();
                                                    //xEleTargetAttr.RemoveAll();

                                                    // Mark ValueReference for removal
                                                    if (targetAttrArr.Length > 0) //if (targetAttrArr.Length > 2)
                                                    {
                                                        valueReferenceToRemove.Add(targetAttrFirstNode);
                                                    }

                                                }
                                            }
                                        }
                                    }


                                }
                            }

                            if (valueReferenceToRemove.Any())
                            {
                                // Remove all the ValueReference marked for removing to clean up.
                                foreach (var valRef in valueReferenceToRemove)
                                {
                                    var properties = from item in ele.Descendants(nsWfs + "Property")
                                                     let element = item.Element(nsWfs + "ValueReference")
                                                     where element != null && _namespacePrefix + ":" + element.Value == valRef //"identifikasjon"
                                                     select item;

                                    if (properties != null && properties.Any())
                                    {
                                        properties.Remove();
                                        //foreach (var xElement in properties.Elements().ToList())
                                        //{
                                        //    xElement.Remove();
                                        //}
                                    }
                                }
                                valueReferenceToRemove.Clear();
                            }

                            if (ele.Name == nsWfs + "Update")
                            {
                                // Fix valuereference for Geometry ValueReference, e.g. Område should be omraade
                                var valueReferences = ele.DescendantsAndSelf(nsWfs + "ValueReference"); // Update
                                foreach (var xEleAttributeMapping in _attributeMappingsGeom)
                                {
                                    // Special handling for geoms
                                    string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                                    string[] targetAttrArr = targetAttrVal.Split('/');
                                    string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                                    if (targetAttrArr[0] == featureType)
                                    {
                                        foreach (var valRef in valueReferences.ToList())
                                        {
                                            if (_namespacePrefix + ":" + valRef.Value == targetAttrFirstNode)
                                            {
                                                //xEleAttributeMapping
                                                string strNewContent =
                                              xEleAttributeMapping.Element("sourceExpression")
                                                                  .Element("OCQL")
                                                                  .Value;
                                                valRef.ReplaceNodes(strNewContent);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    Console.WriteLine("Transactions Count:{0}", countTransactions);

                }
                if (countTransactions > 0)
                {
                    retVal = docWfs;
                }
                else
                {
                    retVal = docWfs;
                }
            }

            catch (Exception ex)
            {
                logger.ErrorException("Simplify failed:", ex);
                throw;
            }
            return retVal;
        }

        /// <summary>
        ///  Replace namespace prefix for the Geoserver app-schema Mappingfile.
        /// </summary>
        /// <param name="attributeMappings">The attribute mappings XElements.</param>
        private void MappingfileReplaceNamespacePrefix(IEnumerable<XElement> attributeMappings)
        {
            try
            {
                foreach (var attrMapping in attributeMappings.ToList())
                {
                    XElement xEleTargetAttribute = attrMapping.Element("targetAttribute");
                    // replace namespace prefix in mapping with the prefix found in the source data
                    string originalTargetAttr = xEleTargetAttribute.Value;
                    var targetAttrArr = TargetAttrArrReplaceNamespacePrefix(originalTargetAttr);
                    string targetAttr = String.Join("/", targetAttrArr);
                    xEleTargetAttribute.ReplaceNodes(targetAttr);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("MappingfileReplaceNamespacePrefix failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Replace namespace prefix for an array of targetAttribute strings
        /// </summary>
        /// <param name="originalTargetAttr">The original target attribute string array.</param>
        /// <returns> the modified target attribute string array </returns>
        private string[] TargetAttrArrReplaceNamespacePrefix(string originalTargetAttr)
        {
            try
            {
                string[] originalTargetAttrArr = originalTargetAttr.Split('/');
                string[] targetAttrArr = originalTargetAttrArr;
                for (int i = 0; i < originalTargetAttrArr.Length; i++)
                {
                    targetAttrArr[i] = originalTargetAttrArr[i].Replace(_namespacePrefixMapping + ":", _namespacePrefix + ":");
                }
                return targetAttrArr;
            }
            catch (Exception ex)
            {
                logger.ErrorException("TargetAttrArrReplaceNamespacePrefix failed:", ex);
                throw;
            }
        }

        public bool SetCsvMappingFiles(List<string> csvMappingFiles)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get all wfs transactions (Insert, Update,  Delete, Replace) from changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private IEnumerable<XElement> GetWfsTransactions(XElement changeLog)
        {
            try
            {
                // Namespace must be set: xmlns:wfs="http://www.opengis.net/wfs/2.0"
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";

                IEnumerable<XElement> transactions =
                    from item in changeLog.Descendants()
                    where item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" || item.Name == nsWfs + "Replace"
                    select item;

                return transactions;
            }
            catch (Exception ex)
            {
                logger.ErrorException("SetXmlMappingFile failed:", ex);
                return null;
                //throw;
            }
        }
    }
}
