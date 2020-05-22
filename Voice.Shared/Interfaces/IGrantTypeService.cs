using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Shared.Core.Interfaces
{
    public interface IGrantTypeService : IService, IServiceCore<grant_typeC, dto_grant_type_updateC>
    {
        Task<grant_typeC> AddNew(dto_grant_type_newC _dto);
    }
}
