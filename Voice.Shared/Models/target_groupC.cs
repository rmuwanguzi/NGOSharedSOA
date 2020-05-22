

namespace Voice.Shared.Core.Models
{
    public class target_groupC : serverC
    {
        public int target_gp_id { get; set; }
        public string target_gp_name { get; set; }
        public int no_of_grantees { get; set; }
        public string target_gp_image_url { get; set; }
        public int no_of_users { get; set; }
        public string alias { get; set; }
        public int media_count { get; set; }
        public int publication_count { get; set; }
        public int story_count { get; set; }
    }
}
