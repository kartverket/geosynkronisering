namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/fes/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("PropertyIsEqualTo", Namespace="http://www.opengis.net/fes/2.0", IsNullable=false)]
    public partial class BinaryComparisonOpType : ComparisonOpsType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("expression")]
        public object[] expression;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool matchCase;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(MatchActionType.Any)]
        public MatchActionType matchAction;
        
        public BinaryComparisonOpType()
        {
            this.matchCase = true;
            this.matchAction = MatchActionType.Any;
        }
    }
}
