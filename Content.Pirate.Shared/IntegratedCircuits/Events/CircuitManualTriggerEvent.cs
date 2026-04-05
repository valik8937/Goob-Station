using Robust.Shared.GameObjects;

namespace Content.Pirate.Shared.IntegratedCircuits.Events;

/// <summary>
/// Викликається на мікросхемі, коли гравець вибрав її з меню (або вона активувалася автоматично, бо була одна).
/// </summary>
public sealed class CircuitManualTriggerEvent : EntityEventArgs
{
    public EntityUid User { get; }

    public CircuitManualTriggerEvent(EntityUid user)
    {
        User = user;
    }
}
