using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voice.Server
{
    public static class ExtensionsController
    {
        public static string ToErrorMessage(this System.Web.Http.ModelBinding.ModelStateDictionary modelstate)
        {
            try
            {
                var  messages = string.Join("; ", modelstate.Values
                                         .SelectMany(x => x.Errors)
                                         .Select(x => x.ErrorMessage));
                    return messages;
                
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}