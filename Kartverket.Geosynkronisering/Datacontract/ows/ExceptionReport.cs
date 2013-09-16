namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.opengis.net/ows/1.1")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/ows/1.1", IsNullable=false)]
    public partial class ExceptionReport
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Exception")]
        public ExceptionTypeCollection Exception;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
        public string lang;
    }
}
