using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Models
{
    public class assign_voice_resource_to_mydriveC : serverC
    {
        public int user_id { get; set; }
        public int resource_id { get; set; }
        public int un_id { get; set; }
    }
}
