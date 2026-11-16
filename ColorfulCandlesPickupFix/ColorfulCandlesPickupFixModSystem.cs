using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;

namespace ColorfulCandlesPickupFix;

public class ColorfulCandlesPickupFixModSystem : ModSystem
{
    private static readonly HashSet<ColorfulCandlesPickupFixModSystem> ActiveSystems = [];
    private static Harmony? harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        if (ActiveSystems.Contains(this)) return;

        // in case we run this mod in singleplayer, which shares the same assembly as the local server
        if (ActiveSystems.Count == 0)
        {
            var patcher = new Harmony("colorfulcandlespickupfix");
            patcher.PatchAll(typeof(CandlePickupPatch).Assembly);
            harmony = patcher;
        }

        ActiveSystems.Add(this);
    }

    public override void Dispose()
    {
        if (ActiveSystems.Remove(this) && ActiveSystems.Count == 0)
        {
            harmony?.UnpatchAll(harmony.Id);
            harmony = null;
        }

        base.Dispose();
    }
}