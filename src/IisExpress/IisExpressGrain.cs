using System;
using System.Diagnostics;
using System.IO;

namespace RimDev.Sandbox.IisExpress
{
    [DebuggerDisplay("IIS Express Grain <{Name}>")]
    public class IisExpressGrain : IGrain, IIisExpressConfiguration
    {
        public IisExpressGrain(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
        }

        public string Name { get; protected set; }
        public string Source { get; protected set; }
        public string WorkingDirectory { get; protected set; }
        public RimDev.Automation.WebHosting.IisExpress Instance { get; protected set; }

        public void Dispose()
        {
            if (Instance != null)
                Instance.Dispose();
        }

        public void Setup(Sandbox sandbox)
        {
            if (string.IsNullOrWhiteSpace(Source) || !Directory.Exists(Source))
                throw new ArgumentException("Source must be defined and must exist", "Source");
            
            var location = sandbox.CreateGrainDirectory(this);
            
            /* move contents to sandbox area */
            sandbox.CopyAll(new DirectoryInfo(Source), new DirectoryInfo(location));

            WorkingDirectory = location;
            Instance = RimDev.Automation.WebHosting.IisExpress.CreateServer(location, Name).Result;
            Instance.Start();
        }

        public string HttpEndpoint
        {
            get
            {
                return Instance == null
                    ? string.Empty
                    : string.Format("http://localhost:{0}", Instance.HttpPort);
            }
        }

        public string HttpsEndpoint
        {
            get
            {
                return Instance == null
                    ? string.Empty
                    : string.Format("https://localhost:{0}", Instance.HttpsPort);
            }
        }

        public IIisExpressConfiguration WithSource(string source)
        {
            if (source == null) throw new ArgumentNullException("source");
            Source = source;
            return this;
        }
    }
}