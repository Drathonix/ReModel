using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ReModel.Client.Patches;

[HarmonyPatch(typeof(CharacterSystem), "onCharSelCmd")]

public class PatchCharacterSystemClient
{
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return Transpilers.MethodReplacer(instructions,
            typeof(TreeAttribute).GetMethod("GetBool"), typeof(PatchCharacterSystemClient).GetMethod("OverrideGetBool"));
    }

    /// <summary>
    /// Always allow class changing even without perms.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool OverrideGetBool(SyncedTreeAttribute __instance, string key)
    {
        Console.WriteLine("Called");
        return true;
    }
}
