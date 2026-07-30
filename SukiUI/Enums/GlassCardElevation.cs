namespace SukiUI.Enums;

/// <summary>
/// Resting drop shadow for a <see cref="Controls.GlassCard"/>. Each level maps onto a
/// BoxShadows token that already exists per theme variant, so light and dark both work
/// with no new colours.
///
/// <para><see cref="None"/> is the default and emits no style block at all, which is what
/// keeps the several hundred existing cards byte-for-byte unchanged. The absence of a
/// Setter is the guarantee, not a Setter restoring the old value.</para>
/// </summary>
public enum GlassCardElevation
{
    /// <summary>No shadow. Today's appearance, and the default.</summary>
    None,

    /// <summary>A hairline lift. Suitable for cards in a dense grid.</summary>
    Low,

    /// <summary>A clear lift. Suitable for a card that is the focus of its region.</summary>
    Medium,

    /// <summary>A pronounced lift. Reserved for cards that float over other content.</summary>
    High,
}
