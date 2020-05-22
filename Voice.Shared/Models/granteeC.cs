using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Models
{
    public class granteeC : serverC
    {
        public int grantee_id { get; set; }
        public string grantee_name { get; set; }
        public string company_email { get; set; }
        public string company_address { get; set; }
        public string contact_person { get; set; }
        public string contact_phone { get; set; }
        public string target_gp_ids { get; set; }
        public int grant_type_id { get; set; }
        public string grant_type_name { get; set; }
        public int no_of_users { get; set; }
        public string grantee_image_url { get; set; }//if we created
        public string alias { get; set; }
        public string json_target_groups { get; set; }
        public int target_gp_count { get; set; }//upate on insert and update
        public int media_count { get; set; }
        public int publication_count { get; set; }
        public int story_count { get; set; }
        public string grantee_profile_url { get; set; }
    }
}
