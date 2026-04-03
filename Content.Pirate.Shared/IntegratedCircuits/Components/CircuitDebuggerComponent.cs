using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Modes the circuit debugger can operate in.
/// </summary>
[Serializable, NetSerializable]
public enum DebuggerMode : byte
{
    /// <summary>
    /// Write a string value to a pin.
    /// </summary>
    String,

    /// <summary>
    /// Write a numeric value to a pin.
    /// </summary>
    Number,

    /// <summary>
    /// Scan an entity to store its reference, then write that ref to a pin.
    /// </summary>
    Ref,

    /// <summary>
    /// Clear a pin's data (write null).
    /// </summary>
    Null,
}

/// <summary>
/// Marks an item as a circuit debugger tool.
/// The debugger allows players to directly write data values to circuit pins
/// and pulse activator pins. In Ref mode, clicking on an entity stores its
/// EntityUid for later writing to a pin.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CircuitDebuggerComponent : Component
{
    /// <summary>
    /// Current operating mode of the debugger.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DebuggerMode Mode = DebuggerMode.Null;

    /// <summary>
    /// The data value stored in the debugger's memory.
    /// Can be null (Null mode), a string, a float/int, or an EntityUid (Ref mode).
    /// This is written to a pin when the debugger is used on a circuit.
    /// </summary>
    [ViewVariables]
    public object? StoredData;

    /// <summary>
    /// Whether the debugger is currently waiting for the player to click on an entity
    /// to capture its EntityUid as a reference. Set to true when entering Ref mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AcceptingRefs;
}
