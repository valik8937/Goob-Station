using Content.Pirate.Shared.IntegratedCircuits.Components;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits.UI;

[Serializable, NetSerializable]
public sealed class CircuitDebuggerBoundUIState : BoundUserInterfaceState
{
    public DebuggerMode Mode { get; }
    public string CurrentDataDisplay { get; }
    public bool AcceptingRefs { get; }

    public CircuitDebuggerBoundUIState(DebuggerMode mode, string currentDataDisplay, bool acceptingRefs)
    {
        Mode = mode;
        CurrentDataDisplay = currentDataDisplay;
        AcceptingRefs = acceptingRefs;
    }
}

[Serializable, NetSerializable]
public sealed class DebuggerSetModeMessage : BoundUserInterfaceMessage
{
    public DebuggerMode Mode { get; }
    public DebuggerSetModeMessage(DebuggerMode mode) => Mode = mode;
}

[Serializable, NetSerializable]
public sealed class DebuggerSetValueMessage : BoundUserInterfaceMessage
{
    public string Value { get; }
    public DebuggerSetValueMessage(string value) => Value = value;
}
