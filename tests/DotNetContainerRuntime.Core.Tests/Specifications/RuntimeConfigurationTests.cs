using System.Text.Json;
using DotNetContainerRuntime.Core.Specifications;
using Xunit;

namespace DotNetContainerRuntime.Core.Tests.Specifications;

public class RuntimeConfigurationTests
{
    [Fact]
    public void RuntimeConfiguration_MinimalConfig_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var config = new RuntimeConfiguration
        {
            OciVersion = "1.2.0",
            Root = new RootConfiguration
            {
                Path = "rootfs",
                ReadOnly = true
            },
            Process = new ProcessConfiguration
            {
                User = new UserConfiguration
                {
                    Uid = 0,
                    Gid = 0
                },
                Args = new List<string> { "/bin/sh" },
                Cwd = "/"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        var deserialized = JsonSerializer.Deserialize<RuntimeConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("1.2.0", deserialized.OciVersion);
        Assert.NotNull(deserialized.Root);
        Assert.Equal("rootfs", deserialized.Root.Path);
        Assert.True(deserialized.Root.ReadOnly);
        Assert.NotNull(deserialized.Process);
        Assert.Equal(0u, deserialized.Process.User.Uid);
    }

    [Fact]
    public void RuntimeConfiguration_WithLinuxConfig_ShouldIncludeNamespaces()
    {
        // Arrange
        var config = new RuntimeConfiguration
        {
            OciVersion = "1.2.0",
            Root = new RootConfiguration { Path = "rootfs" },
            Process = new ProcessConfiguration
            {
                User = new UserConfiguration { Uid = 0, Gid = 0 },
                Cwd = "/"
            },
            Linux = new LinuxConfiguration
            {
                Namespaces = new List<NamespaceConfiguration>
                {
                    new() { Type = "pid" },
                    new() { Type = "network" },
                    new() { Type = "mount" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        // Assert
        Assert.Contains("\"linux\"", json);
        Assert.Contains("\"namespaces\"", json);
        Assert.Contains("\"pid\"", json);
        Assert.Contains("\"network\"", json);
    }

    [Fact]
    public void RuntimeConfiguration_WithResources_ShouldSerializeCorrectly()
    {
        // Arrange
        var config = new RuntimeConfiguration
        {
            OciVersion = "1.2.0",
            Root = new RootConfiguration { Path = "rootfs" },
            Process = new ProcessConfiguration
            {
                User = new UserConfiguration { Uid = 0, Gid = 0 },
                Cwd = "/"
            },
            Linux = new LinuxConfiguration
            {
                Resources = new ResourcesConfiguration
                {
                    Memory = new MemoryConfiguration
                    {
                        Limit = 536870912 // 512MB
                    },
                    Cpu = new CpuConfiguration
                    {
                        Shares = 1024,
                        Quota = 50000,
                        Period = 100000
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        var deserialized = JsonSerializer.Deserialize<RuntimeConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Linux);
        Assert.NotNull(deserialized.Linux.Resources);
        Assert.NotNull(deserialized.Linux.Resources.Memory);
        Assert.Equal(536870912, deserialized.Linux.Resources.Memory.Limit);
        Assert.NotNull(deserialized.Linux.Resources.Cpu);
        Assert.Equal(1024ul, deserialized.Linux.Resources.Cpu.Shares);
    }

    [Fact]
    public void ProcessConfiguration_WithCapabilities_ShouldSerializeCorrectly()
    {
        // Arrange
        var process = new ProcessConfiguration
        {
            User = new UserConfiguration { Uid = 1000, Gid = 1000 },
            Args = new List<string> { "/app/server" },
            Cwd = "/app",
            Capabilities = new CapabilitiesConfiguration
            {
                Effective = new List<string> { "CAP_NET_BIND_SERVICE" },
                Permitted = new List<string> { "CAP_NET_BIND_SERVICE" }
            },
            NoNewPrivileges = true
        };

        // Act
        var json = JsonSerializer.Serialize(process, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        var deserialized = JsonSerializer.Deserialize<ProcessConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.NoNewPrivileges);
        Assert.NotNull(deserialized.Capabilities);
        Assert.NotNull(deserialized.Capabilities.Effective);
        Assert.Contains("CAP_NET_BIND_SERVICE", deserialized.Capabilities.Effective);
    }

    [Fact]
    public void MountConfiguration_ShouldSerializeCorrectly()
    {
        // Arrange
        var mount = new MountConfiguration
        {
            Destination = "/data",
            Source = "/host/data",
            Type = "bind",
            Options = new List<string> { "rbind", "ro" }
        };

        // Act
        var json = JsonSerializer.Serialize(mount);
        var deserialized = JsonSerializer.Deserialize<MountConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("/data", deserialized.Destination);
        Assert.Equal("/host/data", deserialized.Source);
        Assert.Equal("bind", deserialized.Type);
        Assert.NotNull(deserialized.Options);
        Assert.Contains("rbind", deserialized.Options);
        Assert.Contains("ro", deserialized.Options);
    }

    [Fact]
    public void WindowsConfiguration_ShouldSerializeCorrectly()
    {
        // Arrange
        var config = new RuntimeConfiguration
        {
            OciVersion = "1.2.0",
            Root = new RootConfiguration { Path = "rootfs" },
            Process = new ProcessConfiguration
            {
                User = new UserConfiguration { Uid = 0, Gid = 0 },
                Cwd = "C:\\"
            },
            Windows = new WindowsConfiguration
            {
                LayerFolders = new List<string> 
                { 
                    "C:\\layers\\base",
                    "C:\\layers\\app"
                },
                Resources = new WindowsResourcesConfiguration
                {
                    Memory = new WindowsMemoryConfiguration
                    {
                        Limit = 1073741824 // 1GB
                    },
                    Cpu = new WindowsCpuConfiguration
                    {
                        Count = 2,
                        Maximum = 5000
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        var deserialized = JsonSerializer.Deserialize<RuntimeConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Windows);
        Assert.NotNull(deserialized.Windows.LayerFolders);
        Assert.Equal(2, deserialized.Windows.LayerFolders.Count);
        Assert.NotNull(deserialized.Windows.Resources);
        Assert.Equal(1073741824ul, deserialized.Windows.Resources.Memory?.Limit);
    }
}
