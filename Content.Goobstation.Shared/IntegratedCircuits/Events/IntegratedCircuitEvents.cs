using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.IntegratedCircuits.Events;

/// <summary>
/// Raised on a circuit entity when one of its activator pins receives a pulse.
/// Systems implementing specific circuit behavior should subscribe to this event.
/// </summary>
[Serializable, NetSerializable]
public sealed class CircuitActivatedEvent : EntityEventArgs
{
    /// <summary>
    /// Zero-based index of the activator pin that was pulsed.
    /// </summary>
    public int ActivatorIndex { get; }

    public CircuitActivatedEvent(int activatorIndex)
    {
        ActivatorIndex = activatorIndex;
    }
}

/// <summary>
/// Raised on a circuit entity when it is added to an assembly.
/// </summary>
public sealed class CircuitAddedToAssemblyEvent : EntityEventArgs
{
    /// <summary>
    /// The assembly entity the circuit was added to.
    /// </summary>
    public EntityUid AssemblyUid { get; }

    public CircuitAddedToAssemblyEvent(EntityUid assemblyUid)
    {
        AssemblyUid = assemblyUid;
    }
}

/// <summary>
/// Raised on a circuit entity when it is removed from an assembly.
/// </summary>
public sealed class CircuitRemovedFromAssemblyEvent : EntityEventArgs
{
    /// <summary>
    /// The assembly entity the circuit was removed from.
    /// </summary>
    public EntityUid AssemblyUid { get; }

    public CircuitRemovedFromAssemblyEvent(EntityUid assemblyUid)
    {
        AssemblyUid = assemblyUid;
    }
}

/// <summary>
/// Raised on a circuit entity when data is written to one of its pins.
/// </summary>
public sealed class PinDataChangedEvent : EntityEventArgs
{
    public PinType PinType { get; }
    public int PinIndex { get; }
    public object? NewData { get; }

    public PinDataChangedEvent(PinType pinType, int pinIndex, object? newData)
    {
        PinType = pinType;
        PinIndex = pinIndex;
        NewData = newData;
    }
}
