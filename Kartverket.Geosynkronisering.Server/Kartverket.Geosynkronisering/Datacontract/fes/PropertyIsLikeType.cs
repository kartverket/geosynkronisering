namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/fes/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("PropertyIsLike", Namespace="http://www.opengis.net/fes/2.0", IsNullable=false)]
    public partial class PropertyIsLikeType : ComparisonOpsType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("expression")]
        public object[] expression;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string wildCard;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string singleChar;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string escapeChar;
    }
}
