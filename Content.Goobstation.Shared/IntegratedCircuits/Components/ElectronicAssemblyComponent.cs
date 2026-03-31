using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.IntegratedCircuits.Components;

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
    /// Ordered list of circuit entity UIDs installed in this assembly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> CircuitEntities = new();

    /// <summary>
    /// Color applied to the assembly's detail overlay sprite.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color DetailColor = Color.Black;
}
