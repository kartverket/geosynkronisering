namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("WFS_Capabilities", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class WFS_CapabilitiesType : CapabilitiesBaseType
    {
        
        /// <remarks/>
        public WFS_CapabilitiesTypeWSDL WSDL;
        
        /// <remarks/>
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
