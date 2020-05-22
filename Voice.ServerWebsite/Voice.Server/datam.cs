using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace Voice.Server
{
    public class datam
    {
        public static SortedList<string, ModelStateDictionary> DATA_CONTROLLER_MODEL_STATE { get; set; }
    }
}