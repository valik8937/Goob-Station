using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.IntegratedCircuits;

/// <summary>
/// Uniquely identifies a pin within an electronic assembly.
/// Used for serializing wire connections between circuits.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public sealed partial class PinAddress
{
    /// <summary>
    /// The entity UID of the integrated circuit component that owns the pin.
    /// </summary>
    [DataField]
    public EntityUid ComponentUid { get; set; }

    /// <summary>
    /// Whether this addresses an input, output, or activator pin.
    /// </summary>
    [DataField]
    public PinType PinType { get; set; }

    /// <summary>
    /// Zero-based index of the pin within the pin list of the given type.
    /// </summary>
    [DataField]
    public int PinIndex { get; set; }

    public PinAddress()
    {
    }

    public PinAddress(EntityUid componentUid, PinType pinType, int pinIndex)
    {
        ComponentUid = componentUid;
        PinType = pinType;
        PinIndex = pinIndex;
    }
}
