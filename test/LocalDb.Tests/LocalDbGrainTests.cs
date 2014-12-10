using System.Data.SqlClient;
using Xunit;

namespace Sandbox.LocalDb.Tests
{
    public class LocalDbGrainTests
    {
        [Fact]
        public void Can_register_with_sandbox_using_defaults()
        {
            var sandbox = new Sandbox().UseLocalDb();
            Assert.True(sandbox.Exists("LocalDb"));
        }

        [Fact]
        public void Can_register_with_sandbox_using_other_name()
        {
            var sandbox = new Sandbox().UseLocalDb(name: "Test");
            Assert.True(sandbox.Exists("Test"));
        }

        [Fact]
        public void Can_access_localdb_from_grain()
        {
            using (var sandbox = new Sandbox().UseLocalDb().Play())
            {
                var sb = sandbox as dynamic;
                using (SqlConnection connection = sb.LocalDb.Instance.OpenConnection())
                {
                    var databale = connection.GetSchema();
                    Assert.NotNull(databale);
                }
            }
        }
    }
}
