using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Models
{
    public class grant_typeC : serverC
    {
        public int grant_type_id { get; set; }
        public string grant_type_name { get; set; }
        public int no_of_target_groups { get; set; }
        public int no_of_grantees { get; set; }
        public int no_of_users { get; set; }
        public int media_count { get; set; }
        public int publication_count { get; set; }
        public int story_count { get; set; }
    }
}
