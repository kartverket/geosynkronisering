namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Insert", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class InsertType : AbstractTransactionActionType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public XmlElementCollection Any;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("application/gml+xml; version=3.2")]
        public string inputFormat;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string srsName;
        
        public InsertType()
        {
            this.inputFormat = "application/gml+xml; version=3.2";
        }
    }
}
