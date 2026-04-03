using System.Linq;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Events;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Shared.IntegratedCircuits.Systems;

/// <summary>
/// Core system for managing integrated circuit pins, wiring, data flow, and activation.
/// Provides the API that other systems use to read/write pin data and trigger circuits.
/// </summary>
public abstract class SharedIntegratedCircuitSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// Maximum number of elements a list pin can hold.
    /// </summary>
    public const int MaxListLength = 500;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntegratedCircuitComponent, ComponentRemove>(OnCircuitRemoved);
    }

    private void OnCircuitRemoved(Entity<IntegratedCircuitComponent> ent, ref ComponentRemove args)
    {
        DisconnectAll(ent);
    }

    #region Pin Access

    /// <summary>
    /// Gets the pin list of the given type from a circuit component.
    /// </summary>
    public List<IntegratedPin>? GetPinList(IntegratedCircuitComponent comp, PinType pinType)
    {
        return pinType switch
        {
            PinType.Input => comp.InputPins,
            PinType.Output => comp.OutputPins,
            PinType.Activator => comp.ActivatorPins,
            _ => null,
        };
    }

    /// <summary>
    /// Gets a specific pin by type and index. Returns null if out of bounds.
    /// </summary>
    public IntegratedPin? GetPin(IntegratedCircuitComponent comp, PinType pinType, int index)
    {
        var list = GetPinList(comp, pinType);
        if (list == null || index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    /// <summary>
    /// Gets a specific pin by type and index from an entity.
    /// </summary>
    public IntegratedPin? GetPin(EntityUid uid, PinType pinType, int index,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return null;

        return GetPin(comp, pinType, index);
    }

    #endregion

    #region Data Read/Write

    /// <summary>
    /// Reads the current data value from the specified pin.
    /// </summary>
    public object? ReadPinData(EntityUid uid, PinType pinType, int pinIndex,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return null;

        var pin = GetPin(comp, pinType, pinIndex);
        return pin?.Data;
    }

    /// <summary>
    /// Writes data to a pin, validating it against the pin's data type.
    /// Returns true if the data was successfully written.
    /// </summary>
    public bool WritePinData(EntityUid uid, PinType pinType, int pinIndex, object? data,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        var pin = GetPin(comp, pinType, pinIndex);
        if (pin == null)
            return false;

        // Activator pins don't store data.
        if (pin.PinType == PinType.Activator)
            return false;

        if (!ValidatePinData(pin.DataType, data))
            return false;

        pin.Data = data;
        Dirty(uid, comp);

        var ev = new PinDataChangedEvent(pinType, pinIndex, data);
        RaiseLocalEvent(uid, ev);

        return true;
    }

    /// <summary>
    /// Validates that <paramref name="data"/> is acceptable for a pin of the given data type.
    /// </summary>
    public bool ValidatePinData(PinDataType dataType, object? data)
    {
        if (data == null)
        {
            // Boolean and List pins don't accept null.
            return dataType != PinDataType.Boolean && dataType != PinDataType.List;
        }

        return dataType switch
        {
            PinDataType.Any => data is float or int or double or string or EntityUid or List<object?>,
            PinDataType.Number => data is float or int or double,
            PinDataType.String => data is string,
            PinDataType.Boolean => data is bool,
            PinDataType.Ref => data is EntityUid,
            PinDataType.List => data is List<object?>,
            PinDataType.Color => data is string s && IsValidHexColor(s),
            PinDataType.Dir => data is int dir && IsValidDirection(dir),
            PinDataType.Index => data is int idx && idx >= 0 && idx <= MaxListLength,
            PinDataType.Char => data is string ch && ch.Length == 1,
            _ => false,
        };
    }

    private static bool IsValidHexColor(string s)
    {
        if (s.Length != 7 || s[0] != '#')
            return false;

        for (var i = 1; i < 7; i++)
        {
            var c = char.ToUpperInvariant(s[i]);
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
                return false;
        }

        return true;
    }

    private static bool IsValidDirection(int dir)
    {
        // Standard SS14 AtmosDirection / Direction values: N=1, S=2, E=4, W=8, NE=5, NW=9, SE=6, SW=10
        return dir is 1 or 2 or 4 or 8 or 5 or 6 or 9 or 10;
    }

    #endregion

    #region Wiring

    /// <summary>
    /// Connects two pins together. Both pins must be of the same channel type
    /// (both data or both activator). Returns true on success.
    /// </summary>
    public bool ConnectPins(PinAddress from, PinAddress to)
    {
        if (!TryComp<IntegratedCircuitComponent>(from.ComponentUid, out var fromComp) ||
            !TryComp<IntegratedCircuitComponent>(to.ComponentUid, out var toComp))
            return false;

        var fromPin = GetPin(fromComp, from.PinType, from.PinIndex);
        var toPin = GetPin(toComp, to.PinType, to.PinIndex);
        if (fromPin == null || toPin == null)
            return false;

        // Can't wire a pin to itself.
        if (from.ComponentUid == to.ComponentUid && from.PinType == to.PinType && from.PinIndex == to.PinIndex)
            return false;

        // Channel types must match: data ↔ data, activator ↔ activator.
        var fromIsActivator = fromPin.PinType == PinType.Activator;
        var toIsActivator = toPin.PinType == PinType.Activator;
        if (fromIsActivator != toIsActivator)
            return false;

        // Prevent connecting same-type data pins (Input↔Input, Output↔Output).
        // Only Input↔Output and Activator↔Activator are valid.
        if (!fromIsActivator && fromPin.PinType == toPin.PinType)
            return false;

        // Check if they must be in the same assembly.
        if (fromComp.AssemblyUid != toComp.AssemblyUid)
            return false;

        // Avoid duplicate connections.
        if (fromPin.LinkedPins.Any(p =>
                p.ComponentUid == to.ComponentUid && p.PinType == to.PinType && p.PinIndex == to.PinIndex))
            return false;

        fromPin.LinkedPins.Add(to);
        toPin.LinkedPins.Add(from);

        Dirty(from.ComponentUid, fromComp);
        Dirty(to.ComponentUid, toComp);

        return true;
    }

    /// <summary>
    /// Disconnects two previously connected pins.
    /// </summary>
    public bool DisconnectPins(PinAddress from, PinAddress to)
    {
        if (!TryComp<IntegratedCircuitComponent>(from.ComponentUid, out var fromComp) ||
            !TryComp<IntegratedCircuitComponent>(to.ComponentUid, out var toComp))
            return false;

        var fromPin = GetPin(fromComp, from.PinType, from.PinIndex);
        var toPin = GetPin(toComp, to.PinType, to.PinIndex);
        if (fromPin == null || toPin == null)
            return false;

        var removed = fromPin.LinkedPins.RemoveAll(p =>
            p.ComponentUid == to.ComponentUid && p.PinType == to.PinType && p.PinIndex == to.PinIndex) > 0;

        removed |= toPin.LinkedPins.RemoveAll(p =>
            p.ComponentUid == from.ComponentUid && p.PinType == from.PinType && p.PinIndex == from.PinIndex) > 0;

        if (removed)
        {
            Dirty(from.ComponentUid, fromComp);
            Dirty(to.ComponentUid, toComp);
        }

        return removed;
    }

    /// <summary>
    /// Disconnects all wires from all pins on a circuit.
    /// </summary>
    public void DisconnectAll(EntityUid uid, IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        DisconnectAllFromList(uid, comp.InputPins, PinType.Input);
        DisconnectAllFromList(uid, comp.OutputPins, PinType.Output);
        DisconnectAllFromList(uid, comp.ActivatorPins, PinType.Activator);
    }

    private void DisconnectAllFromList(EntityUid uid, List<IntegratedPin> pins, PinType pinType)
    {
        for (var i = 0; i < pins.Count; i++)
        {
            var pin = pins[i];
            var address = new PinAddress(uid, pinType, i);

            // Make a copy to avoid modifying during iteration.
            var linkedCopy = new List<PinAddress>(pin.LinkedPins);
            foreach (var linked in linkedCopy)
            {
                DisconnectPins(address, linked);
            }
        }
    }

    #endregion

    #region Data Propagation

    /// <summary>
    /// Pushes the data from the specified output pin to all connected input pins.
    /// </summary>
    public void PushData(EntityUid uid, PinType pinType, int pinIndex,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        var pin = GetPin(comp, pinType, pinIndex);
        if (pin == null)
            return;

        foreach (var linked in pin.LinkedPins)
        {
            if (pin.PinType == PinType.Activator)
            {
                // Activator pins trigger the target circuit.
                ActivateCircuit(linked.ComponentUid, linked.PinIndex);
            }
            else
            {
                // Data pins copy their value to the linked pin.
                WritePinData(linked.ComponentUid, linked.PinType, linked.PinIndex, pin.Data);
            }
        }
    }

    /// <summary>
    /// Pushes data from all output pins of a circuit.
    /// </summary>
    public void PushAllOutputs(EntityUid uid, IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        for (var i = 0; i < comp.OutputPins.Count; i++)
        {
            PushData(uid, PinType.Output, i, comp);
        }
    }

    #endregion

    #region Activation

    /// <summary>
    /// Activates a circuit through one of its activator pins.
    /// Checks cooldown and assembly loop-protection limits,
    /// then raises <see cref="CircuitActivatedEvent"/>.
    /// Virtual so that the server system can add power checks.
    /// </summary>
    public virtual bool ActivateCircuit(EntityUid uid, int activatorIndex,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        if (activatorIndex < 0 || activatorIndex >= comp.ActivatorPins.Count)
            return false;

        // Check cooldown.
        var curTime = _timing.CurTime;
        if (curTime - comp.LastActivation < TimeSpan.FromSeconds(comp.CooldownPerUse))
            return false;

        // Check assembly-level loop protection with lazy per-tick reset.
        // Instead of resetting counters every frame in Update() (expensive with many assemblies),
        // we reset on first activation each tick — zero cost when idle.
        if (comp.AssemblyUid is { } assemblyUid &&
            TryComp<ElectronicAssemblyComponent>(assemblyUid, out var assemblyComp))
        {
            if (assemblyComp.LastActivationTick != curTime)
            {
                assemblyComp.LastActivationTick = curTime;
                assemblyComp.CurrentTickActivations = 0;
            }

            if (assemblyComp.CurrentTickActivations >= assemblyComp.MaxActivationsPerTick)
                return false; // Short-circuit: too many activations this tick.

            assemblyComp.CurrentTickActivations++;
        }

        comp.LastActivation = curTime;

        var ev = new CircuitActivatedEvent(activatorIndex);
        RaiseLocalEvent(uid, ev);

        return true;
    }

    #endregion

    #region Target Resolution

    /// <summary>
    /// Returns the entity that represents the circuit's physical presence in the world.
    /// If the circuit is inside an assembly, returns the assembly.
    /// If the circuit is lying on its own, returns itself.
    /// </summary>
    public EntityUid GetActingEntity(EntityUid circuitUid, IntegratedCircuitComponent? comp = null)
    {
        if (Resolve(circuitUid, ref comp, false) && comp.AssemblyUid is { } assemblyUid)
            return assemblyUid;

        return circuitUid;
    }

    /// <summary>
    /// Checks whether a circuit (via its assembly or itself) can physically interact with a target entity.
    /// Uses transform system for range checking.
    /// </summary>
    /// <param name="circuitUid">The circuit entity.</param>
    /// <param name="target">The target to interact with.</param>
    /// <param name="range">Maximum interaction range in tiles. Default ~1.5 for adjacent.</param>
    /// <param name="comp">Optional resolved component.</param>
    /// <returns>True if the target is within range.</returns>
    public bool CanInteractWith(EntityUid circuitUid, EntityUid target, float range = 1.5f,
        IntegratedCircuitComponent? comp = null)
    {
        var actingEntity = GetActingEntity(circuitUid, comp);

        // Can't interact with ourselves.
        if (actingEntity == target)
            return false;

        // Both must exist in the world.
        if (!TryComp<TransformComponent>(actingEntity, out var actingXform) ||
            !TryComp<TransformComponent>(target, out var targetXform))
            return false;

        // Must be on the same map.
        if (actingXform.MapID != targetXform.MapID)
            return false;

        var actingPos = _transform.GetWorldPosition(actingXform);
        var targetPos = _transform.GetWorldPosition(targetXform);
        var distSq = (actingPos - targetPos).LengthSquared();

        return distSq <= range * range;
    }

    #endregion
}
