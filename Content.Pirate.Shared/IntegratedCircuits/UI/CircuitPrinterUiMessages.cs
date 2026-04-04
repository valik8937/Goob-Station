using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits.UI;

/// <summary>
/// A single recipe entry displayed in the printer UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PrinterRecipeEntry
{
    // ЗМІНЕНО З PrototypeId на RecipeId
    public string RecipeId { get; set; } = string.Empty; 
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, int> Cost { get; set; } = new();
    public bool RequiresUpgrade { get; set; }
}

/// <summary>
/// Full UI state for the circuit printer window.
/// </summary>
[Serializable, NetSerializable]
public sealed class CircuitPrinterBoundUIState : BoundUserInterfaceState
{
    /// <summary>
    /// Current material stocks (material name -> amount).
    /// </summary>
    public Dictionary<string, int> Materials { get; set; } = new();

    /// <summary>
    /// Maximum material capacity.
    /// </summary>
    public int MaterialMax { get; set; }

    /// <summary>
    /// Whether the printer has the advanced upgrade.
    /// </summary>
    public bool Upgraded { get; set; }

    /// <summary>
    /// Whether the printer can clone assemblies.
    /// </summary>
    public bool CanClone { get; set; }

    /// <summary>
    /// Whether cloning is instant (upgraded) or takes time.
    /// </summary>
    public bool FastClone { get; set; }

    /// <summary>
    /// Whether the printer is currently busy cloning.
    /// </summary>
    public bool Cloning { get; set; }

    /// <summary>
    /// Available recipe categories with their recipes.
    /// Key = category name, Value = list of recipes.
    /// </summary>
    public Dictionary<string, List<PrinterRecipeEntry>> Categories { get; set; } = new();

    /// <summary>
    /// Currently selected category name.
    /// </summary>
    public string CurrentCategory { get; set; } = string.Empty;
}

// ── Messages from client to server ──

/// <summary>
/// Request to build a specific recipe.
/// </summary>
[Serializable, NetSerializable]
public sealed class CircuitPrinterBuildMessage : BoundUserInterfaceMessage
{
    public string PrototypeId { get; }

    public CircuitPrinterBuildMessage(string prototypeId)
    {
        PrototypeId = prototypeId;
    }
}

/// <summary>
/// Request to change the currently viewed category.
/// </summary>
[Serializable, NetSerializable]
public sealed class CircuitPrinterCategoryMessage : BoundUserInterfaceMessage
{
    public string Category { get; }

    public CircuitPrinterCategoryMessage(string category)
    {
        Category = category;
    }
}
