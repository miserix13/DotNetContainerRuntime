using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetContainerRuntime.Core.Abstractions;
using DotNetContainerRuntime.Core.Specifications;
using DotNetContainerRuntime.Linux.Interop;

namespace DotNetContainerRuntime.Linux;

/// <summary>
/// Linux process manager using fork() and execve() for container process creation.
/// </summary>
public class LinuxProcessManager : IProcessManager
{
    public Task<ProcessContext> ExecuteAsync(
        string containerId,
        ProcessConfiguration processConfig,
        NamespaceContext? namespaceContext,
        ResourceControlContext resourceContext,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux process operations are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int pid = LinuxProcess.fork();

        if (pid < 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"Failed to fork process. fork() returned {pid}, errno: {errno}");
        }

        if (pid == 0)
        {
            // Child process
            try
            {
                ConfigureChildProcess(processConfig);
                ExecuteProcess(processConfig);
            }
            catch
            {
                Environment.Exit(1);
            }
        }

        // Parent process - return context
        return Task.FromResult(new ProcessContext
        {
            ContainerId = containerId,
            Pid = pid,
            StartTime = DateTimeOffset.UtcNow
        });
    }

    public Task SendSignalAsync(
        int pid,
        int signal,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux process operations are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        int result = LinuxProcess.kill(pid, signal);

        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            if (errno != 3) // Ignore ESRCH (no such process)
            {
                throw new InvalidOperationException(
                    $"Failed to kill process {pid}. kill() returned {result}, errno: {errno}");
            }
        }

        return Task.CompletedTask;
    }

    public Task<int> WaitForExitAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux process operations are only supported on Linux.");
        }

        return Task.Run(() =>
        {
            int status = 0;
            int result = LinuxProcess.waitpid(context.Pid, out status, 0);

            if (result < 0)
            {
                int errno = Marshal.GetLastPInvokeError();
                throw new InvalidOperationException(
                    $"Failed to wait for process {context.Pid}. waitpid() returned {result}, errno: {errno}");
            }

            return GetExitCode(status);
        }, cancellationToken);
    }
    
    public bool IsProcessRunning(int pid)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux process operations are only supported on Linux.");
        }

        // Send signal 0 to check if process exists
        int result = LinuxProcess.kill(pid, 0);
        return result == 0;
    }

    public Task SetUserAsync(
        int uid,
        int gid,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux process operations are only supported on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Set group ID first (must be done before dropping privileges)
        int result = LinuxProcess.setgid((uint)gid);
        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"Failed to set GID to {gid}. setgid() returned {result}, errno: {errno}");
        }

        // Set user ID
        result = LinuxProcess.setuid((uint)uid);
        if (result != 0)
        {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"Failed to set UID to {uid}. setuid() returned {result}, errno: {errno}");
        }

        return Task.CompletedTask;
    }

    private void ConfigureChildProcess(ProcessConfiguration processConfig)
    {
        // Change working directory
        if (!string.IsNullOrEmpty(processConfig.Cwd))
        {
            Directory.SetCurrentDirectory(processConfig.Cwd);
        }

        // Set user and group
        if (processConfig.User?.Uid != null && processConfig.User?.Gid != null)
        {
            SetUserAsync((int)processConfig.User.Uid, (int)processConfig.User.Gid, CancellationToken.None)
                .GetAwaiter().GetResult();
        }

        // Set environment variables
        if (processConfig.Env != null)
        {
            foreach (var env in processConfig.Env)
            {
                var parts = env.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0], parts[1]);
                }
            }
        }

        // Set capabilities (if specified)
        // TODO: Implement when Capability type is defined
        // if (processConfig.Capabilities != null)
        // {
        //     ConfigureCapabilities(processConfig.Capabilities);
        // }

        // Set rlimits (if specified)
        // TODO: Implement when POSIXRlimit type is defined
        // if (processConfig.Rlimits != null)
        // {
        //     ConfigureRlimits(processConfig.Rlimits);
        // }
    }

    private void ExecuteProcess(ProcessConfiguration processConfig)
    {
        if (processConfig.Args == null || processConfig.Args.Count == 0)
        {
            throw new InvalidOperationException("Process args cannot be empty");
        }

        string executable = processConfig.Args[0];

        // execve requires IntPtr for argv and envp - simplified call
        int result = LinuxProcess.execve(executable, nint.Zero, nint.Zero);

        // If execve returns, it failed
        int errno = Marshal.GetLastPInvokeError();
        throw new InvalidOperationException(
            $"Failed to execute {executable}. execve() returned {result}, errno: {errno}");
    }

    // TODO: Re-enable when Capability and POSIXRlimit types are defined
    /*
    private void ConfigureCapabilities(Capability capabilities)
    {
        // This is a simplified implementation
        // Full implementation would use cap_set_proc() from LinuxCapabilities
        
        // For now, we'll just drop all capabilities if NoNewPrivileges is set
        if (capabilities.Ambient?.Length == 0 && 
            capabilities.Effective?.Length == 0 && 
            capabilities.Permitted?.Length == 0)
        {
            // Drop all capabilities
            LinuxCapabilities.prctl(LinuxCapabilities.PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0);
        }
    }

    private void ConfigureRlimits(POSIXRlimit[] rlimits)
    {
        // This would use setrlimit() syscall
        // Not implemented in our P/Invoke declarations yet
        // TODO: Add setrlimit() support
    }
    */

    private static int GetExitCode(int status)
    {
        // WEXITSTATUS macro: (status >> 8) & 0xFF
        return (status >> 8) & 0xFF;
    }

    private static bool WasKilledBySignal(int status)
    {
        // WIFSIGNALED macro: ((status & 0x7F) + 1) >> 1 > 0
        return ((status & 0x7F) + 1) >> 1 > 0;
    }

    private static int GetTermSignal(int status)
    {
        // WTERMSIG macro: status & 0x7F
        return status & 0x7F;
    }

    public void Dispose()
    {
        // No resources to cleanup
    }
}
