namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/wfs/2.0", IsNullable=true)]
    public partial class StoredQueryListItemType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Title")]
        public TitleCollection Title;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ReturnFeatureType")]
        public XmlQualifiedNameCollection ReturnFeatureType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string id;
    }
}
