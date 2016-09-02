namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Transaction", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class TransactionType : BaseRequestType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Delete", typeof(DeleteType))]
        [System.Xml.Serialization.XmlElementAttribute("Insert", typeof(InsertType))]
        [System.Xml.Serialization.XmlElementAttribute("Native", typeof(NativeType))]
        [System.Xml.Serialization.XmlElementAttribute("Replace", typeof(ReplaceType))]
        [System.Xml.Serialization.XmlElementAttribute("Update", typeof(UpdateType))]
        public AbstractTransactionActionTypeCollection Items;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lockId;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(AllSomeType.ALL)]
        public AllSomeType releaseAction;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string srsName;
        
        public TransactionType()
        {
            this.releaseAction = AllSomeType.ALL;
        }
    }
}
