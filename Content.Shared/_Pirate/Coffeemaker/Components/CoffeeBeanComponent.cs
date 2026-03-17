using Content.Shared.Chemistry.Reagent;

namespace Content.Shared._Pirate.Coffeemaker.Components;

[RegisterComponent]
public sealed partial class CoffeeBeanComponent : Component
{
    [DataField]
    public List<ReagentQuantity> ExtraReagents = new();
}
