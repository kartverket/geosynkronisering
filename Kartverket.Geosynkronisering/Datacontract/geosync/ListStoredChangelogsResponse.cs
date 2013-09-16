namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://skjema.geonorge.no/sosi/produktspesifikasjon/geosynkronisering/0.3/reqresp" +
        "")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://skjema.geonorge.no/sosi/produktspesifikasjon/geosynkronisering/0.3/reqresp" +
        "", IsNullable=false)]
    public partial class ListStoredChangelogsResponse
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("storedchangelog", Namespace="http://skjema.geonorge.no/sosi/produktspesifikasjon/geosynkronisering/0.3/types", IsNullable=false)]
        public StoredChangelogTypeCollection @return;
        
        public virtual bool ShouldSerializereturn()
        {
            return ((this.@return != null) 
                        && (this.@return.Count > 0));
        }
    }
}
