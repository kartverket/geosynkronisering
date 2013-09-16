namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/wfs/2.0", IsNullable=true)]
    public partial class FeatureTypeType
    {
        
        /// <remarks/>
        public System.Xml.XmlQualifiedName Name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Title")]
        public TitleCollection Title;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Abstract")]
        public AbstractCollection Abstract;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Keywords", Namespace="http://www.opengis.net/ows/1.1")]
        public KeywordsTypeCollection Keywords;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DefaultCRS", typeof(string), DataType="anyURI")]
        [System.Xml.Serialization.XmlElementAttribute("NoCRS", typeof(FeatureTypeTypeNoCRS))]
        [System.Xml.Serialization.XmlElementAttribute("OtherCRS", typeof(string), DataType="anyURI")]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName;
        
        /// <remarks/>
        public OutputFormatListType OutputFormats;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WGS84BoundingBox", Namespace="http://www.opengis.net/ows/1.1")]
        public WGS84BoundingBoxTypeCollection WGS84BoundingBox;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MetadataURL")]
        public MetadataURLTypeCollection MetadataURL;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Element", IsNullable=false)]
        public ElementTypeCollection ExtendedDescription;
        
        public virtual bool ShouldSerializeExtendedDescription()
        {
            return ((this.ExtendedDescription != null) 
                        && (this.ExtendedDescription.Count > 0));
        }
    }
}
