using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Pirate.Shared.IntegratedCircuits;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Content.Shared.Popups;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Server-side system that handles BUI (Bound User Interface) interactions
/// for electronic assemblies: sending state updates, processing remove/rename messages.
/// </summary>
public sealed class ElectronicAssemblyUISystem : EntitySystem
{
    [Dependency] private readonly SharedElectronicAssemblySystem _assemblySys = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly Content.Shared.Containers.ItemSlots.ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly Content.Shared.Hands.EntitySystems.SharedHandsSystem _hands = default!;
    [Dependency] private readonly CircuitToolsSystem _tools = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to UI messages from the client.
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRemoveCircuitMessage>(OnRemoveCircuit);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRemoveBatteryMessage>(OnRemoveBattery);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRenameMessage>(OnRename);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyPinClickMessage>(OnPinClicked);

        // Update BUI state whenever the component changes.
        SubscribeLocalEvent<ElectronicAssemblyComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnUIOpened(EntityUid uid, ElectronicAssemblyComponent comp, BoundUIOpenedEvent args)
    {
        UpdateUI(uid, comp);
    }

    private void OnPinClicked(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyPinClickMessage msg)
    {
        var user = msg.Actor;
        var circuitUid = GetEntity(msg.CircuitEntity);

        if (!comp.CircuitEntities.Contains(circuitUid)) return;

        if (!_hands.TryGetActiveItem(user, out var activeHandItem))
        {
            _popup.PopupEntity("Вам потрібен інструмент (Wirer або Debugger) в руці!", uid, user);
            return;
        }

        var pinAddress = new PinAddress(circuitUid, msg.PinType, msg.PinIndex);

        if (HasComp<CircuitWirerComponent>(activeHandItem.Value))
        {
            _tools.WirerSelectPin(activeHandItem.Value, user, pinAddress);
            UpdateUI(uid, comp);
            return;
        }

        if (HasComp<CircuitDebuggerComponent>(activeHandItem.Value))
        {
            _tools.DebuggerWriteToPin(activeHandItem.Value, user, pinAddress);
            UpdateUI(uid, comp);
            return;
        }

        _popup.PopupEntity("Цей інструмент не підходить для пінів.", uid, user);
    }

    /// <summary>
    /// Handle client request to remove a circuit from the assembly.
    /// </summary>
    private void OnRemoveCircuit(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyRemoveCircuitMessage msg)
    {
        var circuitUid = GetEntity(msg.CircuitEntity);

        if (!TryComp<IntegratedCircuitComponent>(circuitUid, out var circuitComp))
            return;

        _assemblySys.TryRemoveCircuit(uid, circuitUid, comp, circuitComp);
        UpdateUI(uid, comp);
    }

    /// <summary>
    /// Handle client request to remove the battery.
    /// </summary>
    private void OnRemoveBattery(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyRemoveBatteryMessage msg)
    {
        var user = msg.Actor;

        if (_itemSlots.TryGetSlot(uid, "battery_slot", out var slot))
        {
            _itemSlots.TryEjectToHands(uid, slot, user);
        }

        UpdateUI(uid, comp);
    }

    /// <summary>
    /// Handle client request to rename the assembly.
    /// </summary>
    private void OnRename(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyRenameMessage msg)
    {
        var newName = msg.NewName.Trim();
        if (string.IsNullOrEmpty(newName) || newName.Length > 64)
            return;

        _metadata.SetEntityName(uid, newName);
        UpdateUI(uid, comp);
    }

    private string FormatPinData(IntegratedPin pin)
    {
        if (pin.PinType == PinType.Activator) return "<PULSE>";
        if (pin.Data == null) return "(null)";
        if (pin.Data is string s) return $"(\"{s}\")";
        if (pin.Data is EntityUid ent) return $"({Name(ent)} [Ref])";
        return $"({pin.Data})";
    }

    /// <summary>
    /// Builds and sends the current assembly state to all BUI subscribers.
    /// </summary>
    public void UpdateUI(EntityUid uid, ElectronicAssemblyComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        var circuits = new List<AssemblyCircuitEntry>();
        foreach (var circuitUid in comp.CircuitEntities)
        {
            if (!TryComp<IntegratedCircuitComponent>(circuitUid, out var circuitComp))
                continue;

            var entry = new AssemblyCircuitEntry
            {
                NetEntity = GetNetEntity(circuitUid),
                Name = circuitComp.DisplayName ?? Name(circuitUid),
                Description = Description(circuitUid),
                Removable = circuitComp.Removable,
                Complexity = circuitComp.Complexity,
                Size = circuitComp.Size,
                PowerDrawIdle = circuitComp.PowerDrawIdle,
                PowerDrawPerUse = circuitComp.PowerDrawPerUse
            };

            CircuitPinInfo MapPin(IntegratedPin pin)
            {
                var info = new CircuitPinInfo
                {
                    Name = pin.Name,
                    PinType = pin.PinType,
                    DataType = pin.DataType,
                    DisplayData = FormatPinData(pin),
                    IsConnected = pin.LinkedPins.Count > 0
                };

                var connections = new List<string>();
                foreach(var link in pin.LinkedPins)
                {
                    var linkName = Name(link.ComponentUid);
                    connections.Add(linkName);
                }
                info.ConnectedTo = string.Join(", ", connections);

                return info;
            }

            foreach (var p in circuitComp.InputPins) entry.InputPins.Add(MapPin(p));
            foreach (var p in circuitComp.OutputPins) entry.OutputPins.Add(MapPin(p));
            foreach (var p in circuitComp.ActivatorPins) entry.ActivatorPins.Add(MapPin(p));

            circuits.Add(entry);
        }

        // Battery info
        var hasBattery = false;
        var batteryCharge = 0f;
        var batteryMax = 0f;

        if (_itemSlots.GetItemOrNull(uid, "battery_slot") is { } batteryUid && 
            _battery.TryGetBatteryComponent(batteryUid, out var battery, out _))
        {
            hasBattery = true;
            batteryCharge = battery.CurrentCharge;
            batteryMax = battery.MaxCharge;
        }

        var state = new ElectronicAssemblyBoundUIState
        {
            IsOpened = comp.Opened,
            TotalSize = _assemblySys.GetTotalSize(uid, comp),
            MaxSize = comp.MaxComponents,
            TotalComplexity = _assemblySys.GetTotalComplexity(uid, comp),
            MaxComplexity = comp.MaxComplexity,
            HasBattery = hasBattery,
            BatteryCharge = batteryCharge,
            BatteryMaxCharge = batteryMax,
            Circuits = circuits,
            AssemblyName = Name(uid),
        };

        _ui.SetUiState(uid, ElectronicAssemblyUiKey.Key, state);
    }
}

