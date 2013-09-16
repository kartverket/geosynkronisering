namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DomainType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    public partial class UnNamedDomainType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Range", typeof(RangeType), IsNullable=false)]
        [System.Xml.Serialization.XmlArrayItemAttribute("Value", typeof(ValueType), IsNullable=false)]
        public object[] AllowedValues;
        
        /// <remarks/>
        public AnyValue AnyValue;
        
        /// <remarks/>
        public NoValues NoValues;
        
        /// <remarks/>
        public ValuesReference ValuesReference;
        
        /// <remarks/>
        public ValueType DefaultValue;
        
        /// <remarks/>
        public DomainMetadataType Meaning;
        
        /// <remarks/>
        public DomainMetadataType DataType;
        
        /// <remarks/>
        public DomainMetadataType UOM;
        
        /// <remarks/>
        public DomainMetadataType ReferenceSystem;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Metadata")]
        public MetadataType[] Metadata;
    }
}
