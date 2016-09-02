namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/ows/1.1")]
    public partial class AddressType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DeliveryPoint")]
        public StringCollection DeliveryPoint;
        
        /// <remarks/>
        public string City;
        
        /// <remarks/>
        public string AdministrativeArea;
        
        /// <remarks/>
        public string PostalCode;
        
        /// <remarks/>
        public string Country;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ElectronicMailAddress")]
        public StringCollection ElectronicMailAddress;
    }
}
