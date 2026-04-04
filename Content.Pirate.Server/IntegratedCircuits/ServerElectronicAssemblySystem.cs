using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Tools.Systems;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Pirate.Shared.IntegratedCircuits.Events;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Server-side electronic assembly system that handles power management.
/// Drains idle power from the assembly's battery (inserted as a separate power cell
/// in the "cell_slot" container) for each installed circuit.
/// </summary>
public sealed class ServerElectronicAssemblySystem : SharedElectronicAssemblySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ElectronicAssemblyUISystem _ui = default!;

    private float _accumulatedFrameTime = 0f;
    private const float UpdateInterval = 1.0f; // Оновлюємо енергію РАЗ В СЕКУНДУ, а не 30 разів

    public override void Update(float frameTime)
    {
        _accumulatedFrameTime += frameTime;
        if (_accumulatedFrameTime < UpdateInterval)
            return;

        var query = EntityQueryEnumerator<ElectronicAssemblyComponent>();
        while (query.MoveNext(out var uid, out var assembly))
        {
            // Передаємо накопичений час (1 сек), а не мікро-фреймтайм
            DrainIdlePower(uid, assembly, _accumulatedFrameTime);
        }
        _accumulatedFrameTime = 0f;
    }

    public override void Initialize()
    {
        base.Initialize();

        // Підписуємося на клік предметом по корпусу
        SubscribeLocalEvent<ElectronicAssemblyComponent, InteractUsingEvent>(OnInteractUsing);
        
        // Підписуємося на завершення смужки прогресу (DoAfter)
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyTogglePanelEvent>(OnTogglePanel);
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyToggleWeldEvent>(OnToggleWeld);
    }

    /// <summary>
    /// Drains idle power from the assembly's battery for all installed circuits
    /// that have a non-zero idle power draw.
    /// </summary>
    private void DrainIdlePower(EntityUid assemblyUid, ElectronicAssemblyComponent assembly, float frameTime)
    {
        // Look for a battery in the assembly — either on the entity itself
        // or in a "cell_slot" container (insertable power cell).
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out var batteryUid))
            return;

        var totalIdleDraw = 0f;
        foreach (var circuitUid in assembly.CircuitEntities)
        {
            if (TryComp<IntegratedCircuitComponent>(circuitUid, out var circuit) && circuit.PowerDrawIdle > 0)
            {
                totalIdleDraw += circuit.PowerDrawIdle;
            }
        }

        if (totalIdleDraw <= 0)
            return;

        // Power is in watts, frameTime is in seconds, battery stores joules.
        var energyToUse = totalIdleDraw * frameTime;
        _battery.UseCharge(batteryUid.Value, energyToUse, battery);
    }

    /// <summary>
    /// Attempts to use a given amount of energy (in joules) from the assembly's battery.
    /// Finds the battery via cell_slot or directly on the entity.
    /// Returns true if the battery had sufficient charge and the energy was consumed.
    /// </summary>
    public bool TryUsePower(EntityUid assemblyUid, float amount)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out var batteryUid))
            return false;

        return _battery.TryUseCharge(batteryUid.Value, amount, battery);
    }

    /// <summary>
    /// Checks if the assembly has a battery with any remaining charge.
    /// </summary>
    public bool HasPower(EntityUid assemblyUid)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out _))
            return false;

        return battery.CurrentCharge > 0;
    }

    /// <summary>
    /// Checks if the assembly has enough charge for the given amount.
    /// </summary>
    public bool HasEnoughPower(EntityUid assemblyUid, float amount)
    {
        if (!_battery.TryGetBatteryComponent(assemblyUid, out var battery, out _))
            return false;

        return battery.CurrentCharge >= amount;
    }


private void OnInteractUsing(EntityUid uid, ElectronicAssemblyComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // 1. ПЕРЕВІРЯЄМО ЗВАРКУ
        if (_tool.HasQuality(args.Used, "Welding"))
        {
            // Логічно, що заварити можна тільки закритий корпус
            if (comp.Opened)
            {
                _popup.PopupEntity("Спочатку закрийте панель викруткою, щоб заварити корпус!", uid, args.User);
                return;
            }

            // Запускаємо процес заварювання (триває 2 секунди)
            _tool.UseTool(args.Used, args.User, uid, 2f, "Welding", new AssemblyToggleWeldEvent());
            args.Handled = true;
            return;
        }

        // 2. ПЕРЕВІРЯЄМО ВИКРУТКУ
        if (_tool.HasQuality(args.Used, "Screwing"))
        {
            // Якщо заварено - викрутка безсила
            if (comp.Welded)
            {
                _popup.PopupEntity("Корпус міцно заварений! Відкрутити гвинти неможливо.", uid, args.User);
                return;
            }

            // Запускаємо процес відкручування (триває 1 секунду)
            _tool.UseTool(args.Used, args.User, uid, 1f, "Screwing", new AssemblyTogglePanelEvent());
            args.Handled = true;
            return;
        }
    }

    /// <summary>
    /// Спрацьовує, коли смужка прогресу викрутки дійшла до кінця
    /// </summary>
    private void OnTogglePanel(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyTogglePanelEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        comp.Opened = !comp.Opened; // Міняємо стан на протилежний
        
        var text = comp.Opened ? "відкрутили гвинти і відкрили" : "закрили і закрутили гвинти";
        _popup.PopupEntity($"Ви {text} технічну панель.", uid, args.User);

        // Оновлюємо UI, щоб у гравців, які дивляться в інтерфейс, оновився стан
        _ui.UpdateUI(uid, comp);
        args.Handled = true;
    }

    /// <summary>
    /// Спрацьовує, коли смужка прогресу зварки дійшла до кінця
    /// </summary>
    private void OnToggleWeld(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyToggleWeldEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        comp.Welded = !comp.Welded; // Міняємо стан на протилежний

        var text = comp.Welded ? "наглухо заварили" : "розварили";
        _popup.PopupEntity($"Ви {text} корпус мікросхеми.", uid, args.User);

        args.Handled = true;
    }



}
