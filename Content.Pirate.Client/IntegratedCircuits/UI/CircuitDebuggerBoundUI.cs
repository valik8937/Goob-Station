using Content.Pirate.Shared.IntegratedCircuits.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.UI;

[UsedImplicitly]
public sealed class CircuitDebuggerBoundUI : BoundUserInterface
{
    private CircuitDebuggerWindow? _window;

    public CircuitDebuggerBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<CircuitDebuggerWindow>();
        
        _window.OnModeSelected += mode => SendMessage(new DebuggerSetModeMessage(mode));
        _window.OnDataSaved += value => SendMessage(new DebuggerSetValueMessage(value));

        if (State != null) UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is CircuitDebuggerBoundUIState castState)
            _window?.UpdateState(castState);
    }
}
