using Content.Pirate.Shared.IntegratedCircuits;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.Visualizers;

public sealed class AssemblyVisualizerSystem : VisualizerSystem<ElectronicAssemblyComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, ElectronicAssemblyComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Читаємо дані, які прислав сервер
        AppearanceSystem.TryGetData(uid, AssemblyVisuals.Opened, out bool opened, args.Component);
        AppearanceSystem.TryGetData(uid, AssemblyVisuals.Color, out Color color, args.Component);

        // Формуємо назву стану (наприклад: "setup_small" або "setup_small-open")
        var state = comp.BaseState;
        if (opened)
            state += "-open";

        // Шар 0: Базовий корпус
        args.Sprite.LayerSetState(0, state);

        // Шар 1: Кольорова маска (додаємо "-color" до назви стану)
        args.Sprite.LayerSetState(1, state + "-color");
        args.Sprite.LayerSetColor(1, color);
    }
}
