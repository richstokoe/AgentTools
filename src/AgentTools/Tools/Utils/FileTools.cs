using System.ComponentModel;

namespace RichStokoe.AgentTools.Utils;

public static class FileTools
{
    [Description("Read the full contents of a file. Returns the text content, or an error message if the file does not exist or cannot be read.")]
    public static async Task<string> ReadFile(
        [Description("The absolute or relative path to the file to read.")] string path)
    {
        try
        {
            if (!File.Exists(path))
                return $"Error: file not found: {path}";

            var content = await File.ReadAllTextAsync(path);
            return string.IsNullOrEmpty(content) ? "[empty file]" : content;
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    [Description("Write text content to a file, creating it if it does not exist or overwriting it if it does.")]
    public static async Task<string> WriteFile(
        [Description("The absolute or relative path to the file to write.")] string path,
        [Description("The text content to write to the file.")] string content)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(path, content);
            return $"Written {content.Length} characters to {path}";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }

    [Description("Append text to the end of a file. Creates the file if it does not exist.")]
    public static async Task<string> AppendToFile(
        [Description("The absolute or relative path to the file to append to.")] string path,
        [Description("The text content to append.")] string content)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.AppendAllTextAsync(path, content);
            return $"Appended {content.Length} characters to {path}";
        }
        catch (Exception ex)
        {
            return $"Error appending to file: {ex.Message}";
        }
    }
}
