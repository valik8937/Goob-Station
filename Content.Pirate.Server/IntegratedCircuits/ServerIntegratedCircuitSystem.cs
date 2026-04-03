using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Server-side integrated circuit system that adds power checks to circuit activation.
/// Before a circuit is activated, this system verifies the assembly has sufficient
/// battery charge (from an inserted power cell) for the circuit's per-use power draw.
/// </summary>
public sealed class ServerIntegratedCircuitSystem : SharedIntegratedCircuitSystem
{
    [Dependency] private readonly ServerElectronicAssemblySystem _assembly = default!;

    /// <summary>
    /// Overrides the base activation to add power checks.
    /// If the circuit requires power per use and the assembly doesn't have enough,
    /// the activation is denied.
    /// </summary>
    public override bool ActivateCircuit(EntityUid uid, int activatorIndex,
        IntegratedCircuitComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        // Check power requirements before activation.
        if (comp.PowerDrawPerUse > 0 && comp.AssemblyUid is { } assemblyUid)
        {
            if (!_assembly.HasEnoughPower(assemblyUid, comp.PowerDrawPerUse))
                return false; // Not enough power or no battery.
        }

        // Perform base activation (cooldown, loop protection, raise event).
        var result = base.ActivateCircuit(uid, activatorIndex, comp);

        // If activation succeeded and power is required, consume it now.
        if (result && comp.PowerDrawPerUse > 0 && comp.AssemblyUid is { } asmUid)
        {
            _assembly.TryUsePower(asmUid, comp.PowerDrawPerUse);
        }

        return result;
    }
}
