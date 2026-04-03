using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Client.IntegratedCircuits.UI;

[UsedImplicitly]
public sealed class ElectronicAssemblyBoundUI : BoundUserInterface
{
    private ElectronicAssemblyWindow? _window;

    public ElectronicAssemblyBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ElectronicAssemblyWindow>();

        _window.OnRemoveCircuit += netEntity =>
        {
            SendMessage(new AssemblyRemoveCircuitMessage(netEntity));
        };

        _window.OnRenamePressed += newName =>
        {
            SendMessage(new AssemblyRenameMessage(newName));
        };

        _window.OnRemoveBattery += () =>
        {
            SendMessage(new AssemblyRemoveBatteryMessage());
        };

        if (State != null)
            UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ElectronicAssemblyBoundUIState castState)
            _window?.UpdateState(castState);
    }
}
