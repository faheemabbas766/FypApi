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
    
    public partial class AllPost
    {
        public int post_id { get; set; }
        public Nullable<System.DateTime> post_date { get; set; }
        public string post_text { get; set; }
        public string post_image { get; set; }
        public string post_uc { get; set; }
        public string user_name { get; set; }
        public string user_cnic { get; set; }
        public string user_picture { get; set; }
        public string account_type { get; set; }
        public string position { get; set; }
        public int total_rating { get; set; }
        public string recent_comment { get; set; }
        public Nullable<System.DateTime> recent_comment_date { get; set; }
        public string status { get; set; }
    }
}
