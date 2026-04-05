using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using System.Collections.Generic;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Якщо цей компонент є на EntityPrototype, принтер мікросхем зможе його надрукувати.
/// </summary>
[RegisterComponent]
public sealed partial class CircuitPrintableComponent : Component
{
    /// <summary>
    /// Категорія у вікні принтера (Assemblies, I/O, Tools тощо)
    /// </summary>
    [DataField("category")]
    public string Category = "Misc";

    /// <summary>
    /// Вартість у матеріалах
    /// </summary>
    [DataField("cost")]
    public Dictionary<string, int> Cost = new();

    /// <summary>
    /// Чи потрібен в принтері диск апгрейду для крафту
    /// </summary>
    [DataField("requiresUpgrade")]
    public bool RequiresUpgrade = false;
}
