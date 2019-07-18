using System;
using Xunit;
using NavigationDB;
using NavigationDB.Resources;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace NavigationAgent.Test
{
    public class DBOpsTest
    {
        IConfiguration Configuration { get; set; }
        public DBOpsTest()
        {
            // the type specified here is just so the secrets library can
            // find the UserSecretId we added in the csproj file
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<DBOpsTest>();
            Configuration = builder.Build();
        }
        [Fact]
        public void DBTableTest()
        {
            try
            {
                List<Characteristics> results = null;
                var username = Configuration["dbuser"];
                var password = Configuration["dbpassword"];

                string DBConnectionstring = string.Format("Server=test.c69uuui2tzs0.us-east-1.rds.amazonaws.com; database={0}; UID={1}; password={2}", "NavigationDB", username, password);

                using (var dbOps = new NavigationDBOps(DBConnectionstring))
                {
                    results = dbOps.GetCharacteristics(new List<int>() { -504212, -504197 }).ToList();
                }

                Assert.NotNull(results);
                Assert.Equal(2, results.Count);
            }
            catch (Exception ex)
            {

                Assert.False(true, ex.Message);
            }
        }
    }
}
