using System;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<TimeSpan> GhostRespawnDelay =
        CVarDef.Create("ghost.respawn_delay", TimeSpan.FromMinutes(10), CVar.SERVER | CVar.ARCHIVE);
}
