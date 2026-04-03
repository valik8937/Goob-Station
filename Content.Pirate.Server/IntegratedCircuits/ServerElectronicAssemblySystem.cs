using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Server-side electronic assembly system that handles power management.
/// Drains idle power from the assembly's battery (inserted as a separate power cell
/// in the "cell_slot" container) for each installed circuit.
/// </summary>
public sealed class ServerElectronicAssemblySystem : SharedElectronicAssemblySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Drain idle power from each assembly's battery.
        var query = EntityQueryEnumerator<ElectronicAssemblyComponent>();
        while (query.MoveNext(out var uid, out var assembly))
        {
            DrainIdlePower(uid, assembly, frameTime);
        }
    }

    /// <summary>
    /// Drains idle power from the assembly's battery for all installed circuits
    /// that have a non-zero idle power draw.
    /// </summary>
    private void DrainIdlePower(EntityUid assemblyUid, ElectronicAssemblyComponent assembly, float frameTime)
    {
        // Look for a battery in the assembly — either on the entity itself
        // or in a "cell_slot" container (insertable power cell).
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out var batteryUid))
            return;

        var totalIdleDraw = 0f;
        foreach (var circuitUid in assembly.CircuitEntities)
        {
            if (TryComp<IntegratedCircuitComponent>(circuitUid, out var circuit) && circuit.PowerDrawIdle > 0)
            {
                totalIdleDraw += circuit.PowerDrawIdle;
            }
        }

        if (totalIdleDraw <= 0)
            return;

        // Power is in watts, frameTime is in seconds, battery stores joules.
        var energyToUse = totalIdleDraw * frameTime;
        _battery.UseCharge(batteryUid.Value, energyToUse, battery);
    }

    /// <summary>
    /// Attempts to use a given amount of energy (in joules) from the assembly's battery.
    /// Finds the battery via cell_slot or directly on the entity.
    /// Returns true if the battery had sufficient charge and the energy was consumed.
    /// </summary>
    public bool TryUsePower(EntityUid assemblyUid, float amount)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out var batteryUid))
            return false;

        return _battery.TryUseCharge(batteryUid.Value, amount, battery);
    }

    /// <summary>
    /// Checks if the assembly has a battery with any remaining charge.
    /// </summary>
    public bool HasPower(EntityUid assemblyUid)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out _))
            return false;

        return battery.CurrentCharge > 0;
    }

    /// <summary>
    /// Checks if the assembly has enough charge for the given amount.
    /// </summary>
    public bool HasEnoughPower(EntityUid assemblyUid, float amount)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out _))
            return false;

        return battery.CurrentCharge >= amount;
    }
}
