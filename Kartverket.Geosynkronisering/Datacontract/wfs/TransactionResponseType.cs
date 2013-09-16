namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/wfs/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("TransactionResponse", Namespace="http://www.opengis.net/wfs/2.0", IsNullable=false)]
    public partial class TransactionResponseType
    {
        
        /// <remarks/>
        public TransactionSummaryType TransactionSummary;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Feature", IsNullable=false)]
        public CreatedOrModifiedFeatureTypeCollection InsertResults;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Feature", IsNullable=false)]
        public CreatedOrModifiedFeatureTypeCollection UpdateResults;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Feature", IsNullable=false)]
        public CreatedOrModifiedFeatureTypeCollection ReplaceResults;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version;
        
        public TransactionResponseType()
        {
            this.version = "2.0.0";
        }
        
        public virtual bool ShouldSerializeInsertResults()
        {
            return ((this.InsertResults != null) 
                        && (this.InsertResults.Count > 0));
        }
        
        public virtual bool ShouldSerializeUpdateResults()
        {
            return ((this.UpdateResults != null) 
                        && (this.UpdateResults.Count > 0));
        }
        
        public virtual bool ShouldSerializeReplaceResults()
        {
            return ((this.ReplaceResults != null) 
                        && (this.ReplaceResults.Count > 0));
        }
    }
}
