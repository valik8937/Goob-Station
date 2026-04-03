using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits;

/// <summary>
/// Flags describing what kinds of actions a circuit can perform,
/// or what kinds of actions an assembly case supports.
/// Used to prevent players from inserting e.g. combat circuits into a desktop calculator assembly.
/// </summary>
[Flags, Serializable, NetSerializable]
public enum CircuitActionFlags : byte
{
    None = 0,

    /// <summary>
    /// Circuit can perform combat actions (fire weapons, etc.).
    /// </summary>
    Combat = 1 << 0,

    /// <summary>
    /// Circuit can interact with entities at long range.
    /// </summary>
    LongRange = 1 << 1,

    /// <summary>
    /// Circuit can cause the assembly to move (drone locomotion).
    /// </summary>
    Movement = 1 << 2,
}
