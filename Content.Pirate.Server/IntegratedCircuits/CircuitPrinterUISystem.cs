using Content.Pirate.Shared.IntegratedCircuits;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using System.Collections.Generic;
using System.Linq;

namespace Content.Pirate.Server.IntegratedCircuits;

public sealed class CircuitPrinterUISystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CircuitPrinterComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterCategoryMessage>(OnCategoryChanged);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterBuildMessage>(OnBuild);
    }

    /// <summary>
    /// Перебирає всі Entity у грі. Якщо на них є CircuitPrintableComponent - додає в меню принтера.
    /// </summary>
    private Dictionary<string, List<PrinterRecipeEntry>> GetCategoriesAndRecipes()
    {
        var dict = new Dictionary<string, List<PrinterRecipeEntry>>();

        // Скануємо всі сутності в грі
        foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            // Якщо на сутності немає нашого компонента для друку — пропускаємо
            if (!proto.TryGetComponent<CircuitPrintableComponent>(out var printable, _componentFactory))
                continue;

            if (!dict.ContainsKey(printable.Category))
                dict[printable.Category] = new List<PrinterRecipeEntry>();

            dict[printable.Category].Add(new PrinterRecipeEntry
            {
                RecipeId = proto.ID,
                Name = proto.Name ?? proto.ID,          // Беремо назву прямо з сутності
                Description = proto.Description ?? "",  // Беремо опис прямо з сутності
                Cost = new Dictionary<string, int>(printable.Cost),
                RequiresUpgrade = printable.RequiresUpgrade
            });
        }

        return dict;
    }

    private void OnUIOpened(EntityUid uid, CircuitPrinterComponent comp, BoundUIOpenedEvent args)
    {
        if (args.UiKey is not CircuitPrinterUiKey)
            return;

        var categories = GetCategoriesAndRecipes();

        if (string.IsNullOrEmpty(comp.CurrentCategory) && categories.Count > 0)
        {
            comp.CurrentCategory = categories.Keys.First();
        }

        UpdateUI(uid, comp);
    }

    private void OnCategoryChanged(EntityUid uid, CircuitPrinterComponent comp, CircuitPrinterCategoryMessage msg)
    {
        comp.CurrentCategory = msg.Category;
        UpdateUI(uid, comp);
    }

    private void OnBuild(EntityUid uid, CircuitPrinterComponent comp, CircuitPrinterBuildMessage msg)
    {
        // Шукаємо прототип сутності за ID
        if (!_prototypeManager.TryIndex<EntityPrototype>(msg.PrototypeId, out var proto))
            return;

        // Перевіряємо, чи її справді можна надрукувати
        if (!proto.TryGetComponent<CircuitPrintableComponent>(out var printable, _componentFactory))
            return;

        // Перевірка апгрейдів
        if (printable.RequiresUpgrade && !comp.Upgraded)
            return;

        // Перевірка наявності матеріалів
        foreach (var entry in printable.Cost)
        {
            if (!comp.Materials.TryGetValue(entry.Key, out var amount) || amount < entry.Value)
                return; // Не вистачає металу
        }

        // Віднімаємо матеріали
        foreach (var entry in printable.Cost)
        {
            comp.Materials[entry.Key] -= entry.Value;
        }

        // Спавнимо саму сутність
        var transform = Transform(uid);
        Spawn(proto.ID, _transform.GetMapCoordinates(uid, transform));

        UpdateUI(uid, comp);
    }

    public void UpdateUI(EntityUid uid, CircuitPrinterComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        var state = new CircuitPrinterBoundUIState
        {
            Materials = new Dictionary<string, int>(comp.Materials),
            MaterialMax = comp.MaterialMax,
            Upgraded = comp.Upgraded,
            CanClone = comp.CanClone,
            FastClone = comp.FastClone,
            Cloning = comp.Cloning,
            Categories = GetCategoriesAndRecipes(),
            CurrentCategory = comp.CurrentCategory ?? string.Empty
        };

        _ui.SetUiState(uid, CircuitPrinterUiKey.Key, state);
    }
}