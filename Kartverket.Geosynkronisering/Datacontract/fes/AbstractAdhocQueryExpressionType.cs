namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/fes/2.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.opengis.net/fes/2.0", IsNullable=true)]
    public abstract partial class AbstractAdhocQueryExpressionType : AbstractQueryExpressionType
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AbstractProjectionClause")]
        public ObjectCollection AbstractProjectionClause;
        
        /// <remarks/>
        public object AbstractSelectionClause;
        
        /// <remarks/>
        public object AbstractSortingClause;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public StringCollection typeNames;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="NCName")]
        public StringCollection aliases;
    }
}
