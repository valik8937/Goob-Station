using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Events;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Server.GameObjects; // Для UserInterfaceSystem
using Content.Pirate.Shared.IntegratedCircuits;

namespace Content.Pirate.Server.IntegratedCircuits;

public sealed class ServerCircuitContentSystem : EntitySystem
{
    [Dependency] private readonly SharedIntegratedCircuitSystem _circuits = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CircuitSpeakerComponent, CircuitActivatedEvent>(OnSpeakerActivated);
        
        // Підписуємось на натискання Z на корпусі
        SubscribeLocalEvent<ElectronicAssemblyComponent, UseInHandEvent>(OnAssemblyUsed);
        
        // Підписуємось на відповідь з UI (коли гравець вибрав деталь у списку)
        SubscribeLocalEvent<ElectronicAssemblyComponent, AssemblyInteractSelectMessage>(OnInteractSelect);

        // Підписуємось на саму кнопку (замість хардкоду в UseInHand)
        SubscribeLocalEvent<CircuitButtonComponent, CircuitManualTriggerEvent>(OnButtonTriggered);
    }

    private void OnSpeakerActivated(EntityUid uid, CircuitSpeakerComponent comp, CircuitActivatedEvent args)
    {
        var text = _circuits.ReadPinData(uid, PinType.Input, 0) as string;
        if (string.IsNullOrWhiteSpace(text)) return;
        var actingEntity = _circuits.GetActingEntity(uid);
        _popup.PopupEntity($"[Динамік]: {text}", actingEntity);
    }

    // ГОЛОВНА ЛОГІКА
    private void OnAssemblyUsed(EntityUid uid, ElectronicAssemblyComponent comp, UseInHandEvent args)
    {
        if (args.Handled || comp.Opened) 
            return; // Не можна клікати кнопки, поки корпус відкритий викруткою

        // Збираємо ВСІ інтерактивні мікросхеми в цьому корпусі
        var interactables = new List<EntityUid>();
        foreach (var circuitUid in comp.CircuitEntities)
        {
            if (HasComp<CircuitInteractableComponent>(circuitUid))
            {
                interactables.Add(circuitUid);
            }
        }

        if (interactables.Count == 0)
        {
            _popup.PopupEntity("В цьому пристрої немає нічого для натискання.", uid, args.User);
            args.Handled = true;
            return;
        }

        // ЯКЩО ДЕТАЛЬ ЛИШЕ ОДНА - натискаємо її миттєво
        if (interactables.Count == 1)
        {
            RaiseLocalEvent(interactables[0], new CircuitManualTriggerEvent(args.User));
            args.Handled = true;
            return;
        }

        // ЯКЩО ДЕТАЛЕЙ БІЛЬШЕ НІЖ 1 - формуємо меню
        var options = new List<InteractOption>();
        foreach (var circuitUid in interactables)
        {
            // Беремо кастомне ім'я, або стандартне, якщо гравець не перейменував
            var name = Name(circuitUid);
            if (TryComp<IntegratedCircuitComponent>(circuitUid, out var ic) && !string.IsNullOrEmpty(ic.DisplayName))
                name = ic.DisplayName;

            options.Add(new InteractOption
            {
                CircuitEntity = GetNetEntity(circuitUid),
                Name = name
            });
        }

        // Відправляємо стан і відкриваємо вікно
        var state = new AssemblyInteractBoundUIState(options);
        _uiSystem.SetUiState(uid, AssemblyInteractUiKey.Key, state);
        _uiSystem.OpenUi(uid, AssemblyInteractUiKey.Key, args.User);
        
        args.Handled = true;
    }

    // Обробка вибору гравця з Filtered List Menu
    private void OnInteractSelect(EntityUid uid, ElectronicAssemblyComponent comp, AssemblyInteractSelectMessage args)
    {
        var circuitUid = GetEntity(args.SelectedCircuit);

        // Захист від експлойтів: чи справді мікросхема все ще в цьому корпусі?
        if (!comp.CircuitEntities.Contains(circuitUid))
            return;

        // "Натискаємо" вибрану деталь
        RaiseLocalEvent(circuitUid, new CircuitManualTriggerEvent(args.Actor));

        // Можна закрити UI після вибору, або залишити відкритим для спаму (зазвичай закривають)
        // _uiSystem.CloseUi(uid, AssemblyInteractUiKey.Key, args.Actor); 
    }

    // Логіка самої кнопки (спрацьовує коли її вибрали в меню, або якщо вона одна)
    private void OnButtonTriggered(EntityUid uid, CircuitButtonComponent comp, CircuitManualTriggerEvent args)
    {
        // Кнопка просто пускає імпульс з її 0-го активатора
        _circuits.PushData(uid, PinType.Activator, 0);
        
        var actingEntity = _circuits.GetActingEntity(uid);
        _popup.PopupEntity("Ви натиснули на пристрій.", actingEntity, args.User);
    }
}