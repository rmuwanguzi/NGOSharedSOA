
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

using Voice.Service.Interfaces;
using Voice.Service;
using Voice.DbBase;

namespace Voice.Server.Platform_Specific
{
    public class SqlServerDbContextHelper : IDbContextHelper
    {
        SqlConnectionStringBuilder builder;
        
        public SqlServerDbContextHelper()
        {
            fnn.DB_SCHEMA = "voice_dev2";
            builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = fn.CONN_STRING;
        }
        public BaseContext GetContext()
        {
            return new BaseContext(new SqlConnection(fn.CONN_STRING), true, fnn.DB_SCHEMA);

        }

        public DbConnection GetDBConnection()
        {
            return new SqlConnection(fn.CONN_STRING);
        }

        public DbParameter GetDbParameters(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value);
        }
        public bool? IsSaveDuplicateRecordError(DbUpdateException ex)
        {
            var _sqlException = ex.InnerException as SqlException;
            if (_sqlException == null)
            {
                _sqlException = ex.InnerException.InnerException as SqlException;
            }

            if (_sqlException != null && _sqlException.Errors.OfType<SqlError>()
                .Any(se => se.Number == 2601 || se.Number == 2627 ))/* PK/UKC violation */
            {
                return true;
               
            }
            return false;
        }
        public bool? IsUpdateDuplicateRecordError(DbException ex)
        {
            var _ex = ex as SqlException;
            if (_ex != null && _ex.ErrorCode == 2601)
            {
                return true;
            }
            return null;
        }
    }
}