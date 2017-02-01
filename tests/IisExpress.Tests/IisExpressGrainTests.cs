using System;
using System.IO;
using System.Net;
using Xunit;

namespace RimDev.Sandbox.IisExpress
{
    public class IisExpressGrainTests
    {
        public const string Html = "<html><body><h1>Hello World!</h1></body></html>";

        public const string Aspx =
@"
<%@ Page Language=""C#""%> 
 <html>
 <head>
 <title>Hello World!</title>
 </head>
 <body>

   <% for (int i=1; i <7; i++) 
	{ %>
      <font size=""   <%=i%>""> C# inside aspx! </font> <br>
   <%   }
   Response.Write(""<p><cite>COOL</cite>"");
   %>
 </body>
 </html>";

        [Fact]
        public void Can_register_with_sandbox_using_defaults()
        {
            var sandbox = new Sandbox().UseIisExpress("");
            Assert.True(sandbox.Exists(IisExpressGrainExtensions.DefaultName));
        }

        [Fact]
        public void Can_resiter_with_sandbox_using_other_name()
        {
            var sandbox = new Sandbox().UseIisExpress("Test", (sb, cfg) => cfg.WithSource(""));
            Assert.True(sandbox.Exists("Test"));
        }

        [Fact]
        public void Can_spin_up_web_site()
        {
            var site = CreateTempTestDirectory();
            using (var sandbox = new Sandbox().UseIisExpress(site).Play())
            {
                var sb = sandbox as dynamic;
                var url = sb.IisExpress.HttpEndpoint;

                var response = Get(url);
                Assert.Equal(Html, response);
            }
        }

        [Fact]
        public void IisExpress_supports_aspx_page()
        {
            var site = CreateTempTestDirectory("aspx", Aspx);
            using (var sandbox = new Sandbox().UseIisExpress(site).Play())
            {
                var sb = sandbox as dynamic;
                var url = string.Format("{0}/index.aspx", sb.IisExpress.HttpEndpoint);

                var response = Get(url);
                Assert.Contains("COOL", response);
            }
        }

        private string CreateTempTestDirectory(string extension = "html", string content = Html)
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (Directory.Exists(path))
                throw new Exception("The temporary directory should not already exist.");
            else
                Directory.CreateDirectory(path);

            File.WriteAllText(Path.Combine(path, string.Format("index.{0}", extension)), content);
            File.WriteAllText(Path.Combine(path, "web.config"), "<?xml version=\"1.0\"?><configuration><system.webServer><defaultDocument enabled=\"true\"><files><clear /><add value=\"index.html\" /></files></defaultDocument></system.webServer></configuration>");

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
