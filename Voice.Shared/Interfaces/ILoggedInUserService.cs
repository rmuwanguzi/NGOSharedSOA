using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core.dto;

namespace Voice.Shared.Core.Interfaces
{
   public interface ILoggedInUserService : IService
    {
        dto_logged_user GetLoggedInUser();

    }
}
