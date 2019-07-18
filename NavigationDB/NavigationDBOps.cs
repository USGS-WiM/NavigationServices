using NavigationDB.Resources;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using WIM.Utilities;

namespace NavigationDB
{
    public class NavigationDBOps : dbOps
    {
        #region Properties
        #endregion
        public NavigationDBOps(string pSQLconnstring)
            : base()
        {
            init(pSQLconnstring);
        }
        #region Methods
        public IEnumerable< Dictionary<string, object>> GetAvailableProperties(List<int>Comids)
        {
            try
            {
                string sql = string.Format(@"SELECT * FROM ""vaa"".""Characteristics"" WHERE ""COMID"" IN ({0})",string.Join(",",Comids));
               var results = base.GetItems(sql);
                return results;
            }
            catch (System.Exception ex)
            {
                sm(ex.Message);
                return null;
            }

        }
        #endregion
        #region Helper Methods
        protected override DbCommand getCommand(string sql)
        {
            return new NpgsqlCommand(sql, (NpgsqlConnection)this.connection);
        }

        protected void init(string connectionString)
        {
            this.connection = new NpgsqlConnection(connectionString);

        }
        
        #endregion
    }
}
