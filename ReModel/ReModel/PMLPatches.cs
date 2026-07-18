using HarmonyLib;
using PlayerModelLib;
using ReModel.Common.Patches;
using ReModel.Common.Patches.PlayerModelLib;
using System;
using Vintagestory.GameContent;

namespace ReModel
{
    internal class PMLPatches
    {
        internal static void Run(Harmony _harmony)
        {
            _harmony.Patch(AccessTools.Method(typeof(CharacterSystem),"onCharacterSelection"), AccessTools.Method(typeof(PatchPMLCharacterSystem),"Prefix"));
            _harmony.Patch(AccessTools.Method(typeof(GuiDialogCreateCustomCharacter),"ChangeClass"), AccessTools.Method(typeof(PatchGuiDialogCreateCustomCharacter),"Prefix"));
        }
    }
}