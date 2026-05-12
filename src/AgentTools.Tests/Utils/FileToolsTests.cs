using RichStokoe.AgentTools.Utils;

namespace RichStokoe.AgentTools.Tests.Utils;

public class FileToolsTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"AgentToolsTests_{Guid.NewGuid():N}");

    public FileToolsTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string TempPath(string name) => Path.Combine(_tempDir, name);

    // --- ReadFile ---

    [Fact]
    public async Task ReadFile_FileDoesNotExist_ReturnsNotFoundError()
    {
        var result = await FileTools.ReadFile(TempPath("missing.txt"));
        Assert.StartsWith("Error: file not found:", result);
        Assert.Contains("missing.txt", result);
    }

    [Fact]
    public async Task ReadFile_ExistingFile_ReturnsContents()
    {
        var path = TempPath("hello.txt");
        await File.WriteAllTextAsync(path, "Hello, Agent!");

        var result = await FileTools.ReadFile(path);

        Assert.Equal("Hello, Agent!", result);
    }

    [Fact]
    public async Task ReadFile_EmptyFile_ReturnsEmptyFileMarker()
    {
        var path = TempPath("empty.txt");
        await File.WriteAllTextAsync(path, "");

        var result = await FileTools.ReadFile(path);

        Assert.Equal("[empty file]", result);
    }

    [Fact]
    public async Task ReadFile_MultilineFile_ReturnsFullContents()
    {
        var path = TempPath("multi.txt");
        var content = "line 1\nline 2\nline 3";
        await File.WriteAllTextAsync(path, content);

        var result = await FileTools.ReadFile(path);

        Assert.Equal(content, result);
    }

    // --- WriteFile ---

    [Fact]
    public async Task WriteFile_NewFile_CreatesFileWithContent()
    {
        var path = TempPath("new.txt");

        var result = await FileTools.WriteFile(path, "written content");

        Assert.Contains("written content".Length.ToString(), result);
        Assert.Contains(path, result);
        Assert.Equal("written content", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task WriteFile_ExistingFile_OverwritesContent()
    {
        var path = TempPath("overwrite.txt");
        await File.WriteAllTextAsync(path, "original");

        await FileTools.WriteFile(path, "replaced");

        Assert.Equal("replaced", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task WriteFile_NonExistentDirectory_CreatesDirectoryAndFile()
    {
        var path = TempPath(Path.Combine("nested", "deep", "file.txt"));

        var result = await FileTools.WriteFile(path, "deep content");

        Assert.True(File.Exists(path));
        Assert.Equal("deep content", await File.ReadAllTextAsync(path));
        Assert.Contains("deep content".Length.ToString(), result);
    }

    [Fact]
    public async Task WriteFile_ReportsCharacterCount()
    {
        var path = TempPath("counted.txt");
        var content = "12345";

        var result = await FileTools.WriteFile(path, content);

        Assert.Contains("5", result);
    }

    // --- AppendToFile ---

    [Fact]
    public async Task AppendToFile_NewFile_CreatesFileWithContent()
    {
        var path = TempPath("append_new.txt");

        var result = await FileTools.AppendToFile(path, "first line");

        Assert.True(File.Exists(path));
        Assert.Equal("first line", await File.ReadAllTextAsync(path));
        Assert.Contains("first line".Length.ToString(), result);
    }

    [Fact]
    public async Task AppendToFile_ExistingFile_AppendsWithoutOverwriting()
    {
        var path = TempPath("append_existing.txt");
        await File.WriteAllTextAsync(path, "original");

        await FileTools.AppendToFile(path, " appended");

        Assert.Equal("original appended", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task AppendToFile_NonExistentDirectory_CreatesDirectoryAndFile()
    {
        var path = TempPath(Path.Combine("append_dir", "file.txt"));

        await FileTools.AppendToFile(path, "content");

        Assert.True(File.Exists(path));
        Assert.Equal("content", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task AppendToFile_CalledMultipleTimes_AccumulatesContent()
    {
        var path = TempPath("accumulate.txt");

        await FileTools.AppendToFile(path, "a");
        await FileTools.AppendToFile(path, "b");
        await FileTools.AppendToFile(path, "c");

        Assert.Equal("abc", await File.ReadAllTextAsync(path));
    }
}
