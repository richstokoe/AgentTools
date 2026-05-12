namespace RichStokoe.AgentTools;

/// <summary>
/// Classifies the kind of operations an agent tool performs.
/// Used as a flags mask when filtering tools via <see cref="ToolManager.GetTools"/>.
/// </summary>
/// <remarks>
/// <see cref="GetTools"/> requires an explicit type filter — passing <see cref="None"/>
/// (the default) returns an empty list. Use the convenience composites <see cref="ReadWrite"/>
/// or <see cref="All"/> to opt into multiple categories at once.
/// </remarks>
[Flags]
public enum AgentToolTypes
{
    /// <summary>
    /// No type assigned. Tools with this value are never returned by <see cref="ToolManager.GetTools"/>.
    /// </summary>
    None      = 0,
    /// <summary>
    /// Tool performs read-only operations and has no persistent side-effects.
    /// </summary>
    Read      = 1,
    /// <summary>
    /// Tool writes or modifies state (e.g. files, external services).
    /// </summary>
    Write     = 2,
    /// <summary>
    /// Tool can cause irreversible side-effects (e.g. shell execution).
    /// Callers must opt in explicitly to receive tools of this type.
    /// </summary>
    Dangerous = 4,
    /// <summary>
    /// Convenience composite for <see cref="Read"/> | <see cref="Write"/>.
    /// Excludes <see cref="Dangerous"/> tools.
    /// </summary>
    ReadWrite = Read | Write,
    /// <summary>
    /// All tool types combined, including <see cref="Dangerous"/>.
    /// Update this when adding new members to the enum.
    /// </summary>
    All       = Read | Write | Dangerous
}

/// <summary>
/// Marks a static method as an AI agent tool, making it discoverable by <see cref="ToolManager"/>.
/// Methods must also have a <see cref="System.ComponentModel.DescriptionAttribute"/> to provide
/// the tool description exposed to the AI model.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class AgentToolAttribute : Attribute
{
    /// <summary>
    /// Optional display name for the tool. When not set, the method name is used.
    /// This name is also used when filtering with <see cref="ToolManager.GetTools"/>.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Classifies the tool as performing read, write, or both kinds of operations.
    /// Defaults to <see cref="AgentToolTypes.None"/> (unclassified).
    /// </summary>
    public AgentToolTypes Type { get; init; } = AgentToolTypes.None;
}
