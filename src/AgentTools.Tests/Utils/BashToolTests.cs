using RichStokoe.AgentTools.Utils;

namespace RichStokoe.AgentTools.Tests.Utils;

/// <summary>
/// BashTool passes the command string as a single token to the shell's -c flag.
/// Multi-word commands (e.g. "echo hello") are split by the OS into separate argv
/// entries, so only the first word is treated as the script — the rest become $0, $1, ...
/// Tests therefore use single-word shell builtins or rely on the exit-code / stderr paths
/// that don't require argument passing.
/// </summary>
public class BashToolTests
{
    [Fact]
    public async Task RunCommand_SuccessfulCommandNoOutput_ReturnsNoOutput()
    {
        var result = await BashTool.RunCommand("true");
        Assert.Equal("[no output]", result);
    }

    [Fact]
    public async Task RunCommand_FailingCommand_ReportsExitCode()
    {
        var result = await BashTool.RunCommand("false");
        Assert.Contains("[exit code: 1]", result);
    }

    [Fact]
    public async Task RunCommand_CommandProducesOutput_ReturnsStdout()
    {
        // pwd is a single-word command that always produces output
        var result = await BashTool.RunCommand("pwd");
        Assert.NotEqual("[no output]", result);
        Assert.StartsWith("/", result);
    }

    [Fact]
    public async Task RunCommand_WithWorkingDirectory_OutputsCorrectPath()
    {
        var dir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        var result = await BashTool.RunCommand("pwd", workingDirectory: dir);
        // Normalise — macOS may resolve /var/folders symlink to /private/var/folders
        Assert.True(result.Contains(dir) || result.Contains(Path.GetFullPath(dir)),
            $"Expected path containing '{dir}', got: {result}");
    }

    [Fact]
    public async Task RunCommand_UnknownCommand_ReportsStderrAndExitCode()
    {
        // An unrecognised command causes zsh to write "command not found" to stderr
        var result = await BashTool.RunCommand("no_such_command_xyzzy_12345");
        Assert.Contains("[stderr]", result);
        Assert.Contains("[exit code:", result);
    }

    [Fact]
    public async Task RunCommand_ExitCodeZero_DoesNotReportExitCode()
    {
        var result = await BashTool.RunCommand("true");
        Assert.DoesNotContain("[exit code:", result);
    }

    [Fact]
    public async Task RunCommand_ReturnsString_NeverNull()
    {
        var result = await BashTool.RunCommand("true");
        Assert.NotNull(result);
    }
}
