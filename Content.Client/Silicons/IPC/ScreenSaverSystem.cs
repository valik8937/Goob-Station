using Content.Shared.Silicons.IPC;
using Robust.Client.GameObjects;

namespace Content.Client.Silicons.IPC;

public sealed class ScreenSaverSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScreenSaverActionEvent>(OnAction);
    }

    private void OnAction(ScreenSaverActionEvent args)
    {
        if (args.Handled)
            return;

        var uiController = IoCManager.Resolve<Robust.Client.UserInterface.IUserInterfaceManager>().GetUIController<ScreenSaverUIController>();
        uiController.ToggleMenu(args.Performer);
        
        args.Handled = true;
    }
}
