using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Voice.DbBase;

namespace Voice.Server
{
    public class DbContextVoice : BaseContext
    {
        public DbContextVoice() : base(new SqlConnection(fn.CONN_STRING), true, "voice_dev2")
        {

        }
    }
}