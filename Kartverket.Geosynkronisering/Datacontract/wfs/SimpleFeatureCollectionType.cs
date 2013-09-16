namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FeatureCollectionType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("SimpleFeatureCollection", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class SimpleFeatureCollectionType
    {
        
        /// <remarks/>
        public System.Xml.XmlElement boundedBy;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("member")]
        public MemberPropertyTypeCollection member;
    }
}
