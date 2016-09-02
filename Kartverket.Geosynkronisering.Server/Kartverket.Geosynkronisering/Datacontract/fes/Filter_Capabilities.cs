namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.opengis.net/fes/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/fes/2.0", IsNullable=false)]
    public partial class Filter_Capabilities
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Constraint", IsNullable=false)]
        public DomainType[] Conformance;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ResourceIdentifier", IsNullable=false)]
        public ResourceIdentifierType[] Id_Capabilities;
        
        /// <remarks/>
        public Scalar_CapabilitiesType Scalar_Capabilities;
        
        /// <remarks/>
        public Spatial_CapabilitiesType Spatial_Capabilities;
        
        /// <remarks/>
        public Temporal_CapabilitiesType Temporal_Capabilities;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Function", IsNullable=false)]
        public AvailableFunctionType[] Functions;
        
        /// <remarks/>
        public Extended_CapabilitiesType Extended_Capabilities;
    }
}
