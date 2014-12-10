using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Sandbox.Tests
{
    public class SandboxTests : IDisposable
    {
        public Sandbox Sandbox { get; set; }

        public SandboxTests()
        {
            Sandbox = new Sandbox();
        }

        [Fact]
        public void Can_create_a_sandbox()
        {
            Assert.NotNull(Sandbox);
        }

        [Fact]
        public void Can_play_in_a_sandbox()
        {
            Assert.NotNull(Sandbox.Play());
        }

        [Fact]
        public void Can_setup_a_grain()
        {
            using (var sb = new Sandbox())
            {
                var sandbox = sb as dynamic;
                sandbox.Test = new TestGrain();
                sandbox.Play();

                Assert.True(sandbox.Test.IsSetup);
            }
        }

        [Fact]
        public void Can_dispose_of_grains()
        {
            var sandbox = new Sandbox() as dynamic;
            sandbox.Test = new TestGrain();
            sandbox.Dispose();
            Assert.True(sandbox.Test.IsDisposed);
        }

        [Fact]
        public void Prinicipal_is_everyone_by_default()
        {
            Assert.Equal("Everyone", Sandbox.Principal);
        }

        [Fact]
        public void Can_set_prinicipal()
        {
            Sandbox.SetPermissionPrinicipal("Test");
            Assert.Equal("Test", Sandbox.Principal);
        }

        [Fact]
        public void Can_create_grain_directory()
        {
            var path = Sandbox.CreateGrainDirectory(new TestGrain());
            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void Can_get_strongly_typed_grain()
        {
            using (var sb = new Sandbox())
            {
                var sandbox = sb as dynamic;
                sandbox.Test = new TestGrain();

                Assert.NotNull(sb.Get<TestGrain>("Test"));
            }
        }

        [Fact]
        public void Get_returns_null_if_not_found()
        {
            Assert.Null(Sandbox.Get<TestGrain>("nope"));
        }

        [Fact]
        public void Exists_returns_true_if_grain_exists()
        {
            Sandbox.Add(new TestGrain());
            Assert.True(Sandbox.Exists("TestGrain"));
        }

        [Fact]
        public void Exists_throws_exception_if_grain_exists_and_throwOnExists()
        {
            Assert.Throws<ArgumentException>(() => {
                Sandbox.Add(new TestGrain());
                Assert.True(Sandbox.Exists("TestGrain", throwOnExists: true));
            });
        }

        public void Dispose()
        {
            Sandbox.Dispose();
        }
    }

    public class TestGrain : IGrain
    {
        public TestGrain()
        {
            Name = "TestGrain";
        }

        public bool IsDisposed { get; protected set; }
        public bool IsSetup { get; protected set; }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public string Name { get; set; }

        public void Setup(Sandbox sandbox)
        {
            Debug.WriteLine("Setting up Test Grain.");
            IsSetup = true;
        }
    }
}
