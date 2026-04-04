using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using System.Collections.Generic;

namespace Content.Pirate.Shared.IntegratedCircuits;

/// <summary>
/// Прототип рецепту для принтера інтегральних схем.
/// Зберігається в YAML файлах.
/// </summary>
[Prototype("circuitRecipe")]
public sealed class CircuitRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// ID сутності, яка буде створена (заспавнена).
    /// </summary>
    [DataField("result", required: true)]
    public string Result { get; private set; } = string.Empty;

    /// <summary>
    /// Назва рецепту, яка відображається в UI.
    /// </summary>
    [DataField("name")]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Опис рецепту в UI.
    /// </summary>
    [DataField("description")]
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Категорія (вкладка) у вікні принтера (наприклад: "Assemblies", "Tools", "Logic").
    /// </summary>
    [DataField("category")]
    public string Category { get; private set; } = "Misc";

    /// <summary>
    /// Вартість крафту (Матеріал -> Кількість).
    /// </summary>
    [DataField("cost")]
    public Dictionary<string, int> Cost { get; private set; } = new();

    /// <summary>
    /// Чи потрібен апгрейд принтера для цього рецепту.
    /// </summary>
    [DataField("requiresUpgrade")]
    public bool RequiresUpgrade { get; private set; } = false;
}