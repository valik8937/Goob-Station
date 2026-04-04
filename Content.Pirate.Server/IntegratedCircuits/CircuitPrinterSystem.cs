using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.GameObjects;
using System;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Серверна система принтера. Обробляє фізичну взаємодію з принтером 
/// (вставку матеріалів, дисків оновлення тощо).
/// </summary>
public sealed class CircuitPrinterSystem : EntitySystem
{
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CircuitPrinterUISystem _ui = default!;

    // Скільки "одиниць" матеріалу дає один лист сталі. 
    // За твоїми коментарями: 1 лист = 100 одиниць матеріалу.
    private const int UnitsPerSheet = 100;

    public override void Initialize()
    {
        base.Initialize();

        // Підписуємося на івент кліку предметом по принтеру
        SubscribeLocalEvent<CircuitPrinterComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, CircuitPrinterComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Перевіряємо, чи предмет, яким клікають, є стаком (купою матеріалів)
        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        // В SS14 матеріали мають StackTypeId. Наприклад, "Steel", "Glass" тощо.
        // Перевіряємо, чи підтримує наш принтер цей матеріал.
        var materialName = GetMaterialNameFromStack(stack.StackTypeId);

        if (materialName == null || !comp.Materials.ContainsKey(materialName))
        {
            _popup.PopupEntity("Принтер не приймає цей матеріал!", uid, args.User);
            return;
        }

        // Рахуємо, скільки одиниць матеріалу ще може вмістити принтер
        var currentAmount = comp.Materials[materialName];
        var spaceLeft = comp.MaterialMax - currentAmount;

        if (spaceLeft <= 0)
        {
            _popup.PopupEntity("Принтер вже повністю заповнений цим матеріалом!", uid, args.User);
            args.Handled = true;
            return;
        }

        // Рахуємо, скільки листів ми можемо забрати з руки
        var maxSheetsToInsert = spaceLeft / UnitsPerSheet;
        var sheetsToTake = Math.Min(stack.Count, maxSheetsToInsert);

        if (sheetsToTake <= 0)
        {
            _popup.PopupEntity("Тут недостатньо місця навіть для одного листа!", uid, args.User);
            args.Handled = true;
            return;
        }

        // Віднімаємо листи з руки гравця
        _stack.SetCount(args.Used, stack.Count - sheetsToTake);

        // Додаємо матеріал у принтер
        comp.Materials[materialName] += (sheetsToTake * UnitsPerSheet);

        _popup.PopupEntity($"Ви завантажили {sheetsToTake} листів матеріалу {materialName}.", uid, args.User);

        // Оновлюємо UI, щоб гравець одразу побачив нові цифри
        _ui.UpdateUI(uid, comp);

        // Позначаємо івент як оброблений, щоб гравець не вдарив принтер сталлю
        args.Handled = true;
    }

    /// <summary>
    /// Мапить ідентифікатор стаку SS14 у назву матеріалу для нашого словника.
    /// Залежно від вашої кодової бази, ID можуть відрізнятися (наприклад "Steel" чи "Metal").
    /// </summary>
    private string? GetMaterialNameFromStack(string stackTypeId)
    {
        return stackTypeId switch
        {
            "Steel" => "Steel", // Звичайна сталь в SS14
            "Glass" => "Glass", // Скло
            "Plastic" => "Plastic",
            _ => null
        };
    }
}