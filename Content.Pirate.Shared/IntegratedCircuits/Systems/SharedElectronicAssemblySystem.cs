using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Events;

namespace Content.Pirate.Shared.IntegratedCircuits.Systems;

/// <summary>
/// System for managing electronic assemblies — adding/removing circuits,
/// checking capacity limits, action flags, and handling lifecycle events.
/// </summary>
public abstract class SharedElectronicAssemblySystem : EntitySystem
{
    [Dependency] private readonly SharedIntegratedCircuitSystem _circuits = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElectronicAssemblyComponent, ComponentRemove>(OnAssemblyRemoved);
    }

    /// <summary>
    /// When an assembly is removed, disconnect and release all circuits.
    /// </summary>
    private void OnAssemblyRemoved(Entity<ElectronicAssemblyComponent> ent, ref ComponentRemove args)
    {
        // Remove all circuits (without requiring the panel to be open).
        var circuits = new List<EntityUid>(ent.Comp.CircuitEntities);
        foreach (var circuitUid in circuits)
        {
            RemoveCircuit(ent, circuitUid, ent.Comp);
        }
    }

    #region Capacity

    /// <summary>
    /// Calculates the total size of all installed circuits.
    /// </summary>
    public int GetTotalSize(EntityUid uid, ElectronicAssemblyComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return 0;

        var total = 0;
        foreach (var circuitUid in comp.CircuitEntities)
        {
            if (TryComp<IntegratedCircuitComponent>(circuitUid, out var circuit))
                total += circuit.Size;
        }

        return total;
    }

    /// <summary>
    /// Calculates the total complexity of all installed circuits.
    /// </summary>
    public int GetTotalComplexity(EntityUid uid, ElectronicAssemblyComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return 0;

        var total = 0;
        foreach (var circuitUid in comp.CircuitEntities)
        {
            if (TryComp<IntegratedCircuitComponent>(circuitUid, out var circuit))
                total += circuit.Complexity;
        }

        return total;
    }

    #endregion

    #region Add/Remove Circuits

    /// <summary>
    /// Attempts to add a circuit entity to an assembly.
    /// Checks that the panel is open, capacity limits are not exceeded,
    /// and that the assembly supports the circuit's action flags.
    /// Returns true on success.
    /// </summary>
    public bool TryAddCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return false;

        if (!assemblyComp.Opened)
            return false;

        // Already in an assembly?
        if (circuitComp.AssemblyUid != null)
            return false;

        // Check action flag compatibility — assembly must support all flags the circuit requires.
        if ((circuitComp.ActionFlags & ~assemblyComp.AllowedActionFlags) != CircuitActionFlags.None)
            return false;

        // Check size limits.
        var totalSize = GetTotalSize(assemblyUid, assemblyComp);
        if (totalSize + circuitComp.Size > assemblyComp.MaxComponents)
            return false;

        // Check complexity limits.
        var totalComplexity = GetTotalComplexity(assemblyUid, assemblyComp);
        if (totalComplexity + circuitComp.Complexity > assemblyComp.MaxComplexity)
            return false;

        // Add to assembly.
        AddCircuit(assemblyUid, circuitUid, assemblyComp, circuitComp);
        return true;
    }

    /// <summary>
    /// Adds a circuit to an assembly without performing any checks.
    /// </summary>
    public void AddCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return;

        assemblyComp.CircuitEntities.Add(circuitUid);
        circuitComp.AssemblyUid = assemblyUid;

        Dirty(assemblyUid, assemblyComp);
        Dirty(circuitUid, circuitComp);

        var ev = new CircuitAddedToAssemblyEvent(assemblyUid);
        RaiseLocalEvent(circuitUid, ev);
    }

    /// <summary>
    /// Attempts to remove a circuit entity from an assembly.
    /// Checks that the panel is open and the circuit is removable.
    /// Disconnects all wires from the circuit.
    /// Returns true on success.
    /// </summary>
    public bool TryRemoveCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return false;

        if (!assemblyComp.Opened)
            return false;

        if (!circuitComp.Removable)
            return false;

        if (circuitComp.AssemblyUid != assemblyUid)
            return false;

        RemoveCircuit(assemblyUid, circuitUid, assemblyComp, circuitComp);
        return true;
    }

    /// <summary>
    /// Removes a circuit from an assembly without performing any checks.
    /// Disconnects all wires.
    /// </summary>
    public void RemoveCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return;

        _circuits.DisconnectAll(circuitUid, circuitComp);

        assemblyComp.CircuitEntities.Remove(circuitUid);
        circuitComp.AssemblyUid = null;

        Dirty(assemblyUid, assemblyComp);
        Dirty(circuitUid, circuitComp);

        var ev = new CircuitRemovedFromAssemblyEvent(assemblyUid);
        RaiseLocalEvent(circuitUid, ev);
    }

    #endregion
}
