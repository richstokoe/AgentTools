using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RichStokoe.AgentTools.Utils;

public static class BashTool
{
    [AgentTool(Type = AgentToolTypes.Dangerous)]
    [Description("Run a shell command and return its output. Use this to execute any system operation: file manipulation, running programs, git commands, searching files with grep/find, checking system state, etc. Returns stdout, stderr, and the exit code. REFUSE any seriously destructive operations, such as wiping a hard drive. Make sure you confirm with the user first before carrying out ANY destructive operations.")]
    public static async Task<string> RunCommand(
        [Description("The shell command to run. On macOS/Linux this runs in zsh; on Windows in cmd.exe.")] string command,
        [Description("Working directory for the command. Defaults to the current directory if not specified.")] string? workingDirectory = null)
    {
        var (shell, shellArg) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ("cmd.exe", "/c")
            : ("/bin/zsh", "-c");

        var psi = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = $"{shellArg} {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var process = Process.Start(psi)!;

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cts.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(cts.Token);

        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            return "Error: command timed out after 60 seconds";
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        var result = "";
        if (!string.IsNullOrEmpty(stdout)) result += stdout;
        if (!string.IsNullOrEmpty(stderr)) result += (result.Length > 0 ? "\n[stderr]\n" : "[stderr]\n") + stderr;
        if (process.ExitCode != 0) result += $"\n[exit code: {process.ExitCode}]";

        return string.IsNullOrEmpty(result) ? "[no output]" : result.TrimEnd();
    }
}
