using Content.Pirate.Shared.IntegratedCircuits.UI;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.UI;

public sealed class AssemblyInteractBoundUI : BoundUserInterface
{
    private AssemblyInteractWindow? _window;

    public AssemblyInteractBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<AssemblyInteractWindow>();
        
        _window.OnOptionSelected += netEntity =>
        {
            SendMessage(new AssemblyInteractSelectMessage(netEntity));
        };

        if (State != null) UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is AssemblyInteractBoundUIState castState)
            _window?.UpdateState(castState);
    }
}
