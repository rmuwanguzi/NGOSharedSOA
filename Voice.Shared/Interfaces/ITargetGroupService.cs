using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Interfaces
{
   public interface ITargetGroupService : IService, IServiceCore<target_groupC, dto_target_group_updateC>
    {
        Task<target_groupC> AddNew(dto_target_group_newC _dto);
        Task<List<target_groupC>> GetAllTargetGroupGrantType(int grant_type_id);
        Task<List<target_groupC>> GetAllTargetGroupsByGrantee(int grantee_id);

    }
}
