
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Voice.Shared.Core.Interfaces
{
    public interface IVoiceUserService : IService, IServiceCore<dto_voice_userC, dto_voice_user_updateC>
    {
        Task<dto.dto_voice_userC> AddNew(dto_voice_user_newC _dto);
        Task<dto.dto_voice_userC> AddNewSubAdmin(dto_voice_sub_admin_newC _dto);
        Task<bool> UpdatePcUserRights(dto_user_rights_updateC _dto);
        Task<bool> ChangeUserPwd(dto_change_password _dto);
        Task<bool> ChangeUserPcStatus(dto_user_login_access_updateC _dto);
        Task<List<dto_voice_userC>> GetUsersByTargetGroup(int target_group_id, long fs_timestamp);
        Task<List<dto_voice_userC>> GetUsersByGrantee(int grantee_id, long fs_timestamp);
        Task<dto_UserPageRightsDataC> GetUserRights(int user_id);
        Task<List<dto_voice_userC>> GetUsersByGrantType(int grant_type_id,long fs_timestamp);
        Task<dto.dto_voice_userC> UpdateB(dto_voice_user_updateC _dto);

    }
}
