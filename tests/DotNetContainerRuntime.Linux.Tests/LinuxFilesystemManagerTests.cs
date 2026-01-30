using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Linux;

namespace DotNetContainerRuntime.Linux.Tests;

public class LinuxFilesystemManagerTests
{
    [Fact]
    public void Constructor_OnNonLinux_ShouldThrowPlatformNotSupportedException()
    {
        // This test will pass on Windows/Mac and be skipped on Linux
        if (!OperatingSystem.IsLinux())
        {
            // Act & Assert
            var exception = Assert.Throws<PlatformNotSupportedException>(() => new LinuxFilesystemManager());
            Assert.Contains("Linux", exception.Message);
        }
    }

    [Fact]
    public void Constructor_WithCustomStorageRoot_ShouldUseProvidedPath()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange & Act
        var customPath = "/tmp/test-storage";
        var manager = new LinuxFilesystemManager(customPath);

        // Assert - just verify it doesn't throw
        Assert.NotNull(manager);
    }

    [Fact]
    public async Task PrepareRootfsAsync_WithReadOnlyTrue_ShouldReturnContextWithReadOnly()
    {
        // Skip if not on Linux or not root
        if (!OperatingSystem.IsLinux() || Environment.GetEnvironmentVariable("USER") != "root")
        {
            return;
        }

        // Arrange
        var manager = new LinuxFilesystemManager("/tmp/test-storage");
        var tempRootfs = Path.Combine(Path.GetTempPath(), "test-rootfs");
        Directory.CreateDirectory(tempRootfs);

        try
        {
            // Act
            var context = await manager.PrepareRootfsAsync("test-container", tempRootfs, readOnly: true, CancellationToken.None);

            // Assert
            Assert.NotNull(context);
            Assert.Equal("test-container", context.ContainerId);
            Assert.True(context.ReadOnly);
            Assert.Null(context.UpperPath); // Read-only doesn't need upper layer
            Assert.Null(context.WorkPath); // Read-only doesn't need work directory

            // Cleanup
            await manager.CleanupAsync(context, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected if not running as root
        }
        finally
        {
            if (Directory.Exists(tempRootfs))
            {
                Directory.Delete(tempRootfs, recursive: true);
            }
        }
    }

    [Fact]
    public async Task PrepareRootfsAsync_WithReadOnlyFalse_ShouldReturnContextWithUpperAndWork()
    {
        // Skip if not on Linux or not root
        if (!OperatingSystem.IsLinux() || Environment.GetEnvironmentVariable("USER") != "root")
        {
            return;
        }

        // Arrange
        var manager = new LinuxFilesystemManager("/tmp/test-storage");
        var tempRootfs = Path.Combine(Path.GetTempPath(), "test-rootfs");
        Directory.CreateDirectory(tempRootfs);

        try
        {
            // Act
            var context = await manager.PrepareRootfsAsync("test-container-rw", tempRootfs, readOnly: false, CancellationToken.None);

            // Assert
            Assert.NotNull(context);
            Assert.Equal("test-container-rw", context.ContainerId);
            Assert.False(context.ReadOnly);
            Assert.NotNull(context.UpperPath);
            Assert.NotNull(context.WorkPath);

            // Cleanup
            await manager.CleanupAsync(context, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected if not running as root
        }
        finally
        {
            if (Directory.Exists(tempRootfs))
            {
                Directory.Delete(tempRootfs, recursive: true);
            }
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
        var manager = new LinuxFilesystemManager("/tmp/test-dispose");

        // Act & Assert - should not throw
        manager.Dispose();
    }
}
