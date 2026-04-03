using Content.Pirate.Shared.IntegratedCircuits.Components;
using Content.Pirate.Shared.IntegratedCircuits.UI;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.IntegratedCircuits;

/// <summary>
/// Server-side system for handling Circuit Printer BUI messages.
/// Manages category switching, recipe building, material deduction, and UI state updates.
/// </summary>
public sealed class CircuitPrinterUISystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    // Hardcoded recipes for now. In a full implementation, this could be driven by a Prototype data definition (e.g., LatheRecipePrototype).
    // Key: Category Name, Value: List of recipes
    private readonly Dictionary<string, List<PrinterRecipeEntry>> _recipes = new()
    {
        {
            "Assemblies", new List<PrinterRecipeEntry>
            {
                new() { PrototypeId = "ElectronicAssemblySmall", Name = "Small Assembly", Description = "A small case for circuitry.", Cost = new() { { "Steel", 100 } } },
                new() { PrototypeId = "ElectronicAssemblyMedium", Name = "Medium Assembly", Description = "A medium case for circuitry.", Cost = new() { { "Steel", 250 } } },
                new() { PrototypeId = "ElectronicAssemblyLarge", Name = "Large Assembly", Description = "A large case for circuitry.", Cost = new() { { "Steel", 500 } } },
                new() { PrototypeId = "ElectronicAssemblyDrone", Name = "Drone Assembly", Description = "A mobile drone assembly.", Cost = new() { { "Steel", 400 } } },
                new() { PrototypeId = "ElectronicAssemblyWallmount", Name = "Wall-mounted Assembly", Description = "An assembly that mounts to a wall.", Cost = new() { { "Steel", 200 } } }
            }
        },
        {
            "Tools", new List<PrinterRecipeEntry>
            {
                new() { PrototypeId = "CircuitWirer", Name = "Circuit Wirer", Description = "Tool for wiring pins.", Cost = new() { { "Steel", 50 } } },
                new() { PrototypeId = "CircuitDebugger", Name = "Circuit Debugger", Description = "Tool for debugging pins.", Cost = new() { { "Steel", 50 } } }
            }
        }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CircuitPrinterComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterCategoryMessage>(OnCategoryChanged);
        SubscribeLocalEvent<CircuitPrinterComponent, CircuitPrinterBuildMessage>(OnBuild);
    }

    private void OnUIOpened(EntityUid uid, CircuitPrinterComponent comp, BoundUIOpenedEvent args)
    {
        if (args.UiKey is not CircuitPrinterUiKey)
            return;

        if (string.IsNullOrEmpty(comp.CurrentCategory) && _recipes.Count > 0)
        {
            // Select first category by default
            var enumerator = _recipes.Keys.GetEnumerator();
            if (enumerator.MoveNext())
                comp.CurrentCategory = enumerator.Current;
        }

        UpdateUI(uid, comp);
    }

    private void OnCategoryChanged(EntityUid uid, CircuitPrinterComponent comp, CircuitPrinterCategoryMessage msg)
    {
        if (!_recipes.ContainsKey(msg.Category))
            return;

        comp.CurrentCategory = msg.Category;
        UpdateUI(uid, comp);
    }

    private void OnBuild(EntityUid uid, CircuitPrinterComponent comp, CircuitPrinterBuildMessage msg)
    {
        // Find recipe
        PrinterRecipeEntry? recipeToBuild = null;
        foreach (var category in _recipes.Values)
        {
            foreach (var recipe in category)
            {
                if (recipe.PrototypeId == msg.PrototypeId)
                {
                    recipeToBuild = recipe;
                    break;
                }
            }
            if (recipeToBuild != null) break;
        }

        if (recipeToBuild == null)
            return;

        // Check upgrades
        if (recipeToBuild.RequiresUpgrade && !comp.Upgraded)
            return;

        // Check materials
        foreach (var (mat, cost) in recipeToBuild.Cost)
        {
            if (!comp.Materials.TryGetValue(mat, out var amount) || amount < cost)
                return;
        }

        // Deduct materials
        foreach (var (mat, cost) in recipeToBuild.Cost)
        {
            comp.Materials[mat] -= cost;
        }

        // Spawn item
        var transform = Transform(uid);
        Spawn(recipeToBuild.PrototypeId, _transform.GetMapCoordinates(uid, transform));

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
            Categories = _recipes,
            CurrentCategory = comp.CurrentCategory ?? string.Empty
        };

        _ui.SetUiState(uid, CircuitPrinterUiKey.Key, state);
    }
}
