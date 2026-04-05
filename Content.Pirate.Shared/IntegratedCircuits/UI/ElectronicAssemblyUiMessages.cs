using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using Content.Pirate.Shared.IntegratedCircuits;

namespace Content.Pirate.Shared.IntegratedCircuits.UI;

[Serializable, NetSerializable]
public sealed class CircuitPinInfo
{
    public string Name { get; set; } = string.Empty;
    public PinType PinType { get; set; }
    public PinDataType DataType { get; set; }
    public string DisplayData { get; set; } = "(null)";
    public bool IsConnected { get; set; }
    public string ConnectedTo { get; set; } = string.Empty;
}

[Serializable, NetSerializable]
public sealed class AssemblyCircuitEntry
{
    public NetEntity NetEntity { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Removable { get; set; } = true;
    public int Complexity { get; set; }
    public int Size { get; set; }
    public float PowerDrawIdle { get; set; }
    public float PowerDrawPerUse { get; set; }

    public List<CircuitPinInfo> InputPins { get; set; } = new();
    public List<CircuitPinInfo> OutputPins { get; set; } = new();
    public List<CircuitPinInfo> ActivatorPins { get; set; } = new();
}

[Serializable, NetSerializable]
public sealed class ElectronicAssemblyBoundUIState : BoundUserInterfaceState
{
    public bool IsOpened { get; set; }
    public int TotalSize { get; set; }
    public int MaxSize { get; set; }
    public int TotalComplexity { get; set; }
    public int MaxComplexity { get; set; }
    public bool HasBattery { get; set; }
    public float BatteryCharge { get; set; }
    public float BatteryMaxCharge { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    
    public List<AssemblyCircuitEntry> Circuits { get; set; } = new();
}

[Serializable, NetSerializable]
public sealed class AssemblyPinClickMessage : BoundUserInterfaceMessage
{
    public NetEntity CircuitEntity { get; }
    public PinType PinType { get; }
    public int PinIndex { get; }

    public AssemblyPinClickMessage(NetEntity circuitEntity, PinType pinType, int pinIndex)
    {
        CircuitEntity = circuitEntity;
        PinType = pinType;
        PinIndex = pinIndex;
    }
}

[Serializable, NetSerializable]
public sealed class AssemblyRemoveCircuitMessage : BoundUserInterfaceMessage
{
    public NetEntity CircuitEntity { get; }

    public AssemblyRemoveCircuitMessage(NetEntity circuitEntity)
    {
        CircuitEntity = circuitEntity;
    }
}

[Serializable, NetSerializable]
public sealed class AssemblyRenameMessage : BoundUserInterfaceMessage
{
    public string NewName { get; }

    public AssemblyRenameMessage(string newName)
    {
        NewName = newName;
    }
}

[Serializable, NetSerializable]
public sealed class AssemblyRemoveBatteryMessage : BoundUserInterfaceMessage
{
}

