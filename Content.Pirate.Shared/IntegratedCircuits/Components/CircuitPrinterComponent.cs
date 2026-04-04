using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Marks an entity as an integrated circuit printer.
/// The printer fabricates circuit components and assembly cases from raw materials.
/// Can be upgraded to produce advanced circuits and to clone assemblies.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CircuitPrinterComponent : Component
{
    /// <summary>
    /// Whether the printer has the advanced designs upgrade installed.
    /// When true, research-tier circuits become available.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Upgraded;

    /// <summary>
    /// Whether the printer can clone (duplicate) entire assemblies from save code.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanClone = true;

    /// <summary>
    /// If true, cloning is instant. If false, cloning takes time proportional to cost.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FastClone;

    /// <summary>
    /// Current material stocks. Key = material name, Value = amount in units.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, int> Materials = new()
    {
        { "Steel", 0 },
        { "Glass", 0 },
        { "Plastic", 0 }
    };

    /// <summary>
    /// Maximum material capacity for each material type, in units.
    /// Default: 25 sheets * 100 units = 2500.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaterialMax = 2500;

    /// <summary>
    /// Whether the printer is currently cloning an assembly.
    /// </summary>
    [ViewVariables]
    public bool Cloning;

    /// <summary>
    /// Currently selected category in the UI.
    /// </summary>
    [ViewVariables]
    public string? CurrentCategory;
}
