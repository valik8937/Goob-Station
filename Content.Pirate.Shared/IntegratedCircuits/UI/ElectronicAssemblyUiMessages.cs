using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits.UI;

/// <summary>
/// Information about a single circuit installed inside an assembly, sent to the client for display.
/// </summary>
[Serializable, NetSerializable]
public sealed class AssemblyCircuitEntry
{
    /// <summary>
    /// Network entity reference to the circuit.
    /// </summary>
    public NetEntity NetEntity { get; set; }

    /// <summary>
    /// Display name of the circuit (custom name or prototype name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this circuit can be removed by the player.
    /// </summary>
    public bool Removable { get; set; } = true;
}

/// <summary>
/// Full UI state sent from the server to the client for the assembly window.
/// Mirrors the original open_interact() HTML layout.
/// </summary>
[Serializable, NetSerializable]
public sealed class ElectronicAssemblyBoundUIState : BoundUserInterfaceState
{
    /// <summary>
    /// Whether the assembly's maintenance panel is open.
    /// </summary>
    public bool IsOpened { get; set; }

    /// <summary>
    /// Total size of all installed circuits.
    /// </summary>
    public int TotalSize { get; set; }

    /// <summary>
    /// Maximum size capacity of the assembly.
    /// </summary>
    public int MaxSize { get; set; }

    /// <summary>
    /// Total complexity of all installed circuits.
    /// </summary>
    public int TotalComplexity { get; set; }

    /// <summary>
    /// Maximum complexity capacity of the assembly.
    /// </summary>
    public int MaxComplexity { get; set; }

    /// <summary>
    /// Whether the assembly has a battery installed.
    /// </summary>
    public bool HasBattery { get; set; }

    /// <summary>
    /// Current battery charge in joules. Only relevant if HasBattery is true.
    /// </summary>
    public float BatteryCharge { get; set; }

    /// <summary>
    /// Maximum battery capacity in joules. Only relevant if HasBattery is true.
    /// </summary>
    public float BatteryMaxCharge { get; set; }

    /// <summary>
    /// List of circuits currently installed in the assembly.
    /// </summary>
    public List<AssemblyCircuitEntry> Circuits { get; set; } = new();

    /// <summary>
    /// Display name of the assembly itself.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;
}

// ── Messages from client to server ──

/// <summary>
/// Request to remove a specific circuit from the assembly.
/// </summary>
[Serializable, NetSerializable]
public sealed class AssemblyRemoveCircuitMessage : BoundUserInterfaceMessage
{
    public NetEntity CircuitEntity { get; }

    public AssemblyRemoveCircuitMessage(NetEntity circuitEntity)
    {
        CircuitEntity = circuitEntity;
    }
}

/// <summary>
/// Request to rename the assembly.
/// </summary>
[Serializable, NetSerializable]
public sealed class AssemblyRenameMessage : BoundUserInterfaceMessage
{
    public string NewName { get; }

    public AssemblyRenameMessage(string newName)
    {
        NewName = newName;
    }
}

/// <summary>
/// Request to remove the battery from the assembly.
/// </summary>
[Serializable, NetSerializable]
public sealed class AssemblyRemoveBatteryMessage : BoundUserInterfaceMessage
{
}
