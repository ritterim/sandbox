using System;

namespace RimDev.Sandbox.Transform
{
    public static class TransformExtensions
    {
        public const string DefaultName = "Transform";

        public static Sandbox UseTranform(this Sandbox sandbox, string name = DefaultName, Action<dynamic> config = null)
        {
            var grain = new TransformGrain(name);
            sandbox.Add(grain);

            if (config != null)
                config((dynamic) sandbox);

            return sandbox;
        }
    }
}
