//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChangelogManager
{
    using System;
    using System.Collections.Generic;
    using Dapper.Contrib.Extensions;

    public class Service
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string Keywords { get; set; }
        public string Fees { get; set; }
        public string AccessConstraints { get; set; }
        public string ProviderName { get; set; }
        public string ProviderSite { get; set; }
        public string IndividualName { get; set; }
        public string Phone { get; set; }
        public string Facsimile { get; set; }
        public string Deliverypoint { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string EMail { get; set; }
        public string OnlineResourcesUrl { get; set; }
        public string HoursOfService { get; set; }
        public string ContactInstructions { get; set; }
        public string Role { get; set; }
        public string ServiceURL { get; set; }
        [Key]
        public string ServiceID { get; set; }
        public string Namespace { get; set; }
        public string SchemaLocation { get; set; }
    }
}
