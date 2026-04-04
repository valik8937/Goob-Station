using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.Events;
using Content.Pirate.Shared.IntegratedCircuits.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Content.Pirate.Shared.IntegratedCircuits;

namespace Content.Pirate.Server.IntegratedCircuits;

public sealed class ServerCircuitContentSystem : EntitySystem
{
    [Dependency] private readonly SharedIntegratedCircuitSystem _circuits = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // 1. Коли динамік отримує імпульс (активується)
        SubscribeLocalEvent<CircuitSpeakerComponent, CircuitActivatedEvent>(OnSpeakerActivated);

        // 2. Коли гравець "використовує" зібраний корпус в руці (натискає Z)
        SubscribeLocalEvent<ElectronicAssemblyComponent, UseInHandEvent>(OnAssemblyUsed);
    }

    private void OnSpeakerActivated(EntityUid uid, CircuitSpeakerComponent comp, CircuitActivatedEvent args)
    {
        // Читаємо текст із першого вхідного піна (індекс 0)
        var text = _circuits.ReadPinData(uid, PinType.Input, 0) as string;

        if (string.IsNullOrWhiteSpace(text))
            return;

        // Шукаємо сутність, яка повинна "говорити" (Корпус або сама мікросхема, якщо вона лежить на підлозі)
        var actingEntity = _circuits.GetActingEntity(uid);

        // Виводимо повідомлення над корпусом (поки що через Popup, потім можна через ChatSystem)
        _popup.PopupEntity($"[Динамік]: {text}", actingEntity);
    }

    private void OnAssemblyUsed(EntityUid uid, ElectronicAssemblyComponent comp, UseInHandEvent args)
    {
        if (args.Handled || comp.Opened) 
            return; // Не можна клікати кнопки, поки корпус відкритий

        bool pressed = false;

        // Шукаємо всі мікросхеми-кнопки всередині корпусу
        foreach (var circuitUid in comp.CircuitEntities)
        {
            if (HasComp<CircuitButtonComponent>(circuitUid))
            {
                // Кнопка має вихідний імпульсний пін (Activator, індекс 0). 
                // Ми "штовхаємо" з нього імпульс далі по дротах.
                _circuits.PushData(circuitUid, PinType.Activator, 0);
                pressed = true;
            }
        }

        if (pressed)
        {
            _popup.PopupEntity("Ви натиснули на пристрій.", uid, args.User);
            args.Handled = true;
        }
    }
}