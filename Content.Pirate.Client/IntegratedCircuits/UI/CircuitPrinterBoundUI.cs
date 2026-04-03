using Content.Pirate.Shared.IntegratedCircuits.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.UI;

[UsedImplicitly]
public sealed class CircuitPrinterBoundUI : BoundUserInterface
{
    private CircuitPrinterWindow? _window;

    public CircuitPrinterBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CircuitPrinterWindow>();

        _window.OnBuildPressed += prototypeId =>
        {
            SendMessage(new CircuitPrinterBuildMessage(prototypeId));
        };

        _window.OnCategorySelected += category =>
        {
            SendMessage(new CircuitPrinterCategoryMessage(category));
        };

        if (State != null)
            UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is CircuitPrinterBoundUIState castState)
            _window?.UpdateState(castState);
    }
}
