using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ColorfulCandlesPickupFix;

[HarmonyPatch(typeof(BlockBunchOCandles), nameof(BlockBunchOCandles.OnBlockInteractStart))]
internal static class CandlePickupPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getItem = AccessTools.Method(typeof(IWorldAccessor), nameof(IWorldAccessor.GetItem),
            [typeof(AssetLocation)]);
        var resolveCandle = AccessTools.Method(typeof(CandlePickupPatch), nameof(ResolveCandleItem));
        var patched = 0;

        foreach (var instruction in instructions)
            if (instruction.Calls(getItem))
            {
                // replace only hardcoded candle lookup
                yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction)
                    .MoveBlocksFrom(instruction);
                yield return new CodeInstruction(OpCodes.Call, resolveCandle);
                patched++;
            }
            else
            {
                yield return instruction;
            }

        if (patched != 1)
            throw new InvalidOperationException(
                $"Expected one candle lookup in BlockBunchOCandles pickup, found {patched}.");
    }

    private static Item? ResolveCandleItem(IWorldAccessor world, AssetLocation vanillaCode, Block block)
    {
        if (block.Code.Domain == "colorfulcandles" && block.Drops is { Length: > 0 }
                                                   && block.Drops[0]?.ResolvedItemstack?.Item is { } candle)
            return candle;

        return world.GetItem(vanillaCode);
    }
}