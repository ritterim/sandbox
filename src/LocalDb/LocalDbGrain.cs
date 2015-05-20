using System;
using System.Diagnostics;
using System.Linq;

namespace RimDev.Sandbox.LocalDb
{
    [DebuggerDisplay("LocalDb Grain <{Name}>")]
    public class LocalDbGrain : IGrain, ILocalDbConfiguration
    {
        public LocalDbGrain(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            
            Name = name;
            DatabasePrefix = Name;
            Version = Automation.Sql.LocalDb.Versions.InstalledVersions.FirstOrDefault();
            DatabaseSuffixGenerator = () => string.Format("{0}_{1}", Guid.NewGuid().ToString("N"), DateTime.Now.Ticks);
        }

        public string Name { get; protected set; }
        public string Version { get; protected set; }
        public string DatabaseName { get; protected set; }
        public string DatabasePrefix { get; protected set; }
        public Func<string> DatabaseSuffixGenerator { get; protected set; }

        public Automation.Sql.LocalDb Instance { get; protected set; }

        public void Dispose()
        {
            if (Instance != null)
                Instance.Dispose();
        }

        public void Setup(Sandbox sandbox)
        {
            var location = sandbox.CreateGrainDirectory(this);
            Instance = new RimDev.Automation.Sql.LocalDb(DatabaseName, Version, location, DatabasePrefix, DatabaseSuffixGenerator);
        }

        public ILocalDbConfiguration WithVersion(string version)
        {
            if (version == null) throw new ArgumentNullException("version");

            Version = version;
            return this;
        }

        public ILocalDbConfiguration WithDatabaseName(string databaseName)
        {
            if (databaseName == null) throw new ArgumentNullException("databaseName");

            DatabaseName = databaseName;
            return this;
        }

        public ILocalDbConfiguration WithDatabasePrefix(string databasePrefix)
        {
            if (databasePrefix == null) throw new ArgumentNullException("databasePrefix");
            DatabasePrefix = databasePrefix;
            
            return this;
        }

        public ILocalDbConfiguration WithDatabaseSuffix(Func<string> databaseSuffixGenerator)
        {
            if (databaseSuffixGenerator == null) throw new ArgumentNullException("databaseSuffixGenerator");
            DatabaseSuffixGenerator = databaseSuffixGenerator;

            return this;
        }
    }
}