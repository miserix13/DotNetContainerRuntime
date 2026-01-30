using DotNetContainerRuntime.Core.Abstractions;
using Xunit;

namespace DotNetContainerRuntime.Core.Tests.Abstractions;

public class NamespaceTypeTests
{
    [Theory]
    [InlineData(NamespaceType.Pid)]
    [InlineData(NamespaceType.Network)]
    [InlineData(NamespaceType.Mount)]
    [InlineData(NamespaceType.Ipc)]
    [InlineData(NamespaceType.Uts)]
    [InlineData(NamespaceType.User)]
    [InlineData(NamespaceType.Cgroup)]
    [InlineData(NamespaceType.Time)]
    public void NamespaceType_AllValues_ShouldBeDefined(NamespaceType namespaceType)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(NamespaceType), namespaceType));
    }

    [Fact]
    public void NamespaceType_ShouldHaveExpectedCount()
    {
        // Arrange
        var values = Enum.GetValues<NamespaceType>();

        // Assert - 8 namespace types
        Assert.Equal(8, values.Length);
    }
}

public class RuntimePlatformTests
{
    [Theory]
    [InlineData(RuntimePlatform.Linux)]
    [InlineData(RuntimePlatform.Windows)]
    public void RuntimePlatform_AllValues_ShouldBeDefined(RuntimePlatform platform)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(RuntimePlatform), platform));
    }

    [Fact]
    public void RuntimePlatform_ShouldHaveTwoPlatforms()
    {
        // Arrange
        var values = Enum.GetValues<RuntimePlatform>();

        // Assert
        Assert.Equal(2, values.Length);
    }
}

public class ResourceLimitsTests
{
    [Fact]
    public void ResourceLimits_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var limits = new ResourceLimits();

        // Assert
        Assert.Null(limits.MemoryLimit);
        Assert.Null(limits.MemoryReservation);
        Assert.Null(limits.MemorySwap);
        Assert.Null(limits.CpuShares);
        Assert.Null(limits.CpuQuota);
        Assert.Null(limits.CpuPeriod);
        Assert.Null(limits.CpuSet);
        Assert.Null(limits.BlockIOWeight);
        Assert.Null(limits.PidsLimit);
    }

    [Fact]
    public void ResourceLimits_WithMemory_ShouldSetCorrectly()
    {
        // Arrange & Act
        var limits = new ResourceLimits
        {
            MemoryLimit = 536870912, // 512MB
            MemoryReservation = 268435456 // 256MB
        };

        // Assert
        Assert.Equal(536870912, limits.MemoryLimit);
        Assert.Equal(268435456, limits.MemoryReservation);
    }

    [Fact]
    public void ResourceLimits_WithCpu_ShouldSetCorrectly()
    {
        // Arrange & Act
        var limits = new ResourceLimits
        {
            CpuShares = 1024,
            CpuQuota = 50000,
            CpuPeriod = 100000,
            CpuSet = "0-3"
        };

        // Assert
        Assert.Equal(1024ul, limits.CpuShares);
        Assert.Equal(50000, limits.CpuQuota);
        Assert.Equal(100000ul, limits.CpuPeriod);
        Assert.Equal("0-3", limits.CpuSet);
    }
}

public class ResourceUsageTests
{
    [Fact]
    public void ResourceUsage_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var usage = new ResourceUsage();

        // Assert
        Assert.Equal(0, usage.MemoryUsage);
        Assert.Equal(0, usage.MemoryMaxUsage);
        Assert.Equal(0ul, usage.CpuUsage);
        Assert.Equal(0, usage.ProcessCount);
    }

    [Fact]
    public void ResourceUsage_WithValues_ShouldSetCorrectly()
    {
        // Arrange & Act
        var usage = new ResourceUsage
        {
            MemoryUsage = 104857600, // 100MB
            MemoryMaxUsage = 209715200, // 200MB
            CpuUsage = 1234567890,
            ProcessCount = 5
        };

        // Assert
        Assert.Equal(104857600, usage.MemoryUsage);
        Assert.Equal(209715200, usage.MemoryMaxUsage);
        Assert.Equal(1234567890ul, usage.CpuUsage);
        Assert.Equal(5, usage.ProcessCount);
    }
}

