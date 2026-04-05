using System.Globalization;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Pirate.Server.IntegratedCircuits;

public sealed class CircuitDebuggerUISystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly CircuitToolsSystem _tools = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CircuitDebuggerComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CircuitDebuggerComponent, DebuggerSetModeMessage>(OnSetMode);
        SubscribeLocalEvent<CircuitDebuggerComponent, DebuggerSetValueMessage>(OnSetValue);
        SubscribeLocalEvent<CircuitDebuggerComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnUIOpened(EntityUid uid, CircuitDebuggerComponent comp, BoundUIOpenedEvent args)
    {
        UpdateUI(uid, comp);
    }

    private void OnAfterInteract(EntityUid uid, CircuitDebuggerComponent comp, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!comp.AcceptingRefs)
            return;

        if (args.Target is not { } target)
            return;

        comp.StoredData = target;
        comp.AcceptingRefs = false;
        Dirty(uid, comp);

        _popup.PopupEntity(Loc.GetString("circuit-debugger-ref-stored", ("target", target)), uid, args.User);

        UpdateUI(uid, comp);

        args.Handled = true;
    }

    private void OnSetMode(EntityUid uid, CircuitDebuggerComponent comp, DebuggerSetModeMessage msg)
    {
        _tools.DebuggerSetMode(uid, msg.Mode, comp.StoredData, comp);
        UpdateUI(uid, comp);
    }

    private void OnSetValue(EntityUid uid, CircuitDebuggerComponent comp, DebuggerSetValueMessage msg)
    {
        if (comp.Mode == DebuggerMode.String)
        {
            _tools.DebuggerSetMode(uid, DebuggerMode.String, msg.Value, comp);
        }
        else if (comp.Mode == DebuggerMode.Number)
        {
            // Намагаємось розпарсити число (підтримує крапку і кому)
            if (float.TryParse(msg.Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                _tools.DebuggerSetMode(uid, DebuggerMode.Number, num, comp);
            }
            else
            {
                _popup.PopupEntity("Invalid number format!", uid, msg.Actor);
                return;
            }
        }
        
        UpdateUI(uid, comp);
    }

    public void UpdateUI(EntityUid uid, CircuitDebuggerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false)) return;

        string display = "(null)";
        if (comp.StoredData != null)
        {
            if (comp.StoredData is EntityUid ent)
                display = $"[Ref] {Name(ent)}";
            else if (comp.StoredData is string str)
                display = $"\"{str}\"";
            else
                display = comp.StoredData.ToString() ?? "(null)";
        }

        var state = new CircuitDebuggerBoundUIState(comp.Mode, display, comp.AcceptingRefs);
        _ui.SetUiState(uid, CircuitDebuggerUiKey.Key, state);
    }
}
