using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NLog;
using System.Xml.XPath;

namespace Kartverket.Geosynkronisering.Subscriber.BL.SchemaMapping
{
    /// <summary>
    /// Schema transformation with mapping from the nested structure of one or more simple features to the simple features for the Geoserver app-schema Mappingfile.
    /// Author: Lars Eggan, NOIS.
    /// </summary>
    public class GeoserverMapping : ITypeMappings
    {
        /// <summary>
        /// The namespace prefix for the current dataset
        /// </summary>
        private string _namespacePrefix;
        /// <summary>
        /// The namespace URI for GML application schema for he current dataset
        /// </summary>
        private string _namespaceUri;

        /// <summary>
        /// The namespace prefix in the GeoServer mapping file
        /// </summary>
        private string _namespacePrefixMapping;

        /// <summary>
        /// The attribute mappings for non-geometry attributes
        /// </summary>
        private IEnumerable<XElement> _attributeMappings;
        /// <summary>
        /// The attributemappings for geometry attributes
        /// </summary>
        private IEnumerable<XElement> _attributeMappingsGeom;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        //public string NamespacePrefix
        //{
        //    get { return _namespacePrefix; }
        //    set { _namespacePrefix = value; }
        //}

        /// <summary>
        /// Gets or sets the namespace URI.
        /// </summary>
        /// <value>
        /// The namespace URI.
        /// </value>
        public string NamespaceUri
        {
            get { return _namespaceUri; }
            set { _namespaceUri = value; }
        }

        /// <summary>
        /// Sets the GeoServer XML mapping file.
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

                // list All attributeMappings that contains <sourceExpression>, replace all " with blank
                foreach (var attrMapping in attributeMappings.ToList())
                {
                    //Console.WriteLine(attrMapping);
                    if (attrMapping.Element("sourceExpression").Element("OCQL").Value.StartsWith("\"") && attrMapping.Element("sourceExpression").Element("OCQL").Value.EndsWith("\""))
                    {
                        // Replace all " with blank, e.g "høyde" --> høyde
                        string newVal = attrMapping.Element("sourceExpression").Element("OCQL").Value;
                        newVal = newVal.Replace("\"", "");

                        attrMapping.Element("sourceExpression").Element("OCQL").Value = newVal;
                    }
                }

                _attributeMappings = attributeMappings;

                //
                // Special handling geometries, they conaint <targetAttributeNode>
                //
                var attributeMappingsGeom =
                    from item in doc.Descendants("AttributeMapping")
                    where (from m in item.Elements("sourceExpression")
                           where (from n in item.Elements("targetAttributeNode") select n).Any() == true
                           select m).Any()
                    select item;
                // list All attributeMappingsGeom that contains <sourceExpression>, replace all " with blank
                foreach (var attrMappingGeo in attributeMappingsGeom.ToList())
                {
                    //Console.WriteLine(attrMapping);
                    if (attrMappingGeo.Element("sourceExpression").Element("OCQL").Value.StartsWith("\"") && attrMappingGeo.Element("sourceExpression").Element("OCQL").Value.EndsWith("\""))
                    {
                        // Replace all " with blank, e.g "Område" --> område
                        string newVal = attrMappingGeo.Element("sourceExpression").Element("OCQL").Value;
                        newVal = newVal.Replace("\"", "");

                        attrMappingGeo.Element("sourceExpression").Element("OCQL").Value = newVal;
                    }
                }
                _attributeMappingsGeom = attributeMappingsGeom;


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
                    XNamespace nsApp = _namespaceUri; // "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                    XmlNamespaceManager nsmgrApp = new XmlNamespaceManager(new NameTable());
                    nsmgrApp.AddNamespace(_namespacePrefix, _namespaceUri);
                    XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                    XNamespace nsFes = "http://www.opengis.net/fes/2.0";
                    nsmgrApp.AddNamespace("wfs", "http://www.opengis.net/wfs/2.0");

