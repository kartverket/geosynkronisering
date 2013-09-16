namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    [System.Xml.Serialization.XmlRootAttribute("Exception", Namespace="http://www.opengis.net/ows/1.1", IsNullable=false)]
    public partial class ExceptionType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExceptionText")]
        public StringCollection ExceptionText;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string exceptionCode;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string locator;
    }
}
