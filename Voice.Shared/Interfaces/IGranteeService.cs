using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Interfaces
{
    public interface IGranteeService : IService, IServiceCore<granteeC, dto_grantee_update>
    {
        Task<granteeC> AddNew(dto_grantee_newC _dto);
       
        Task<List<granteeC>> GetAllGranteesByTargetGroup(int target_gp_id);
        Task<List<granteeC>> GetAllGranteesByGrantType(int grant_type_id);

    }
}
