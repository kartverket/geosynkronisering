namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NativeType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DeleteType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReplaceType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(UpdateType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InsertType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/wfs/2.0", IsNullable=true)]
    public abstract partial class AbstractTransactionActionType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string handle;
    }
}
