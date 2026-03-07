// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._Pirate.Photo;

[RegisterComponent]
public sealed partial class PhotoCameraUserComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> AlertPrototype = "PhotoCameraUsed";
}
