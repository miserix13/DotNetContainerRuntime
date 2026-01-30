using DotNetContainerRuntime.Core.Specifications;
using Xunit;

namespace DotNetContainerRuntime.Core.Tests.Specifications;

public class OciVersionTests
{
    [Fact]
    public void OciVersion_RuntimeSpec_ShouldBeValid()
    {
        // Assert
        Assert.NotNull(OciVersion.RuntimeSpec);
        Assert.NotEmpty(OciVersion.RuntimeSpec);
        Assert.Matches(@"^\d+\.\d+\.\d+$", OciVersion.RuntimeSpec);
    }

    [Fact]
    public void OciVersion_ImageSpec_ShouldBeValid()
    {
        // Assert
        Assert.NotNull(OciVersion.ImageSpec);
        Assert.NotEmpty(OciVersion.ImageSpec);
        Assert.Matches(@"^\d+\.\d+\.\d+$", OciVersion.ImageSpec);
    }

    [Fact]
    public void OciVersion_RuntimeSpec_ShouldBe_1_2_0()
    {
        // Assert
        Assert.Equal("1.2.0", OciVersion.RuntimeSpec);
    }

    [Fact]
    public void OciVersion_ImageSpec_ShouldBe_1_1_0()
    {
        // Assert
        Assert.Equal("1.1.0", OciVersion.ImageSpec);
    }
}
