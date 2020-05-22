using Voice.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace Voice.Server.Platform_Specific
{
    public class MessageDialog : IMessageDialog
    { 
       
        public void ErrorMessage(string message, string title = null)
        {
            throw new NotImplementedException();
        }
        public object tag { get; set; }
        public void ErrorMessage(string key, string message, string controller_key = null)
        {
            if (!string.IsNullOrWhiteSpace(controller_key))
            {
                var modelState = datam.DATA_CONTROLLER_MODEL_STATE[controller_key];
                modelState.AddModelError(controller_key, message);
            }
         }
    }
}