using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core.Models;

namespace Voice.Shared.Core.Interfaces
{
   public  interface ITokenService : IService
    {
        string GenerateToken(voice_userC _obj, double duration_minutes);
        ClaimsPrincipal ValidateToken(string access_token);
    }
}
