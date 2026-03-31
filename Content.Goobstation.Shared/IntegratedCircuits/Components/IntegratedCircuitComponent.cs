using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.IntegratedCircuits.Components;

/// <summary>
/// Marks an entity as an integrated circuit (microchip).
/// Circuits are placed inside <see cref="ElectronicAssemblyComponent"/> entities
/// and connected to each other via pins.
/// </summary>
/// <remarks>
/// Each circuit type (logic gate, sensor, manipulator, etc.) should add this component
/// and define its pins via YAML prototypes. The actual behavior when activated is
/// implemented by subscribing to <see cref="Events.CircuitActivatedEvent"/>.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IntegratedCircuitComponent : Component
{
    /// <summary>
    /// How much "complexity budget" this circuit uses inside an assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Complexity = 1;

    /// <summary>
    /// How much physical space this circuit occupies inside an assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Size = 1;

    /// <summary>
    /// Input data pins — receive data from other circuits' outputs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<IntegratedPin> InputPins = new();

    /// <summary>
    /// Output data pins — send data to other circuits' inputs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<IntegratedPin> OutputPins = new();

    /// <summary>
    /// Activator pins — trigger circuit execution or propagate pulses.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<IntegratedPin> ActivatorPins = new();

    /// <summary>
    /// Minimum time between activations (in seconds).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CooldownPerUse = 0.1f;

    /// <summary>
    /// Power consumed (in watts) each time the circuit is activated.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PowerDrawPerUse;

    /// <summary>
    /// Power consumed (in watts) continuously while inside a powered assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PowerDrawIdle;

    /// <summary>
    /// The assembly entity this circuit is currently installed in, if any.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? AssemblyUid;

    /// <summary>
    /// Whether this circuit can be removed from its assembly by players.
    /// Built-in circuits (e.g. prefab parts) may set this to false.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Removable = true;

    /// <summary>
    /// Custom display name set by the player (via rename).
    /// If null, the entity's Name is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? DisplayName;

    /// <summary>
    /// Last time this circuit was activated (world time). Used for cooldown.
    /// </summary>
    [ViewVariables]
    public TimeSpan LastActivation = TimeSpan.Zero;
}