public class NamespaceContextTests
{
    [Fact]
    public void NamespaceContext_RequiredProperties_ShouldBeSet()
    {
        // Arrange
        var fileDescriptors = new Dictionary<NamespaceType, int>
        {
            [NamespaceType.Pid] = 10,
            [NamespaceType.Network] = 11
        };
        
        var paths = new Dictionary<NamespaceType, string>
        {
            [NamespaceType.Pid] = "/proc/12345/ns/pid",
            [NamespaceType.Network] = "/proc/12345/ns/net"
        };

        // Act
        var context = new NamespaceContext
        {
            FileDescriptors = fileDescriptors,
            Paths = paths
        };

        // Assert
        Assert.Equal(2, context.FileDescriptors.Count);
        Assert.Equal(10, context.FileDescriptors[NamespaceType.Pid]);
        Assert.Equal(2, context.Paths.Count);
        Assert.Equal("/proc/12345/ns/pid", context.Paths[NamespaceType.Pid]);
    }
}

public class FilesystemContextTests
{
    [Fact]
    public void FilesystemContext_RequiredProperties_ShouldBeSet()
    {
        // Arrange & Act
        var context = new FilesystemContext
        {
            ContainerId = "test-container",
            MountPath = "/var/lib/containers/test-container/merged",
            ReadOnly = false
        };

        // Assert
        Assert.Equal("test-container", context.ContainerId);
        Assert.Equal("/var/lib/containers/test-container/merged", context.MountPath);
        Assert.False(context.ReadOnly);
        Assert.Empty(context.MountPoints);
    }

    [Fact]
    public void FilesystemContext_WithOverlay_ShouldSetPaths()
    {
        // Arrange & Act
        var context = new FilesystemContext
        {
            ContainerId = "test-container",
            MountPath = "/var/lib/containers/test-container/merged",
            UpperPath = "/var/lib/containers/test-container/upper",
            WorkPath = "/var/lib/containers/test-container/work",
            ReadOnly = false
        };

        // Assert
        Assert.Equal("/var/lib/containers/test-container/upper", context.UpperPath);
        Assert.Equal("/var/lib/containers/test-container/work", context.WorkPath);
    }
}

public class ProcessContextTests
{
    [Fact]
    public void ProcessContext_RequiredProperties_ShouldBeSet()
    {
        // Arrange & Act
        var context = new ProcessContext
        {
            ContainerId = "test-container",
            Pid = 12345
        };

        // Assert
        Assert.Equal("test-container", context.ContainerId);
        Assert.Equal(12345, context.Pid);
        Assert.True(context.StartTime <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ProcessContext_StartTime_ShouldBeInitialized()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        
        // Act
        var context = new ProcessContext
        {
            ContainerId = "test",
            Pid = 1
        };
        
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(context.StartTime >= before);
        Assert.True(context.StartTime <= after);
    }
}

public class ResourceControlContextTests
{
    [Fact]
    public void ResourceControlContext_RequiredProperties_ShouldBeSet()
    {
        // Arrange & Act
        var context = new ResourceControlContext
        {
            ContainerId = "test-container",
            Path = "/sys/fs/cgroup/test-container"
        };

        // Assert
        Assert.Equal("test-container", context.ContainerId);
        Assert.Equal("/sys/fs/cgroup/test-container", context.Path);
        Assert.Null(context.Handle);
    }

    [Fact]
    public void ResourceControlContext_WithHandle_ShouldSetCorrectly()
    {
        // Arrange & Act
        var handle = new IntPtr(123456);
        var context = new ResourceControlContext
        {
            ContainerId = "test-container",
            Path = "JobObject-test",
            Handle = handle
        };

        // Assert
        Assert.Equal(handle, context.Handle);
    }
}
