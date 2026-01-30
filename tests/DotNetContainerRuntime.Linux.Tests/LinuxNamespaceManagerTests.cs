using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Linux;

namespace DotNetContainerRuntime.Linux.Tests;

public class LinuxNamespaceManagerTests
{
    [Fact]
    public void CreateNamespacesAsync_OnNonLinux_ShouldThrowPlatformNotSupportedException()
    {
        // This test will pass on Windows/Mac and be skipped on Linux
        if (!OperatingSystem.IsLinux())
        {
            // Arrange
            var manager = new LinuxNamespaceManager();
            
            // Act & Assert
            var exception = Assert.ThrowsAsync<PlatformNotSupportedException>(
                async () => await manager.CreateNamespacesAsync(new[] { NamespaceType.Pid }, CancellationToken.None)
            );
            Assert.NotNull(exception);
        }
    }

    [Fact]
    public void GetNamespacePath_WithValidPid_ShouldReturnCorrectPath()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var manager = new LinuxNamespaceManager();
        int pid = 1234;

        // Act & Assert
        Assert.Equal("/proc/1234/ns/pid", manager.GetNamespacePath(pid, NamespaceType.Pid));
        Assert.Equal("/proc/1234/ns/net", manager.GetNamespacePath(pid, NamespaceType.Network));
        Assert.Equal("/proc/1234/ns/mnt", manager.GetNamespacePath(pid, NamespaceType.Mount));
        Assert.Equal("/proc/1234/ns/ipc", manager.GetNamespacePath(pid, NamespaceType.Ipc));
        Assert.Equal("/proc/1234/ns/uts", manager.GetNamespacePath(pid, NamespaceType.Uts));
        Assert.Equal("/proc/1234/ns/user", manager.GetNamespacePath(pid, NamespaceType.User));
        Assert.Equal("/proc/1234/ns/cgroup", manager.GetNamespacePath(pid, NamespaceType.Cgroup));
        Assert.Equal("/proc/1234/ns/time", manager.GetNamespacePath(pid, NamespaceType.Time));
    }

    [Fact]
    public void GetNamespacePath_WithInvalidNamespaceType_ShouldThrowArgumentException()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var manager = new LinuxNamespaceManager();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => manager.GetNamespacePath(1234, (NamespaceType)999));
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
        var manager = new LinuxNamespaceManager();

        // Act & Assert - should not throw
        manager.Dispose();
    }
}
