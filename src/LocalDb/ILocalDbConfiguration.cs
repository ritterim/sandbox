using System;

namespace RimDev.Sandbox.LocalDb
{
    public interface ILocalDbConfiguration
    {
        ILocalDbConfiguration WithVersion(string version);
        ILocalDbConfiguration WithDatabaseName(string databaseName);
        ILocalDbConfiguration WithDatabasePrefix(string databasePrefix);
        ILocalDbConfiguration WithDatabaseSuffix(Func<string> databaseSuffixGenerator);
    }
}