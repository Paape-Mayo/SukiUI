namespace SukiUI.Enums;

/// <summary>
/// Describes where a SukiGroupSideMenuItem sits within its visual group,
/// controlling which corners are rounded to form a connected rectangle.
/// </summary>
public enum SukiGroupSegmentPosition
{
    /// <summary>The item is the only member of the group.</summary>
    Sole,

    /// <summary>The item is the topmost member of a multi-item group.</summary>
    Top,

    /// <summary>The item is between the top and bottom members.</summary>
    Middle,

    /// <summary>The item is the bottommost member of a multi-item group.</summary>
    Bottom,
}
