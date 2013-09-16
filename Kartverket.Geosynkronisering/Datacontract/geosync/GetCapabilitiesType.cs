namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GetCapabilitiesType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    public partial class GetCapabilitiesType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Version", IsNullable=false)]
        public StringCollection AcceptVersions;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Section", IsNullable=false)]
        public StringCollection Sections;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("OutputFormat", IsNullable=false)]
        public StringCollection AcceptFormats;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string updateSequence;
        
        public virtual bool ShouldSerializeAcceptVersions()
        {
            return ((this.AcceptVersions != null) 
                        && (this.AcceptVersions.Count > 0));
        }
        
        public virtual bool ShouldSerializeSections()
        {
            return ((this.Sections != null) 
                        && (this.Sections.Count > 0));
        }
        
        public virtual bool ShouldSerializeAcceptFormats()
        {
            return ((this.AcceptFormats != null) 
                        && (this.AcceptFormats.Count > 0));
        }
    }
}
