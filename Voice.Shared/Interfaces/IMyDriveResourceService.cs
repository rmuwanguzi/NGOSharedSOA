using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;

namespace Voice.Shared.Core.Interfaces
{
    public interface IMyDriveResourceService : IService
    {
        Task<bool> AddResourceToMyDrive(int resource_id);
        Task<List<voice_resourceC>> GetAllMyDriveResourcesByUserId();
        Task<bool> RemoveResourceFromMyDrive(int resource_id);

    }
}
