using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Tools.Systems;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Pirate.Shared.IntegratedCircuits.Events;
using Content.Pirate.Shared.IntegratedCircuits;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Robust.Server.GameObjects; // Потрібно для роботи з UI на сервері

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
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!; // Додано для закриття вікон
    [Dependency] private readonly Content.Shared.Containers.ItemSlots.ItemSlotsSystem _itemSlots = default!;

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

        // Слухаємо звичайний клік по об'єкту
        SubscribeLocalEvent<ElectronicAssemblyComponent, ActivateInWorldEvent>(OnActivate);
        
        // Підписуємося на ініціалізацію для встановлення початкових візуалів
        SubscribeLocalEvent<ElectronicAssemblyComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ElectronicAssemblyComponent comp, MapInitEvent args)
    {
        _appearance.SetData(uid, AssemblyVisuals.Opened, comp.Opened);
        _appearance.SetData(uid, AssemblyVisuals.Color, comp.DetailColor);

        // Блокуємо слот батареї, якщо корпус з'являється у світі закритим
        _itemSlots.SetLock(uid, "battery_slot", !comp.Opened);
    }

    private bool TryGetAssemblyBattery(EntityUid assemblyUid, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Content.Server.Power.Components.BatteryComponent? battery, out EntityUid batteryUid)
    {
        battery = null;
        batteryUid = default;
        var inserted = _itemSlots.GetItemOrNull(assemblyUid, "battery_slot");
        if (inserted != null && _battery.TryGetBatteryComponent(inserted.Value, out battery, out _))
        {
            batteryUid = inserted.Value;
            return true;
        }
        return false;
    }

    private void DrainIdlePower(EntityUid assemblyUid, ElectronicAssemblyComponent assembly, float frameTime)
    {
        if (!TryGetAssemblyBattery(assemblyUid, out var battery, out var batteryUid))
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

        var energyToUse = totalIdleDraw * frameTime;
        _battery.UseCharge(batteryUid, energyToUse, battery);
    }

    public bool TryUsePower(EntityUid assemblyUid, float amount)
    {
        if (!TryGetAssemblyBattery(assemblyUid, out var battery, out var batteryUid))
            return false;

        return _battery.TryUseCharge(batteryUid, amount, battery);
    }

    public bool HasPower(EntityUid assemblyUid)
    {
        if (!TryGetAssemblyBattery(assemblyUid, out var battery, out _))
            return false;

        return battery.CurrentCharge > 0;
    }

    public bool HasEnoughPower(EntityUid assemblyUid, float amount)
    {
        if (!TryGetAssemblyBattery(assemblyUid, out var battery, out _))
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
            if (comp.Opened)
            {
                _popup.PopupEntity("Спочатку закрийте панель викруткою, щоб заварити корпус!", uid, args.User);
                return;
            }

            _tool.UseTool(args.Used, args.User, uid, 2f, "Welding", new AssemblyToggleWeldEvent());
            args.Handled = true;
            return;
        }

        // 2. ПЕРЕВІРЯЄМО ВИКРУТКУ
        if (_tool.HasQuality(args.Used, "Screwing"))
        {
            if (comp.Welded)
            {
                _popup.PopupEntity("Корпус міцно заварений! Відкрутити гвинти неможливо.", uid, args.User);
                return;
            }

            _tool.UseTool(args.Used, args.User, uid, 1f, "Screwing", new AssemblyTogglePanelEvent());
            args.Handled = true;
            return;
        }

        // 3. ПЕРЕВІРЯЄМО, ЧИ ЦЕ МІКРОСХЕМА
        if (TryComp<IntegratedCircuitComponent>(args.Used, out var circuitComp))
        {
            if (TryAddCircuit(uid, args.Used, comp, circuitComp))
            {
                _popup.PopupEntity("Ви вставили мікросхему в корпус.", uid, args.User);
                args.Handled = true;
            }
            else
            {
                _popup.PopupEntity("Не вдалося! (Кришка закрита або немає місця)", uid, args.User);
            }
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

        comp.Opened = !comp.Opened;
        
        // Блокуємо слот, якщо закрито. Розблоковуємо, якщо відкрито.
        _itemSlots.SetLock(uid, "battery_slot", !comp.Opened);

        // ПОВІДОМЛЯЄМО КЛІЄНТУ ПРО ЗМІНУ (Синхронізація)
        Dirty(uid, comp); 

        var text = comp.Opened ? "відкрутили гвинти і відкрили" : "закрили і закрутили гвинти";
        _popup.PopupEntity($"Ви {text} технічну панель.", uid, args.User);

        _appearance.SetData(uid, AssemblyVisuals.Opened, comp.Opened);
        _ui.UpdateUI(uid, comp);
        args.Handled = true;
    }

    private void OnToggleWeld(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyToggleWeldEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        comp.Welded = !comp.Welded;
        
        // ПОВІДОМЛЯЄМО КЛІЄНТУ ПРО ЗМІНУ (Синхронізація)
        Dirty(uid, comp);

        var text = comp.Welded ? "наглухо заварили" : "розварили";
        _popup.PopupEntity($"Ви {text} корпус мікросхеми.", uid, args.User);

        args.Handled = true;
    }

    private void OnActivate(EntityUid uid, ElectronicAssemblyComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        // ПЕРЕВІРКА: Якщо закритий або заварений - нічого не відкриваємо
        // (Спливаюче повідомлення прибрано за вашим попереднім проханням щодо stealth)
        if (!comp.Opened)
        {
            args.Handled = true;
            return;
        }

        if (comp.Welded)
        {
            args.Handled = true;
            return;
        }

        // Якщо все ок - ВРУЧНУ відкриваємо UI на сервері
        // Клієнт отримає команду "відкрий вікно" і зробить це без жодних миготінь
        _uiSystem.OpenUi(uid, ElectronicAssemblyUiKey.Key, args.User);
        args.Handled = true;
    }
}