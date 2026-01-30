using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Linux;

namespace DotNetContainerRuntime.Linux.Tests;

public class LinuxCgroupControllerTests
{
    [Fact]
    public void Constructor_OnNonLinux_ShouldThrowPlatformNotSupportedException()
    {
        // This test will pass on Windows/Mac and be skipped on Linux
        if (!OperatingSystem.IsLinux())
        {
            // Act & Assert
            var exception = Assert.Throws<PlatformNotSupportedException>(() => new LinuxCgroupController());
            Assert.Contains("Linux", exception.Message);
        }
    }

    [Fact]
    public void Constructor_WithCustomBasePath_ShouldUseProvidedPath()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange & Act
        var customPath = "/tmp/test-cgroups";
        var controller = new LinuxCgroupController(customPath);

        // Assert - just verify it doesn't throw
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task CreateResourceGroupAsync_WithValidLimits_ShouldReturnContext()
    {
        // Skip if not on Linux or not root
        if (!OperatingSystem.IsLinux() || Environment.GetEnvironmentVariable("USER") != "root")
        {
            return;
        }

        // Arrange
        var controller = new LinuxCgroupController("/tmp/test-cgroups");
        var limits = new ResourceLimits
        {
            MemoryLimit = 100_000_000,
            CpuShares = 1024
        };

        try
        {
            // Act
            var context = await controller.CreateResourceGroupAsync("test-container", limits, CancellationToken.None);

            // Assert
            Assert.NotNull(context);
            Assert.Equal("test-container", context.ContainerId);
            Assert.Contains("test-container", context.Path);

            // Cleanup
            await controller.DeleteResourceGroupAsync(context, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected if not running as root
        }
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var controller = new LinuxCgroupController();

        // Act & Assert - should not throw
        controller.Dispose();
    }
}
