namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreateStoredQueryResponseType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("DropStoredQueryResponse", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class ExecutionStatusType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string status;
        
        public ExecutionStatusType()
        {
            this.status = "OK";
        }
    }
}
