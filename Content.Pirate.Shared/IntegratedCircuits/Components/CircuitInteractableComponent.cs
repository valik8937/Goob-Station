using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.IntegratedCircuits.Components;

/// <summary>
/// Додайте цей компонент до мікросхем (кнопки, перемикачі, клавіатури), 
/// які гравець може активувати вручну, тримаючи корпус у руках.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CircuitInteractableComponent : Component
{
}
