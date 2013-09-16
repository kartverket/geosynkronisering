namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.opengis.net/ows/1.1")]
    public partial class ServiceIdentification : DescriptionType
    {
        
        /// <remarks/>
        public CodeType ServiceType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ServiceTypeVersion")]
        public StringCollection ServiceTypeVersion;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Profile", DataType="anyURI")]
        public StringCollection Profile;
        
        /// <remarks/>
        public string Fees;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AccessConstraints")]
        public StringCollection AccessConstraints;
    }
}
