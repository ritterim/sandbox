using Xunit;

namespace Sandbox.Transform.Tests
{
    public class TransformTests
    {
        [Fact]
        public void Can_register_with_sandbox_using_defaults()
        {
            var sandbox = new Sandbox().UseTranform();
            Assert.True(sandbox.Exists(TransformExtensions.DefaultName));
        }

        [Fact]
        public void Can_register_with_sandbox_using_other_name()
        {
            var sandbox = new Sandbox().UseTranform(name: "Test");
            Assert.True(sandbox.Exists("Test"));
        }

        [Fact]
        public void Can_access_transform_from_grain()
        {
            using (var sandbox = new Sandbox().UseTranform().Play())
            {
                var sb = sandbox as dynamic;
                Assert.NotNull(sb.Transform.Instance);
            }
        }
    }
}
