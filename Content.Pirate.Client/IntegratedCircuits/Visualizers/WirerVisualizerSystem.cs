using Content.Pirate.Shared.IntegratedCircuits;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.Visualizers;

public sealed class WirerVisualizerSystem : VisualizerSystem<CircuitWirerComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, CircuitWirerComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData(uid, WirerVisuals.Mode, out WirerMode mode, args.Component))
            return;

        // Змінюємо текстуру залежно від режиму
        var state = mode switch
        {
            WirerMode.Wire => "wirer-wire",
            WirerMode.Wiring => "wirer-wiring",
            WirerMode.Unwire => "wirer-unwire",
            WirerMode.Unwiring => "wirer-unwiring",
            _ => "wirer-wire"
        };

        args.Sprite.LayerSetState(0, state);
    }
}
