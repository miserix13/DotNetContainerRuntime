using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;
using DotNetContainerRuntime.Linux;
using Moq;

namespace DotNetContainerRuntime.Linux.Tests;

public class LinuxContainerRuntimeTests
{
    [Fact]
    public void Constructor_OnNonLinux_ShouldThrowPlatformNotSupportedException()
    {
        // This test will pass on Windows/Mac
        if (!OperatingSystem.IsLinux())
        {
            // Act & Assert
            var exception = Assert.Throws<PlatformNotSupportedException>(() => new LinuxContainerRuntime());
            Assert.Contains("Linux", exception.Message);
        }
    }

    [Fact]
    public void Constructor_WithCustomManagers_ShouldAcceptThem()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var mockNamespaceManager = new Mock<INamespaceManager>();
        var mockResourceController = new Mock<IResourceController>();
        var mockFilesystemManager = new Mock<IFilesystemManager>();
        var mockProcessManager = new Mock<IProcessManager>();

        // Act
        var runtime = new LinuxContainerRuntime(
            mockNamespaceManager.Object,
            mockResourceController.Object,
            mockFilesystemManager.Object,
            mockProcessManager.Object
        );

        // Assert
        Assert.NotNull(runtime);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBundlePath_ShouldThrowDirectoryNotFoundException()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var runtime = new LinuxContainerRuntime();
        var nonExistentPath = "/tmp/non-existent-bundle-" + Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            async () => await runtime.CreateAsync("test-container", nonExistentPath, CancellationToken.None)
        );
    }

    [Fact]
    public async Task StartAsync_WithoutCreate_ShouldThrowInvalidOperationException()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var runtime = new LinuxContainerRuntime();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await runtime.StartAsync("non-existent-container", CancellationToken.None)
        );
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_WithNoContainers_ShouldReturnEmptyCollection()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var runtime = new LinuxContainerRuntime();

        // Act
        var containers = await runtime.ListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(containers);
        Assert.Empty(containers);
    }

    [Fact]
    public async Task GetStateAsync_WithNonExistentContainer_ShouldThrowInvalidOperationException()
    {
        // Skip if not on Linux
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Arrange
        var runtime = new LinuxContainerRuntime();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await runtime.GetStateAsync("non-existent", CancellationToken.None)
        );
        Assert.Contains("not found", exception.Message);
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
        var runtime = new LinuxContainerRuntime();

        // Act & Assert - should not throw
        runtime.Dispose();
    }
}
