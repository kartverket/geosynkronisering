namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class additionalValues
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FeatureCollection", typeof(FeatureCollectionType))]
        [System.Xml.Serialization.XmlElementAttribute("SimpleFeatureCollection", typeof(SimpleFeatureCollectionType))]
        [System.Xml.Serialization.XmlElementAttribute("ValueCollection", typeof(ValueCollectionType))]
        public object Item;
    }
}
