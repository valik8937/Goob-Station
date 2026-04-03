using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits;

/// <summary>
/// The role of a pin on an integrated circuit.
/// </summary>
[Serializable, NetSerializable]
public enum PinType : byte
{
    /// <summary>
    /// Data input pin — receives data from other circuits.
    /// </summary>
    Input,

    /// <summary>
    /// Data output pin — sends data to other circuits.
    /// </summary>
    Output,

    /// <summary>
    /// Activation pin — triggers circuit execution via pulses.
    /// </summary>
    Activator,
}
