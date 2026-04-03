using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits;

/// <summary>
/// Constrains what kind of data a pin can hold.
/// Pins with <see cref="Any"/> accept numbers, strings, refs, and lists.
/// Typed pins reject data that doesn't match.
/// </summary>
[Serializable, NetSerializable]
public enum PinDataType : byte
{
    /// <summary>
    /// Accepts any serializable value (number, string, ref, list, null).
    /// </summary>
    Any,

    /// <summary>
    /// Accepts only numbers (float) or null.
    /// </summary>
    Number,

    /// <summary>
    /// Accepts only text strings or null.
    /// </summary>
    String,

    /// <summary>
    /// Accepts only boolean (true/false). Null is not allowed.
    /// </summary>
    Boolean,

    /// <summary>
    /// Accepts only entity references (EntityUid) or null.
    /// </summary>
    Ref,

    /// <summary>
    /// Accepts only lists or null (clears list).
    /// </summary>
    List,

    /// <summary>
    /// Accepts only hex color strings (#RRGGBB) or null.
    /// </summary>
    Color,

    /// <summary>
    /// Accepts only direction values (1,2,4,8 etc.) or null.
    /// </summary>
    Dir,

    /// <summary>
    /// Accepts only non-negative integers within bounds, or null.
    /// </summary>
    Index,

    /// <summary>
    /// Accepts only single-character strings or null.
    /// </summary>
    Char,
}
