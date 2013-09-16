namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    public partial class RangeType
    {
        
        /// <remarks/>
        public ValueType MinimumValue;
        
        /// <remarks/>
        public ValueType MaximumValue;
        
        /// <remarks/>
        public ValueType Spacing;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
        [System.ComponentModel.DefaultValueAttribute(RangeTypeRangeClosure.closed)]
        public RangeTypeRangeClosure rangeClosure;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rangeClosureSpecified;
        
        public RangeType()
        {
            this.rangeClosure = RangeTypeRangeClosure.closed;
        }
    }
}
