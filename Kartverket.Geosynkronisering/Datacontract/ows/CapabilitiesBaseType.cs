namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(REP_CapabilitiesType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    public partial class CapabilitiesBaseType
    {
        
        /// <remarks/>
        public ServiceIdentification ServiceIdentification;
        
        /// <remarks/>
        public ServiceProvider ServiceProvider;
        
        /// <remarks/>
        public OperationsMetadata OperationsMetadata;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string updateSequence;
    }
}
