//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FypApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AllReport
    {
        public int report_id { get; set; }
        public string reporter_cnic { get; set; }
        public string reporter_name { get; set; }
        public string reporter_picture { get; set; }
        public string reporter_gender { get; set; }
        public string reported_cnic { get; set; }
        public string reported_name { get; set; }
        public string reported_picture { get; set; }
        public string reported_gender { get; set; }
        public System.DateTime report_date { get; set; }
        public string report_status { get; set; }
        public string report_type { get; set; }
        public string report_reason { get; set; }
    }
}