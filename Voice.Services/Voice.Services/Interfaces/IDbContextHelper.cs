
using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace Voice.Service.Interfaces
{
   public interface IDbContextHelper
    {
        DbBase.BaseContext GetContext();
        DbParameter GetDbParameters(string parameterName, object value);
        bool? IsSaveDuplicateRecordError(DbUpdateException ex);
        bool? IsUpdateDuplicateRecordError(DbException ex);
        DbConnection GetDBConnection();
    }
}
