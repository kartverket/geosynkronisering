namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("GetFeatureWithLock", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class GetFeatureWithLockType : GetFeatureType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="positiveInteger")]
        [System.ComponentModel.DefaultValueAttribute("300")]
        public string expiry;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(AllSomeType.ALL)]
        public AllSomeType lockAction;
        
        public GetFeatureWithLockType()
        {
            this.expiry = "300";
            this.lockAction = AllSomeType.ALL;
        }
    }
}
