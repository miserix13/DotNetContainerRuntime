using System.Text.Json;
using DotNetContainerRuntime.Core.Specifications;
using Xunit;

namespace DotNetContainerRuntime.Core.Tests.Specifications;

public class ContainerStateTests
{
    [Fact]
    public void ContainerState_ShouldSerializeToJson()
    {
        // Arrange
        var state = new ContainerState
        {
            OciVersion = "1.2.0",
            Id = "test-container",
            Status = ContainerStatus.Running,
            Pid = 12345,
            Bundle = "/var/lib/containers/test-container",
            Annotations = new Dictionary<string, string>
            {
                ["com.example.key"] = "value"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        // Assert
        Assert.Contains("\"ociVersion\": \"1.2.0\"", json);
        Assert.Contains("\"id\": \"test-container\"", json);
        Assert.Contains("\"status\": \"Running\"", json);
        Assert.Contains("\"pid\": 12345", json);
        Assert.Contains("\"bundle\": \"/var/lib/containers/test-container\"", json);
    }

    [Fact]
    public void ContainerState_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """
        {
            "ociVersion": "1.2.0",
            "id": "test-container",
            "status": "Running",
            "pid": 12345,
            "bundle": "/var/lib/containers/test-container"
        }
        """;

        // Act
        var state = JsonSerializer.Deserialize<ContainerState>(json);

        // Assert
        Assert.NotNull(state);
        Assert.Equal("1.2.0", state.OciVersion);
        Assert.Equal("test-container", state.Id);
        Assert.Equal(ContainerStatus.Running, state.Status);
        Assert.Equal(12345, state.Pid);
        Assert.Equal("/var/lib/containers/test-container", state.Bundle);
    }

    [Fact]
    public void ContainerState_WithoutPid_ShouldOmitInJson()
    {
        // Arrange
        var state = new ContainerState
        {
            OciVersion = "1.2.0",
            Id = "test-container",
            Status = ContainerStatus.Created,
            Pid = null,
            Bundle = "/var/lib/containers/test-container"
        };

        // Act
        var json = JsonSerializer.Serialize(state);

        // Assert
        Assert.DoesNotContain("\"pid\"", json);
    }

    [Theory]
    [InlineData(ContainerStatus.Creating, "Creating")]
    [InlineData(ContainerStatus.Created, "Created")]
    [InlineData(ContainerStatus.Running, "Running")]
    [InlineData(ContainerStatus.Stopped, "Stopped")]
    public void ContainerStatus_ShouldSerializeCorrectly(ContainerStatus status, string expected)
    {
        // Arrange
        var state = new ContainerState
        {
            OciVersion = "1.2.0",
            Id = "test",
            Status = status,
            Bundle = "/test"
        };

        // Act
        var json = JsonSerializer.Serialize(state);

        // Assert
        Assert.Contains($"\"status\":\"{expected}\"", json);
    }
}
