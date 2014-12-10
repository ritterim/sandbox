using System;

namespace Sandbox.IisExpress
{
    public static class IisExpressGrainExtensions
    {
        public const string DefaultName = "IisExpress";

        public static Sandbox UseIisExpress(this Sandbox sandbox, string source)
        {
            return UseIisExpress(sandbox, DefaultName, (sb, cfg) => cfg.WithSource(source));
        }

        public static Sandbox UseIisExpress(this Sandbox sandbox, string name = DefaultName, Action<dynamic, IIisExpressConfiguration> config = null)
        {
            var grain = new IisExpressGrain(name);
            sandbox.Add(grain);

            if (config != null)
                config((dynamic)sandbox, grain);

            return sandbox;
        }
    }
}