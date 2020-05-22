using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Service.Interfaces
{
    public interface IKellerDBContext
    {
        KellermanSoftware.NetDataAccessLayer.DataHelper GetContext();
    }
}
