// SPDX-FileCopyrightText: 2026 CyberLanos <cyber.lanos00@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.NanoChat;

[Serializable, NetSerializable]
public sealed partial class PdaPhotoUploadDoAfterEvent(NetEntity cardUid, NanoChatPhotoData photo) : SimpleDoAfterEvent
{
    #region Pirate: freeze nano chat upload payload
    public NetEntity CardUid = cardUid; // Pirate: freeze nano chat upload target
    public NanoChatPhotoData Photo = photo; // Pirate: freeze nano chat uploaded photo

    public PdaPhotoUploadDoAfterEvent() : this(NetEntity.Invalid, default) // Pirate: serialization ctor for frozen upload payload
    {
    }

    public override DoAfterEvent Clone() => new PdaPhotoUploadDoAfterEvent(CardUid, Photo); // Pirate: preserve payload without aliasing do-after instances
    #endregion
}
