using System;
using RimDev.Automation.Transform;

namespace Sandbox.Transform
{
    public class TransformGrain : IGrain
    {
        public TransformGrain(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
        }

        public ConfigurationTransformer Instance { get; protected set; }

        public void Dispose()
        {
            if (Instance != null)
                Instance.Dispose();
        }

        public string Name { get; protected set; }

        public void Setup(Sandbox sandbox)
        {
            Instance = new ConfigurationTransformer();
        }
    }
}
