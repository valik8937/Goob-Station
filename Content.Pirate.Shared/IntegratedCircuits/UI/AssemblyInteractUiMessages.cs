using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits.UI;

[Serializable, NetSerializable]
public enum AssemblyInteractUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class InteractOption
{
    public NetEntity CircuitEntity;
    public string Name = string.Empty;
}

[Serializable, NetSerializable]
public sealed class AssemblyInteractBoundUIState : BoundUserInterfaceState
{
    public List<InteractOption> Options { get; }

    public AssemblyInteractBoundUIState(List<InteractOption> options)
    {
        Options = options;
    }
}

[Serializable, NetSerializable]
public sealed class AssemblyInteractSelectMessage : BoundUserInterfaceMessage
{
    public NetEntity SelectedCircuit { get; }

    public AssemblyInteractSelectMessage(NetEntity selectedCircuit)
    {
        SelectedCircuit = selectedCircuit;
    }
}
