using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.IntegratedCircuits;

/// <summary>
/// Represents a single I/O pin on an integrated circuit.
/// Pins hold data and can be connected to pins on other circuits via wires.
/// Data pins transfer values; activator pins send execution pulses.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public sealed partial class IntegratedPin
{
    /// <summary>
    /// Display name of the pin (e.g. "A", "result", "on pulse").
    /// </summary>
    [DataField]
    public string Name { get; set; } = "pin";

    /// <summary>
    /// Role of this pin — input, output, or activator.
    /// </summary>
    [DataField]
    public PinType PinType { get; set; } = PinType.Input;

    /// <summary>
    /// Constrains the type of data this pin accepts.
    /// Only relevant for data pins (Input/Output). Activators ignore this.
    /// </summary>
    [DataField]
    public PinDataType DataType { get; set; } = PinDataType.Any;

    /// <summary>
    /// The current value stored in this pin.
    /// Can be null, float, string, bool, EntityUid, or List&lt;object?&gt;.
    /// Activator pins don't store meaningful data.
    /// </summary>
    [DataField]
    public object? Data { get; set; }

    /// <summary>
    /// Addresses of all pins this pin is wired to.
    /// Wires are bidirectional — both ends store the link.
    /// </summary>
    [DataField]
    public List<PinAddress> LinkedPins { get; set; } = new();
}
