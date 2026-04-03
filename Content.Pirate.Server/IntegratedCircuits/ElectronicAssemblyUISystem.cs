using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

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

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to UI messages from the client.
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRemoveCircuitMessage>(OnRemoveCircuit);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRemoveBatteryMessage>(OnRemoveBattery);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyRenameMessage>(OnRename);

        // Update BUI state whenever the component changes.
        SubscribeLocalEvent<ElectronicAssemblyComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnUIOpened(EntityUid uid, ElectronicAssemblyComponent comp, BoundUIOpenedEvent args)
    {
        UpdateUI(uid, comp);
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

            var name = circuitComp.DisplayName ?? Name(circuitUid);

            circuits.Add(new AssemblyCircuitEntry
            {
                NetEntity = GetNetEntity(circuitUid),
                Name = name,
                Removable = circuitComp.Removable,
            });
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
