using Content.Pirate.Shared.IntegratedCircuits;
using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;
using System.Linq;

namespace Content.Pirate.Server.IntegratedCircuits;

public sealed class CircuitPrinterUISystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CircuitPrinterComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterCategoryMessage>(OnCategoryChanged);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterBuildMessage>(OnBuild);
    }

    /// <summary>
    /// Збирає всі існуючі рецепти з YAML файлів і групує їх по категоріям.
    /// </summary>
    private Dictionary<string, List<PrinterRecipeEntry>> GetCategoriesAndRecipes()
    {
        var dict = new Dictionary<string, List<PrinterRecipeEntry>>();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<CircuitRecipePrototype>())
        {
            if (!dict.ContainsKey(proto.Category))
                dict[proto.Category] = new List<PrinterRecipeEntry>();

            dict[proto.Category].Add(new PrinterRecipeEntry
            {
                RecipeId = proto.ID,
                Name = proto.Name,
                Description = proto.Description,
                Cost = new Dictionary<string, int>(proto.Cost),
                RequiresUpgrade = proto.RequiresUpgrade
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
        // 1. Шукаємо рецепт в IPrototypeManager за ID, який прислав клієнт
        if (!_prototypeManager.TryIndex<CircuitRecipePrototype>(msg.PrototypeId, out var recipe))
        {
            Log.Warning($"Спроба крафту неіснуючого рецепту: {msg.PrototypeId}");
            return;
        }

        // 2. Перевіряємо, чи існує сутність (Result), яку ми збираємося спавнити
        if (!_prototypeManager.HasIndex<EntityPrototype>(recipe.Result))
        {
            Log.Error($"Рецепт {recipe.ID} намагається створити неіснуючу сутність: {recipe.Result}!");
            return;
        }

        // 3. Перевірка апгрейдів
        if (recipe.RequiresUpgrade && !comp.Upgraded)
            return;

        // 4. Перевірка наявності матеріалів
        foreach (var entry in recipe.Cost)
        {
            var mat = entry.Key;
            var cost = entry.Value;
            if (!comp.Materials.TryGetValue(mat, out var amount) || amount < cost)
                return;
        }

        // 5. Віднімаємо матеріали
        foreach (var entry in recipe.Cost)
        {
            comp.Materials[entry.Key] -= entry.Value;
        }

        // 6. Спавнимо предмет
        var transform = Transform(uid);
        Spawn(recipe.Result, _transform.GetMapCoordinates(uid, transform));

        // Оновлюємо UI (щоб показати нові цифри металу)
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
            Categories = GetCategoriesAndRecipes(), // Відправляємо динамічний список
            CurrentCategory = comp.CurrentCategory ?? string.Empty
        };

        _ui.SetUiState(uid, CircuitPrinterUiKey.Key, state);
    }
}