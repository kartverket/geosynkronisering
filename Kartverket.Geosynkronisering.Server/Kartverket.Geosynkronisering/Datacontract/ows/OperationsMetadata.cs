namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.opengis.net/ows/1.1")]
    public partial class OperationsMetadata
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Operation")]
        public OperationCollection Operation;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Parameter")]
        public DomainTypeCollection Parameter;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Constraint")]
        public DomainTypeCollection Constraint;
        
        /// <remarks/>
        public object ExtendedCapabilities;
    }
}
