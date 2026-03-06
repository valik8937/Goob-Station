// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Photo;

[Serializable, NetSerializable]
public sealed record PhotoCardUiState(
    byte[]? ImageData,
    string? CustomName,
    string? Caption) : BoundUserInterfaceState;

[Serializable, NetSerializable]
public enum PhotoCardUiKey : byte
{
    Key
}