                    foreach (var ele in transactions.ToList()) //foreach (var ele in docWfs.Descendants(nsWfs + "Insert")) //.Descendants("ArealressursGrense"))  //foreach (var ele in wfs.ToList())
                    {
                        //countTransactions++;


                        // where item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" || item.Name == nsWfs + "Replace"
                        if (ele.Name == nsWfs + "Insert" && !ele.IsEmpty)
                        {
                            //string featureType = "";
                            //XName xNameFeaturetype = null;
                            Console.WriteLine(ele.Name);

                            //20131210-Leg: There could be more than one featureType in one Insert
                            var xNameFeaturetypeListQuery = from item in ele.Elements()
                                                            group item.Name
                                                                by item.Elements().ElementAt(0).Name
                                                                into g
                                                                select g.Distinct().ToList();

                            foreach (var xNameFeaturetypeGroup in xNameFeaturetypeListQuery)
                            {
                                // Because the IGrouping<TKey, TElement>objects produced by a group query are essentially a list of lists,
                                // you must use a nested foreach loop to access the items in each group
                                foreach (var xNameFeaturetype in xNameFeaturetypeGroup)
                                {
                                    //var xNameFeaturetype = xNameFeaturetypeList.ElementAt(0).ElementAt(i); // Gets the full name of this element.

                                    string featureType = xNameFeaturetype.LocalName; //Gets the local (unqualified) part of the name
                                    Console.WriteLine(featureType);

                                    //Console.WriteLine(ele.Elements().ElementAt(0).Name.LocalName);
                                    //string featureType = ele.Elements().ElementAt(0).Name.LocalName; //Gets the local (unqualified) part of the name
                                    //var xNameFeaturetype = ele.Elements().ElementAt(0).Name; // Gets the full name of this element.

                                    //var featureElems = ele.Elements(featureType); //Returns a collection of the child elements of this element 
                                    var featureElems = ele.Elements(xNameFeaturetype);
                                    Console.WriteLine("feature elems.Count():{0}", featureElems.Count());

                                    foreach (var feature in featureElems.ToList())
                                    {
                                        //Console.WriteLine(feature);
                                        foreach (var xEleAttributeMapping in _attributeMappings)
                                        {
                                            // Simplifies the insert element.
                                            // Replace the nodes with complex types in the gml-file with simple types found
                                            //  in the mapping file
                                            SimplifyInsertElement(xEleAttributeMapping, featureType, feature, nsmgrApp, nsApp);
                                        }

                                        foreach (var xEleAttributeMapping in _attributeMappingsGeom)
                                        {
                                            // Special handling for geoms, e.g. Område should be omraade, change the xelement name
                                            SimplifyInsertGeomElement(xEleAttributeMapping, featureType, feature, nsmgrApp, nsApp);
                                        }
                                    }
                                }
                            }

                            countTransactions++;
                        }
                        else if ((ele.Name == nsWfs + "Update" || ele.Name == nsWfs + "Delete") && !ele.IsEmpty)
                        {
                            //
                            //TODO: 20140324-Leg: This code (wws:update) is checked back 20151005, but NOT comletely tested for GeoServer.
                            //

                            // Get the objecttype name
                            string featureType = ele.Attribute("typeName").Value; //Gets the local (unqualified) part of the name
                            var xNameFeaturetype = ele.Attribute("typeName").Name; ; // Gets the full name of this element.
                            Console.WriteLine("featureType: {0} wfs:{1}", featureType, ele.Name);

                            List<string> valueReferenceToRemove = new List<string>();

                            //var values = ele.Descendants(nsWfs + "Value"); // Update only
                            var properties = ele.Descendants(nsWfs + "Property"); // Update only
                            //var valueReferences = ele.Descendants(nsWfs + "ValueReference"); // Update only - 20140323-Leg

                            var valueReferencesFilter = ele.DescendantsAndSelf(nsFes + "ValueReference"); // Delete and Update Filter part
                            IEnumerable<XElement> xElements = null;
                            for (int i = 0; i < 2; i++)
                            {
                                if (i == 0 && properties.Any())
                                {
                                    xElements = properties;
                                }
                                /*
                                 *  if (i == 0 && values.Any())
                            {
                                xElements = values;
                            }
                                 */
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
                                                //
                                                // Filter part - wfs:Update and wfs:Delete
                                                //

                                                // mask of namespace prefix
                                                string targetAttrMinusNamespacePrefix = RemoveNamespacePrefixOfXPathExpression(targetAttr, _namespacePrefix);

                                                if (elePropOrFilter.Value == targetAttrMinusNamespacePrefix)
                                                {
                                                    //replace the children nodes.
                                                    string strNewContent =
                                                        xEleAttributeMapping.Element("sourceExpression")
                                                                            .Element("OCQL")
                                                                            .Value;
                                                    elePropOrFilter.ReplaceNodes(strNewContent);
                                                }
                                                else if (elePropOrFilter.Value == targetAttr)
                                                {
                                                    // 20131015-Leg: Filter ValueReference content with namespace prefix
                                                    string strNewContent =
                                                     xEleAttributeMapping.Element("sourceExpression")
                                                                         .Element("OCQL")
                                                                         .Value;
                                                    elePropOrFilter.ReplaceNodes(_namespacePrefix + ":" + strNewContent);
                                                }
                                            }

                                            else
                                            {
                                                //
                                                // Value part - wfs:update only
                                                //
                                   
                                                //var values = elePropOrFilter.Descendants(nsWfs + "Value");
                                                
                                                var valueReference = elePropOrFilter.Element(nsWfs + "ValueReference");
                                                
                                                //XElement xEleTargetValrefElement = valueReference.XPathSelectElement(targetAttrFirstNode, nsmgrApp) as XElement;
                                                if (valueReference.Value == targetAttrFirstNode) // if (xEleTargetValrefElement != null && !xEleTargetValrefElement.IsEmpty)
                                                {
                                                    foreach (var val in elePropOrFilter.Descendants(nsWfs + "Value"))
                                                    {
                                                        XElement xEleTargetAttr = val.XPathSelectElement(targetAttr, nsmgrApp) as XElement;

                                                        // mask of namespace prefix
                                                        //string targetAttrMinusNamespacePrefix = RemoveNamespacePrefixOfXPathExpression(targetAttr, _namespacePrefix);
                                                        //xEleTargetAttr = elePropOrFilter.XPathSelectElement(targetAttrMinusNamespacePrefix, nsmgrApp) as XElement;

                                                        //XElement x = ele.XPathSelectElement(targetAttrFirstNode, mgr) as XElement;
                                                        if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                                                        {
                                                            XElement xEleTargetAttrFirstNode = val.XPathSelectElement(targetAttrFirstNode, nsmgrApp);

                                                            // Create a new element where we use the element name from the mapping file.
                                                            // Add it before the Property node, then remove the current Value node.
                                                            // e.g. xEleTargetAttrFirstNode is here <app:identifikasjon>, xEleTargetAttr is <app:lokalId>:
                                                            //  <wfs:Property>
                                                            //    <wfs:ValueReference>app:identifikasjon</wfs:ValueReference>
                                                            //    <wfs:Value>
                                                            //      <app:identifikasjon>
                                                            //        <app:Identifikasjon>
                                                            //          <app:lokalId>4eb2d1e4-bf9f-45f2-875f-b9707dd9f885</app:lokalId>
                                                            //          <app:navnerom>no.skogoglandskap.ar5.ArealressursFlate</app:navnerom>
                                                            //          <app:versjonId>1.0</app:versjonId>
                                                            //        </app:Identifikasjon>
                                                            //      </app:identifikasjon>
                                                            //    </wfs:Value>
                                                            //  </wfs:Property>

                                                            // namespace mangler på ValueReference tag. Legger vi på det, så fjernes de med ett nivå i "ValueReference for removal".
                                                            // løsning er å oppdatere det på slutten.

                                                            // 20131015-Leg: ValueReference content with namespace prefix
                                                            XElement newEle = new XElement(nsWfs + "Property",
                                                                new XElement("ValueReference", _namespacePrefix + ":" + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                                //new XElement("ValueReference", xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                                new XElement(nsWfs + "Value",
                                                                new XElement(nsApp + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value)));

                                                            //XElement newEle = new XElement(nsWfs + "Property",
                                                            //    new XElement("ValueReference", xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                            //    new XElement(nApp + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value));

                                                            xEleTargetAttrFirstNode.Parent.Parent.AddBeforeSelf(newEle); //before node Property.Value
                                                            xEleTargetAttr.Remove();

                                                            // Mark <ValueReference> for removal
                                                            if (targetAttrArr.Length > 0) //if (targetAttrArr.Length > 2)
                                                            {
                                                                // 20131015-Leg: ValueReference content with namespace prefix
                                                                valueReferenceToRemove.Add(_namespacePrefix + ":" + targetAttrFirstNode);  //valueReferenceToRemove.Add(targetAttrFirstNode);
                                                            }

                                                        }



                                                    }
                                                }
                                                
                                                #if (false)
                                                
                                               

                                                XElement xEleTargetAttr = elePropOrFilter.XPathSelectElement(targetAttr, nsmgrApp) as XElement;

                                                // mask of namespace prefix
                                                //string targetAttrMinusNamespacePrefix = RemoveNamespacePrefixOfXPathExpression(targetAttr, _namespacePrefix);
                                                //xEleTargetAttr = elePropOrFilter.XPathSelectElement(targetAttrMinusNamespacePrefix, nsmgrApp) as XElement;

                                                //XElement x = ele.XPathSelectElement(targetAttrFirstNode, mgr) as XElement;
                                                if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                                                {
                                                    XElement xEleTargetAttrFirstNode = elePropOrFilter.XPathSelectElement(targetAttrFirstNode, nsmgrApp);

                                                    // Create a new element where we use the element name from the mapping file.
                                                    // Add it before the Property node, then remove the current Value node.
                                                    // e.g. xEleTargetAttrFirstNode is here <app:identifikasjon>, xEleTargetAttr is <app:lokalId>:
                                                    //  <wfs:Property>
                                                    //    <wfs:ValueReference>app:identifikasjon</wfs:ValueReference>
                                                    //    <wfs:Value>
                                                    //      <app:identifikasjon>
                                                    //        <app:Identifikasjon>
                                                    //          <app:lokalId>4eb2d1e4-bf9f-45f2-875f-b9707dd9f885</app:lokalId>
                                                    //          <app:navnerom>no.skogoglandskap.ar5.ArealressursFlate</app:navnerom>
                                                    //          <app:versjonId>1.0</app:versjonId>
                                                    //        </app:Identifikasjon>
                                                    //      </app:identifikasjon>
                                                    //    </wfs:Value>
                                                    //  </wfs:Property>

                                                    // namespace mangler på ValueReference tag. Legger vi på det, så fjernes de med ett nivå i "ValueReference for removal".
                                                    // løsning er å oppdatere det på slutten.

                                                    // 20131015-Leg: ValueReference content with namespace prefix
                                                    XElement newEle = new XElement(nsWfs + "Property",
                                                        new XElement("ValueReference", _namespacePrefix + ":" + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                        //new XElement("ValueReference", xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                        new XElement(nsWfs + "Value",
                                                        new XElement(nsApp + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value)));

                                                    //XElement newEle = new XElement(nsWfs + "Property",
                                                    //    new XElement("ValueReference", xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value),
                                                    //    new XElement(nApp + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value, xEleTargetAttr.Value));

                                                    xEleTargetAttrFirstNode.Parent.Parent.AddBeforeSelf(newEle); //before node Property.Value
                                                    xEleTargetAttr.Remove();

                                                    // Mark <ValueReference> for removal
                                                    if (targetAttrArr.Length > 0) //if (targetAttrArr.Length > 2)
                                                    {
                                                        // 20131015-Leg: ValueReference content with namespace prefix
                                                        valueReferenceToRemove.Add(_namespacePrefix + ":" + targetAttrFirstNode);  //valueReferenceToRemove.Add(targetAttrFirstNode);
                                                    }

                                                }
#endif
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
                                    var propertiesToRemove = from item in ele.Descendants(nsWfs + "Property")
                                                     let element = item.Element(nsWfs + "ValueReference")
                                                     where element != null && _namespacePrefix + ":" + element.Value == valRef //"identifikasjon"
                                                     select item;

                                    if (propertiesToRemove != null && propertiesToRemove.Any())
                                    {
                                        propertiesToRemove.Remove();
                                    }
                                }
                                valueReferenceToRemove.Clear();
                            }

                            if (ele.Name == nsWfs + "Update")
                            {
                                // Fix valuereference for Geometry ValueReference, e.g. Område should be omraade.
                                // using _attributeMappingsGeom
                                FixUpdateGeomValueReference(_attributeMappingsGeom, ele, nsWfs, featureType);
                            }

                            if (ele.Name == nsWfs + "Update")
                            {
                                // Add eventual missing namespace on ValueReference
                                var valueReferences = ele.DescendantsAndSelf("ValueReference");
                                foreach (var valRef in valueReferences.ToList())
                                {
                                    string strNewContent = valRef.Name.ToString();
                                    valRef.Name = nsWfs + strNewContent;
                                }
                            }

                            countTransactions++;
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
                    //retVal = docWfs;
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
        /// Simplifies the Insert element.
        /// Replace the nodes with complex types in the gml-file with simple types found
        ///  in the mapping file
        /// </summary>
        /// <param name="xEleAttributeMapping">The GeoServer AttributeMapping xElement.</param>
        /// <param name="featureType">Type of the feature.</param>
        /// <param name="feature">The feature.</param>
        /// <param name="nsmgrApp">The XmlNamespaceManager used for resolving namespaces in an XPath expression.</param>
        /// <param name="nsApp">The XML namespace used for resolving namespaces in XElement.</param>
        private void SimplifyInsertElement(XElement xEleAttributeMapping, string featureType, XElement feature,
                                           XmlNamespaceManager nsmgrApp, XNamespace nsApp)
        {

            try
            {

                string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                string[] targetAttrArr = targetAttrVal.Split('/');
                //Console.WriteLine("targetAttrArr[0]: {0}", targetAttrArr[0]);


                if (targetAttrArr[0] == _namespacePrefix + ":" + featureType)
                //if (targetAttrArr[0] == ns + featureType) //featureType)
                {
                    string targetAttr = String.Join("/", targetAttrArr, 1, targetAttrArr.Length - 1);
                    // Mask of XPath expression, we want the second one
                    string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                    //Console.WriteLine("targetAttrFirstNode: {0}", targetAttrFirstNode);


                    XElement xEleTargetAttr = feature.XPathSelectElement(targetAttr, nsmgrApp) as XElement;
                    //XElement x = ele.XPathSelectElement(targetAttrFirstNode, mgr) as XElement;
                    if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                    {
                        XElement xEleTargetAttrFirstNode = feature.XPathSelectElement(targetAttrFirstNode, nsmgrApp);

                        //Console.WriteLine("targetAttribute: {0}", targetAttr);
                        //Console.WriteLine("targetAttrFirstNode: {0}", targetAttrFirstNode);
                        //Console.WriteLine("XElement found: {0}", xEleTargetAttr);
                        //Console.WriteLine("XElement found: {0}", xEleTargetAttrFirstNode);
                        //Console.WriteLine("xEleTargetAttr.Value:{0}", xEleTargetAttr.Value);

                        // Create a new element where we use the element name from the mapping file
                        // Add it before the first node, then remove the current node
                        XElement newEle =
                            new XElement(nsApp + xEleAttributeMapping.Element("sourceExpression").Element("OCQL").Value,
                                         xEleTargetAttr.Value);
                        xEleTargetAttrFirstNode.AddBeforeSelf(newEle);
                        xEleTargetAttr.Remove();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("SimplifyInsertElement failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Simplifies the Insert geom element.
        /// </summary>
        /// <param name="xEleAttributeMapping">The GeoServer AttributeMapping xElement.</param>
        /// <param name="featureType">Type of the feature.</param>
        /// <param name="feature">The feature.</param>
        /// <param name="nsmgrApp">The XmlNamespaceManager used for resolving namespaces in an XPath expression.</param>
        /// <param name="nsApp">The XML namespace used for resolving namespaces in XElement.</param>
        private void SimplifyInsertGeomElement(XElement xEleAttributeMapping, string featureType, XElement feature,
                                               XmlNamespaceManager nsmgrApp, XNamespace nsApp)
        {
            try
            {


                // Special handling for geoms, e.g. Område should be omraade, change the xelement name
                string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                string[] targetAttrArr = targetAttrVal.Split('/');

                if (targetAttrArr[0] == _namespacePrefix + ":" + featureType)
                {
                    string targetAttr = String.Join("/", targetAttrArr, 1, targetAttrArr.Length - 1);
                    string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                    XElement xEleTargetAttr = feature.XPathSelectElement(targetAttr, nsmgrApp) as XElement;
                    if (xEleTargetAttr != null && !xEleTargetAttr.IsEmpty)
                    {
                        XElement xEleTargetAttrFirstNode =
                            feature.XPathSelectElement(targetAttrFirstNode, nsmgrApp);
                        string strNewContent =
                            xEleAttributeMapping.Element("sourceExpression")
                                                .Element("OCQL")
                                                .Value;

                        //strNewContent = strNewContent.Replace("\"", ""); // Replace all " with blank (æ,ø,å)

                        xEleTargetAttrFirstNode.Name = nsApp + strNewContent;
                    }
                }
            }
            catch (Exception ex)
            {

                logger.ErrorException("SimplifyInsertGeomElement failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Fix valuereference for Geometry ValueReference using _attributeMappingsGeom.
        /// e.g. Område should be omraade.
        /// </summary>
        /// <param name="attributeMappingsGeom">The GeoServer AttributeMappings for geometries.</param>
        /// <param name="ele">The Update geom root XElement to fix.</param>
        /// <param name="nsWfs">The wfs XML namespace.</param>
        /// <param name="featureType">Type of the feature.</param>
        private void FixUpdateGeomValueReference(IEnumerable<XElement> attributeMappingsGeom, XElement ele, XNamespace nsWfs, string featureType)
        {
            // Fix valuereference for Geometry ValueReference, e.g. Område should be omraade
            var valueReferences = ele.DescendantsAndSelf(nsWfs + "ValueReference"); // Update
            foreach (var xEleAttributeMapping in attributeMappingsGeom)
            {
                // Special handling for geoms
                string targetAttrVal = xEleAttributeMapping.Element("targetAttribute").Value;
                string[] targetAttrArr = targetAttrVal.Split('/');
                string targetAttrFirstNode = targetAttrArr[1]; //+ "/";
                if (targetAttrArr[0] == featureType)
                {
                    foreach (var valRef in valueReferences.ToList())
                    {
                        // 20131015-Leg: ValueReference content has namespace prefix
                        if (valRef.Value == targetAttrFirstNode)  //if (_namespacePrefix + ":" + valRef.Value == targetAttrFirstNode)
                        {
                            string strNewContent =
                                xEleAttributeMapping.Element("sourceExpression")
                                                    .Element("OCQL")
                                                    .Value;
                            //strNewContent = strNewContent.Replace("\"", ""); // Replace all " with blank (æ,ø,å)
                            // 20131015-Leg: ValueReference content with namespace prefix
                            valRef.ReplaceNodes(_namespacePrefix + ":" + strNewContent); //valRef.ReplaceNodes(strNewContent);
                        }
                    }
                }
            }
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
                    string targetAttr = ReplaceNamespacePrefixOfXPathExpression(originalTargetAttr, _namespacePrefixMapping, _namespacePrefix);

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
        /// Replace namespace prefix for an XPath expression.
        ///  (e.g. ar5:ArealressursFlate/ar5:identifikasjon/ar5:Identifikasjon/ar5:lokalId with app:ArealressursFlate/app:identifikasjon/app:Identifikasjon/app:lokalId)
        /// </summary>
        /// <param name="originalXPathExpression"></param>
        /// <param name="oldNamespacePrefix">The old namespace prefix.</param>
        /// <param name="newNamespacePrefix">The new namespace prefix.</param>
        /// <returns>
        /// the modified XPath expression string
        /// </returns>
        private string ReplaceNamespacePrefixOfXPathExpression(string originalXPathExpression, string oldNamespacePrefix, string newNamespacePrefix)
        {
            try
            {
                string[] originalTargetAttrArr = originalXPathExpression.Split('/');
                string[] targetAttrArr = originalTargetAttrArr;
                string oldNamespacePrefixWithColon = oldNamespacePrefix + ":";
                string newNamespacePrefixWithColon = "";
                if (!string.IsNullOrEmpty(newNamespacePrefix))
                {
                    newNamespacePrefixWithColon = newNamespacePrefix + ":";
                }

                for (int i = 0; i < originalTargetAttrArr.Length; i++)
                {
                    targetAttrArr[i] = originalTargetAttrArr[i].Replace(oldNamespacePrefixWithColon, newNamespacePrefixWithColon);
                }
                return String.Join("/", targetAttrArr);
            }
            catch (Exception ex)
            {
                logger.ErrorException("TargetAttrArrReplaceNamespacePrefix failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Removes the namespace prefix of an XPath expression.
        /// </summary>
        /// <param name="originalXPathExpression">The original x path expression.</param>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <returns> the modified XPath expression </returns>
        private string RemoveNamespacePrefixOfXPathExpression(string originalXPathExpression, string namespacePrefix)
        {
            return ReplaceNamespacePrefixOfXPathExpression(originalXPathExpression, namespacePrefix, "");
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
