namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/sosi/produktspesifikasjon/geosynkronisering/0.3/types")]
    public partial class REP_CapabilitiesType : CapabilitiesBaseType
    {
        
        /// <remarks/>
        public object WSDL;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
        [System.Xml.Serialization.XmlArrayItemAttribute("FeatureType", IsNullable=false)]
        public FeatureTypeTypeCollection FeatureTypeList;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://www.opengis.net/fes/2.0")]
        public Filter_Capabilities Filter_Capabilities;
        
        public virtual bool ShouldSerializeFeatureTypeList()
        {
            return ((this.FeatureTypeList != null) 
                        && (this.FeatureTypeList.Count > 0));
        }
    }
}
