using System;
using Content.Client._Pirate.Ghost;
using Content.Client.Ghost;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Ghost.Widgets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Timing;

namespace Content.Client._Pirate.UserInterface.Systems.Ghost;

public sealed class PirateGhostRespawnUIController : UIController, IOnSystemChanged<GhostSystem>, IOnSystemChanged<PirateGhostRespawnSystem>
{
    [UISystemDependency] private readonly GhostSystem? _ghost = default!;
    [UISystemDependency] private readonly PirateGhostRespawnSystem? _respawn = default!;

    private GhostGui? Gui => UIManager.GetActiveUIWidgetOrNull<GhostGui>();

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    public void OnSystemLoaded(GhostSystem system)
    {
        system.PlayerAttached += OnGhostStateChanged;
        system.PlayerUpdated += OnGhostStateChanged;
        system.PlayerDetached += OnGhostDetached;
        system.PlayerRemoved += OnGhostStateChanged;
    }

    public void OnSystemUnloaded(GhostSystem system)
    {
        system.PlayerAttached -= OnGhostStateChanged;
        system.PlayerUpdated -= OnGhostStateChanged;
        system.PlayerDetached -= OnGhostDetached;
        system.PlayerRemoved -= OnGhostStateChanged;
    }

    public void OnSystemLoaded(PirateGhostRespawnSystem system)
    {
        system.StatusChanged += UpdateGui;
    }

    public void OnSystemUnloaded(PirateGhostRespawnSystem system)
    {
        system.StatusChanged -= UpdateGui;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_ghost?.IsGhost == true)
            UpdateGui();
    }

    private void OnScreenLoad()
    {
        if (Gui == null)
            return;

        Gui.RespawnToLobbyPressed += OnRespawnToLobbyPressed;
        UpdateGui();
    }

    private void OnScreenUnload()
    {
        if (Gui == null)
            return;

        Gui.RespawnToLobbyPressed -= OnRespawnToLobbyPressed;
    }

    private void OnRespawnToLobbyPressed()
    {
        _respawn?.RequestRespawnToLobby();
    }

    private void OnGhostDetached()
    {
        _respawn?.Reset();
        UpdateGui();
    }

    private void OnGhostStateChanged(Content.Shared.Ghost.GhostComponent component)
    {
        UpdateGui();
    }

    private void UpdateGui()
    {
        if (Gui == null)
            return;

        if (_ghost?.IsGhost != true)
        {
            Gui.UpdatePirateRespawn(false, false, TimeSpan.Zero);
            return;
        }

        var state = _respawn?.GetDisplayState() ?? new PirateGhostRespawnDisplayState(false, false, TimeSpan.Zero);
        Gui.UpdatePirateRespawn(state.HasStatus, state.CanRespawn, state.RemainingTime);
    }
}
