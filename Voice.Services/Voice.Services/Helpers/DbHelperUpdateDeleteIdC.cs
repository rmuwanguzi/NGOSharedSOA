using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voice.Service
{
  internal class DbHelperUpdateDeleteIdC
    {
        public string col_to_update { get; set; }
        public int new_col_value { get; set; }
        public string table_name { get; set; }
        public string pk_col_name { get; set; }
        public int pk_id { get; set; }
    }
}
