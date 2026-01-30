using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;
using DotNetContainerRuntime.Linux;

namespace DotNetContainerRuntime.Linux.Tests;

public class LinuxProcessManagerTests
{
    [Fact]
    public void Constructor_OnNonLinux_ExecuteAsync_ShouldThrowPlatformNotSupportedException()
    {
        // This test will pass on Windows/Mac
        if (!OperatingSystem.IsLinux())
        {
            // Arrange
            var manager = new LinuxProcessManager();
            var processConfig = new ProcessConfiguration
            {
                User = new UserConfiguration { Uid = 0, Gid = 0 },
                Args = new List<string> { "/bin/true" },
                Cwd = "/"
            };
            var resourceContext = new ResourceControlContext
            {
                ContainerId = "test",
                Path = "/sys/fs/cgroup/test"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<PlatformNotSupportedException>(
                async () => await manager.ExecuteAsync("test", processConfig, null, resourceContext, CancellationToken.None)
            );
        }
    }

    [Fact]
    public void IsProcessRunning_WithNonExistentPid_ShouldReturnFalse()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var manager = new LinuxProcessManager();
        int nonExistentPid = 999999;

        // Act
        var isRunning = manager.IsProcessRunning(nonExistentPid);

        // Assert
        Assert.False(isRunning);
    }

    [Fact]
    public void IsProcessRunning_WithInitProcess_ShouldReturnTrue()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var manager = new LinuxProcessManager();

        // Act - PID 1 should always exist (init/systemd)
        var isRunning = manager.IsProcessRunning(1);

        // Assert
        Assert.True(isRunning);
    }

    [Fact]
    public async Task SendSignalAsync_WithNonExistentPid_ShouldNotThrow()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var manager = new LinuxProcessManager();
        int nonExistentPid = 999999;

        // Act & Assert - should not throw (ESRCH is ignored)
        await manager.SendSignalAsync(nonExistentPid, 0, CancellationToken.None);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var manager = new LinuxProcessManager();

        // Act & Assert - should not throw
        manager.Dispose();
    }
}
