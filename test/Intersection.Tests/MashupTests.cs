using System.IO;
using System.Net;
using RimDev.Automation.Transform;
using Xunit;

namespace RimDev.Sandbox.Intersection
{
    public class MashupTests
    {
        public const string Aspx =
@"
<%@ Page Language=""C#""%> 
 <html>
 <head>
 <title>Hello World!</title>
 </head>
 <body>
   <% 
    var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[""test""].ConnectionString;
    using (var connection = new System.Data.SqlClient.SqlConnection(connectionString)) {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = ""Select PersonID, FirstName, LastName from Persons"";
        using (var reader = command.ExecuteReader()) {
            if (reader.HasRows) {
                while(reader.Read()) {
                    Response.Write(""<p>"" + reader.GetInt32(0) + "" "" + reader.GetString(1) + "" "" + reader.GetString(2) + ""</p>"");
                }
            }
            else 
            {
                Response.Write(""<p>No rows found.</p>"");
            }
        }
    }    
   %>
 </body>
 </html>";

        [Fact]
        public void Can_use_iisexpress_localdb_and_transform()
        {
            var path = CreateTempTestDirectory(Aspx);
            var sandbox = 
                new Sandbox()
                    .UseTranform()
                    .UseIisExpress(path)
                    .UseLocalDb();

            using (sandbox.Play())
            {
                var box = sandbox as dynamic;
                var webConfig = Path.Combine(box.IisExpress.WorkingDirectory, "web.config") as string;
                var transform = box.Transform.Instance as ConfigurationTransformer;
                var database = box.LocalDb.Instance as RimDev.Automation.Sql.LocalDb;

                transform
                    .SetSourceFromFile(webConfig)
                    .Transform.InsertConnectionString("test", database.ConnectionString);

                transform.Apply(webConfig);

                using (var connection = database.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                        "CREATE TABLE Persons (PersonID int,LastName varchar(255),FirstName varchar(255));";
                    command.ExecuteNonQuery();
                }

                using (var connection = database.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                        "Insert Into Persons (PersonID, LastName, FirstName) Values (1, 'Test', 'User');";
                    command.ExecuteNonQuery();
                }

                var url = string.Format("{0}/index.aspx",box.IisExpress.HttpEndpoint);
                var response = Get(url);

                Assert.Contains("Test", response);
                Assert.Contains("User", response);
                Assert.Contains("1", response);
            }
        }

        private string CreateTempTestDirectory(string content)
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(Path.Combine(path, "index.aspx"), content);
            File.WriteAllText(Path.Combine(path, "web.config"), "<?xml version=\"1.0\"?><configuration><system.web><compilation debug=\"true\"/></system.web><connectionStrings/><system.webServer><defaultDocument enabled=\"true\"><files><clear /><add value=\"index.html\" /></files></defaultDocument></system.webServer></configuration>");

            return path;
        }

        private static string Get(string url)
        {
            var request = WebRequest.CreateHttp(url);
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
