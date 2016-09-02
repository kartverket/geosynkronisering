namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TransactionType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LockFeatureType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreateStoredQueryType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DescribeStoredQueriesType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ListStoredQueriesType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GetFeatureType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GetFeatureWithLockType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GetPropertyValueType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DescribeFeatureTypeType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/wfs/2.0", IsNullable=true)]
    public abstract partial class BaseRequestType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string service;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string handle;
        
        public BaseRequestType()
        {
            this.service = "WFS";
            this.version = "2.0.0";
        }
    }
}
