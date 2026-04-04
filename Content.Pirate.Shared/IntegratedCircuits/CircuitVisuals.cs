using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.IntegratedCircuits;

[Serializable, NetSerializable]
public enum AssemblyVisuals : byte
{
    Opened, // bool: чи відкрита кришка
    Color   // Color: колір маски
}

[Serializable, NetSerializable]
public enum WirerVisuals : byte
{
    Mode    // WirerMode: поточний режим паяльника
}
