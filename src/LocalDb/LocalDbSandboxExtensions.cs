using System;

namespace Sandbox.LocalDb
{
    public static class LocalDbSandboxExtensions
    {
        public static Sandbox UseLocalDb(this Sandbox sandbox, string name = "LocalDb", Action<dynamic, ILocalDbConfiguration> config = null)
        {
            var grain = new LocalDbGrain(name);
            sandbox.Add(grain);

            if (config != null)
                config((dynamic)sandbox, grain);

            return sandbox;
        }
    }
}