using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ReModel.Client.Patches;

[HarmonyPatch(typeof(GuiDialogCreateCharacter), "changeClass")]

public class PatchGuiDialogCreateCharacter
{
    /// <summary>
    /// Prevents the client from changing their class while remodelling.
    /// Note that the serverside also blocks class changing unless the "allowcharselonce" permission is active.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
  
    internal static bool Prefix(GuiDialogCreateCharacter __instance, int dir)
    {
        IClientPlayer player = ReModelClient.capi.World.Player;
        if (player != null && player.Entity.WatchedAttributes.GetBool("allowcharselonce") && player.WorldData.CurrentGameMode != EnumGameMode.Creative)
        {
            return false;
        }
        return true;
    }
}

