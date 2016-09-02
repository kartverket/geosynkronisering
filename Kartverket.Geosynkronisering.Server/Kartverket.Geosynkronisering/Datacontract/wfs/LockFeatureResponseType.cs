namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("LockFeatureResponse", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class LockFeatureResponseType
    {
        
        /// <remarks/>
        public FeaturesLockedType FeaturesLocked;
        
        /// <remarks/>
        public FeaturesNotLockedType FeaturesNotLocked;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lockId;
    }
}
