using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Pirate.Shared.IntegratedCircuits.Systems;

/// <summary>
/// Handles circuit tool interactions: wiring pins with the wirer,
/// writing data / pulsing activators with the debugger, and scanning refs.
/// </summary>
public sealed class CircuitToolsSystem : EntitySystem
{
    [Dependency] private readonly SharedIntegratedCircuitSystem _circuits = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Wirer: toggle mode on use-in-hand.
        SubscribeLocalEvent<CircuitWirerComponent, UseInHandEvent>(OnWirerUseInHand);

        // Debugger: capture ref on AfterInteract (clicking on world entities).
        SubscribeLocalEvent<CircuitDebuggerComponent, AfterInteractEvent>(OnDebuggerAfterInteract);
        SubscribeLocalEvent<CircuitDebuggerComponent, UseInHandEvent>(OnDebuggerUseInHand);
    }

    #region Wirer

    /// <summary>
    /// Toggles the wirer between Wire and Unwire modes.
    /// If the wirer was mid-operation (Wiring/Unwiring), it cancels and resets.
    /// </summary>
    private void OnWirerUseInHand(Entity<CircuitWirerComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;

        switch (comp.Mode)
        {
            case WirerMode.Wire:
                comp.Mode = WirerMode.Unwire;
                break;
            case WirerMode.Wiring:
                comp.SelectedPin = null;
                comp.Mode = WirerMode.Wire;
                _popup.PopupEntity(Loc.GetString("circuit-wirer-cancel-wire"), ent, args.User);
                break;
            case WirerMode.Unwire:
                comp.Mode = WirerMode.Wire;
                break;
            case WirerMode.Unwiring:
                comp.SelectedPin = null;
                comp.Mode = WirerMode.Unwire;
                _popup.PopupEntity(Loc.GetString("circuit-wirer-cancel-unwire"), ent, args.User);
                break;
        }

        _appearance.SetData(ent, WirerVisuals.Mode, comp.Mode);
        Dirty(ent);
        args.Handled = true;
    }

    /// <summary>
    /// Called via UI or a system event to select a pin with the wirer.
    /// First call selects the pin; second call connects or disconnects.
    /// </summary>
    /// <param name="wirerUid">The wirer entity.</param>
    /// <param name="user">The player.</param>
    /// <param name="pinAddress">The pin being clicked on.</param>
    public void WirerSelectPin(EntityUid wirerUid, EntityUid user, PinAddress pinAddress,
        CircuitWirerComponent? comp = null)
    {
        if (!Resolve(wirerUid, ref comp, false))
            return;

        switch (comp.Mode)
        {
            case WirerMode.Wire:
            {
                // First step: select the pin.
                comp.SelectedPin = pinAddress;
                comp.Mode = WirerMode.Wiring;
                _popup.PopupEntity(Loc.GetString("circuit-wirer-select-wire"), wirerUid, user);
                break;
            }
            case WirerMode.Wiring:
            {
                // Second step: connect the two pins.
                if (comp.SelectedPin == null)
                {
                    // Shouldn't happen, but reset gracefully.
                    comp.Mode = WirerMode.Wire;
                    break;
                }

                if (comp.SelectedPin.ComponentUid == pinAddress.ComponentUid &&
                    comp.SelectedPin.PinType == pinAddress.PinType &&
                    comp.SelectedPin.PinIndex == pinAddress.PinIndex)
                {
                    _popup.PopupEntity(Loc.GetString("circuit-wirer-same-pin"), wirerUid, user);
                    break;
                }

                var success = _circuits.ConnectPins(comp.SelectedPin, pinAddress);
                if (success)
                    _popup.PopupEntity(Loc.GetString("circuit-wirer-connected"), wirerUid, user);
                else
                    _popup.PopupEntity(Loc.GetString("circuit-wirer-connect-failed"), wirerUid, user);

                comp.SelectedPin = null;
                comp.Mode = WirerMode.Wire;
                break;
            }
            case WirerMode.Unwire:
            {
                // First step: select the pin to unwire from.
                comp.SelectedPin = pinAddress;
                comp.Mode = WirerMode.Unwiring;
                _popup.PopupEntity(Loc.GetString("circuit-wirer-select-unwire"), wirerUid, user);
                break;
            }
            case WirerMode.Unwiring:
            {
                // Second step: disconnect the two pins.
                if (comp.SelectedPin == null)
                {
                    comp.Mode = WirerMode.Unwire;
                    break;
                }

                var success = _circuits.DisconnectPins(comp.SelectedPin, pinAddress);
                if (success)
                    _popup.PopupEntity(Loc.GetString("circuit-wirer-disconnected"), wirerUid, user);
                else
                    _popup.PopupEntity(Loc.GetString("circuit-wirer-disconnect-failed"), wirerUid, user);

                comp.SelectedPin = null;
                comp.Mode = WirerMode.Unwire;
                break;
            }
        }

        Dirty(wirerUid, comp);
    }

    #endregion

    #region Debugger

    /// <summary>
    /// Called via UI or a system event to write data to a specific pin using the debugger.
    /// For activator pins, this pulses the circuit instead of writing data.
    /// </summary>
    /// <param name="debuggerUid">The debugger entity.</param>
    /// <param name="user">The player.</param>
    /// <param name="pinAddress">The target pin to write to / pulse.</param>
    public void DebuggerWriteToPin(EntityUid debuggerUid, EntityUid user, PinAddress pinAddress,
        CircuitDebuggerComponent? comp = null)
    {
        if (!Resolve(debuggerUid, ref comp, false))
            return;

        if (!TryComp<IntegratedCircuitComponent>(pinAddress.ComponentUid, out var circuitComp))
            return;

        var pin = _circuits.GetPin(circuitComp, pinAddress.PinType, pinAddress.PinIndex);
        if (pin == null)
            return;

        if (pin.PinType == PinType.Activator)
        {
            // Pulse the activator instead of writing data.
            _circuits.ActivateCircuit(pinAddress.ComponentUid, pinAddress.PinIndex, circuitComp);
            _popup.PopupEntity(Loc.GetString("circuit-debugger-pulse"), debuggerUid, user);
            return;
        }

        // Write the debugger's stored data to the data pin.
        var success = _circuits.WritePinData(
            pinAddress.ComponentUid,
            pinAddress.PinType,
            pinAddress.PinIndex,
            comp.StoredData);

        if (success)
            _popup.PopupEntity(Loc.GetString("circuit-debugger-write-success"), debuggerUid, user);
        else
            _popup.PopupEntity(Loc.GetString("circuit-debugger-write-failed"), debuggerUid, user);
    }

    /// <summary>
    /// Sets the debugger's mode and stored value.
    /// </summary>
    /// <param name="debuggerUid">The debugger entity.</param>
    /// <param name="mode">The new mode.</param>
    /// <param name="data">The data to store (ignored for Ref and Null modes).</param>
    public void DebuggerSetMode(EntityUid debuggerUid, DebuggerMode mode, object? data = null,
        CircuitDebuggerComponent? comp = null)
    {
        if (!Resolve(debuggerUid, ref comp, false))
            return;

        comp.Mode = mode;
        comp.AcceptingRefs = mode == DebuggerMode.Ref;

        switch (mode)
        {
            case DebuggerMode.Null:
                comp.StoredData = null;
                break;
            case DebuggerMode.Ref:
                // Data is set when the player clicks on a target entity.
                break;
            case DebuggerMode.String:
            case DebuggerMode.Number:
                comp.StoredData = data;
                break;
        }

        Dirty(debuggerUid, comp);
    }

    /// <summary>
    /// When the debugger is in Ref mode and the player clicks on an entity,
    /// store that entity's UID as the debugger's data.
    /// </summary>
    private void OnDebuggerAfterInteract(Entity<CircuitDebuggerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!ent.Comp.AcceptingRefs)
            return;

        if (args.Target is not { } target)
            return;

        ent.Comp.StoredData = target;
        ent.Comp.AcceptingRefs = false;
        Dirty(ent);

        _popup.PopupEntity(
            Loc.GetString("circuit-debugger-ref-stored", ("target", target)),
            ent, args.User);

        args.Handled = true;
    }

    private void OnDebuggerUseInHand(EntityUid uid, CircuitDebuggerComponent comp, UseInHandEvent args)
    {
        if (args.Handled) return;

        // Перемикаємо режими по колу для зручності тестування
        if (comp.Mode == DebuggerMode.String)
        {
            DebuggerSetMode(uid, DebuggerMode.Number, 1f, comp);
            _popup.PopupEntity("Дебагер: Режим [ЧИСЛО] (значення: 1)", uid, args.User);
        }
        else if (comp.Mode == DebuggerMode.Number)
        {
            DebuggerSetMode(uid, DebuggerMode.Ref, null, comp);
            _popup.PopupEntity("Дебагер: Режим [REF] (Клікніть по предмету)", uid, args.User);
        }
        else
        {
            // Для тесту одразу записуємо туди якийсь текст
            DebuggerSetMode(uid, DebuggerMode.String, "Привіт світ!", comp);
            _popup.PopupEntity("Дебагер: Режим [ТЕКСТ] ('Привіт світ!')", uid, args.User);
        }
        
        args.Handled = true;
    }

    #endregion
}
