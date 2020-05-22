using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Models
{
    public class assign_grantee_to_target_groupC : serverC
    {
        public int grantee_id { get; set; }
        public int target_gp_id { get; set; }
        public int grant_type_id { get; set; }
        public int un_id { get; set; }
    }
}
