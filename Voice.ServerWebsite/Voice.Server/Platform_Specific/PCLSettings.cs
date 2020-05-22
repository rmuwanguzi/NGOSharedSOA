using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voice.Shared.Core.Interfaces;
namespace Voice.Server.Platform_Specific
{
    public class PCLSettings : IPCLSettings
    {
        public string BaseUrl
        {
            get; set;
        }
    }
}