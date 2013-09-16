namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("GetPropertyValue", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class GetPropertyValueType : BaseRequestType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://www.opengis.net/fes/2.0")]
        public AbstractQueryExpressionType AbstractQueryExpression;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string valueReference;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string resolvePath;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="nonNegativeInteger")]
        [System.ComponentModel.DefaultValueAttribute("0")]
        public string startIndex;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="nonNegativeInteger")]
        public string count;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(ResultTypeType.results)]
        public ResultTypeType resultType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("application/gml+xml; version=3.2")]
        public string outputFormat;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(ResolveValueType.none)]
        public ResolveValueType resolve;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("*")]
        public string resolveDepth;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="positiveInteger")]
        [System.ComponentModel.DefaultValueAttribute("300")]
        public string resolveTimeout;
        
        public GetPropertyValueType()
        {
            this.startIndex = "0";
            this.resultType = ResultTypeType.results;
            this.outputFormat = "application/gml+xml; version=3.2";
            this.resolve = ResolveValueType.none;
            this.resolveDepth = "*";
            this.resolveTimeout = "300";
        }
    }
}
