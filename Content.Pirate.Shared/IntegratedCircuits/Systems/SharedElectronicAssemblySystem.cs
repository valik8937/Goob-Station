using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Events;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using System.Collections.Generic;

namespace Content.Pirate.Shared.IntegratedCircuits.Systems;

/// <summary>
/// System for managing electronic assemblies — adding/removing circuits,
/// checking capacity limits, action flags, and handling lifecycle events.
/// </summary>
public abstract class SharedElectronicAssemblySystem : EntitySystem
{
    [Dependency] private readonly SharedIntegratedCircuitSystem _circuits = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

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
        var circuits = new List<EntityUid>(ent.Comp.CircuitEntities);
        foreach (var circuitUid in circuits)
        {
            RemoveCircuit(ent, circuitUid, ent.Comp);
        }
    }

    #region Capacity

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

    public bool TryAddCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return false;

        if (!assemblyComp.Opened)
            return false;

        if (circuitComp.AssemblyUid != null)
            return false;

        if ((circuitComp.ActionFlags & ~assemblyComp.AllowedActionFlags) != CircuitActionFlags.None)
            return false;

        var totalSize = GetTotalSize(assemblyUid, assemblyComp);
        if (totalSize + circuitComp.Size > assemblyComp.MaxComponents)
            return false;

        var totalComplexity = GetTotalComplexity(assemblyUid, assemblyComp);
        if (totalComplexity + circuitComp.Complexity > assemblyComp.MaxComplexity)
            return false;

        AddCircuit(assemblyUid, circuitUid, assemblyComp, circuitComp);
        return true;
    }

    public void AddCircuit(EntityUid assemblyUid, EntityUid circuitUid,
        ElectronicAssemblyComponent? assemblyComp = null,
        IntegratedCircuitComponent? circuitComp = null)
    {
        if (!Resolve(assemblyUid, ref assemblyComp, false) ||
            !Resolve(circuitUid, ref circuitComp, false))
            return;

        // ФІЗИЧНО: Створюємо контейнер всередині корпусу і кладемо туди деталь
        // SS14 автоматично забере предмет з руки гравця!
        var container = _container.EnsureContainer<Container>(assemblyUid, "circuit_container");
        _container.Insert(circuitUid, container);

        assemblyComp.CircuitEntities.Add(circuitUid);
        circuitComp.AssemblyUid = assemblyUid;

        Dirty(assemblyUid, assemblyComp);
        Dirty(circuitUid, circuitComp);

        var ev = new CircuitAddedToAssemblyEvent(assemblyUid);
        RaiseLocalEvent(circuitUid, ev);
    }

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

        // ФІЗИЧНО: Дістаємо з контейнера і кладемо на координати самого корпусу (на підлогу)
        var container = _container.EnsureContainer<Container>(assemblyUid, "circuit_container");
        _container.Remove(circuitUid, container);
        _transform.SetCoordinates(circuitUid, _transform.GetMoverCoordinates(assemblyUid));

        Dirty(assemblyUid, assemblyComp);
        Dirty(circuitUid, circuitComp);

        var ev = new CircuitRemovedFromAssemblyEvent(assemblyUid);
        RaiseLocalEvent(circuitUid, ev);
    }

    #endregion
}