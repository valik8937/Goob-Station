using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;
using Content.Pirate.Shared.IntegratedCircuits;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Marks an entity as an electronic assembly — a physical case that holds
/// <see cref="IntegratedCircuitComponent"/> entities and provides them with power.
/// </summary>
/// <remarks>
/// Assemblies come in various sizes (small, medium, large, drone, etc.)
/// and limit the total size and complexity of circuits they can contain.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ElectronicAssemblyComponent : Component
{
    /// <summary>
    /// Maximum total size of all circuits that can fit in this assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxComponents = 25;

    /// <summary>
    /// Maximum total complexity of all circuits in this assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxComplexity = 75;

    /// <summary>
    /// Whether the maintenance panel is currently open.
    /// Circuits can only be added/removed/wired when the panel is open.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Opened = true;

    /// <summary>
    /// Чи заварений корпус зваркою. Якщо так, викрутка не працюватиме.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Welded = false;

    /// <summary>
    /// Ordered list of circuit entity UIDs installed in this assembly.
    /// </summary>
    [DataField]
    public List<EntityUid> CircuitEntities = new();

    /// <summary>
    /// Color applied to the assembly's detail overlay sprite.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color DetailColor = Color.Black;

    /// <summary>
    /// Which circuit action types this assembly case supports.
    /// Circuits requiring unsupported flags cannot be inserted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public CircuitActionFlags AllowedActionFlags = CircuitActionFlags.Combat | CircuitActionFlags.LongRange;

    /// <summary>
    /// Number of circuit activations that have occurred this tick.
    /// Lazily reset when the tick changes (checked in ActivateCircuit).
    /// Used to prevent infinite loops from freezing the server.
    /// </summary>
    [ViewVariables]
    public int CurrentTickActivations;

    /// <summary>
    /// The game time of the last tick when activations were counted.
    /// When curTime differs from this, CurrentTickActivations is reset to 0.
    /// This avoids needing an Update() loop just to reset counters.
    /// </summary>
    [ViewVariables]
    public TimeSpan LastActivationTick = TimeSpan.Zero;

    /// <summary>
    /// Maximum number of circuit activations allowed per tick before the assembly "short-circuits".
    /// </summary>
    [DataField]
    public int MaxActivationsPerTick = 100;
}
