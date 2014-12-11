using System;

namespace RimDev.Sandbox
{
    public interface IGrain : IDisposable
    {
        string Name { get; }
        void Setup(Sandbox sandbox);
    }
}