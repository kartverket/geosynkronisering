namespace Kartverket.Geosynkronisering
{
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.opengis.net/fes/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Filter", Namespace="http://www.opengis.net/fes/2.0", IsNullable=false)]
    public partial class FilterType : AbstractSelectionClauseType
    {
        
        /// <remarks/>
        public ComparisonOpsType comparisonOps;
        
        /// <remarks/>
        public SpatialOpsType spatialOps;
        
        /// <remarks/>
        public TemporalOpsType temporalOps;
        
        /// <remarks/>
        public LogicOpsType logicOps;
        
        /// <remarks/>
        public ExtensionOpsType extensionOps;
        
        /// <remarks/>
        public FunctionType Function;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("_Id")]
        public AbstractIdType[] _Id;
    }
}
