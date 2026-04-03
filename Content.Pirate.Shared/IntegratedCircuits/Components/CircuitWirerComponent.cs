using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Modes the circuit wirer can be in.
/// </summary>
[Serializable, NetSerializable]
public enum WirerMode : byte
{
    /// <summary>
    /// Ready to select the first pin for wiring.
    /// </summary>
    Wire,

    /// <summary>
    /// Ready to select the second pin to complete the wire.
    /// </summary>
    Wiring,

    /// <summary>
    /// Ready to select the first pin for unwiring.
    /// </summary>
    Unwire,

    /// <summary>
    /// Ready to select the second pin to complete the unwire.
    /// </summary>
    Unwiring,
}

/// <summary>
/// Marks an item as a circuit wirer tool.
/// The wirer allows players to connect and disconnect pins between integrated circuits
/// inside an electronic assembly.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CircuitWirerComponent : Component
{
    /// <summary>
    /// Current operating mode of the wirer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public WirerMode Mode = WirerMode.Wire;

    /// <summary>
    /// The first pin selected during a wire/unwire operation.
    /// Null when no pin is currently selected.
    /// </summary>
    [DataField]
    public PinAddress? SelectedPin;
}
