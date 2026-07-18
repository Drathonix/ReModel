using HarmonyLib;
using PlayerModelLib;
using ReModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ReModel.Common.Patches.PlayerModelLib;

[HarmonyPatch(typeof(GuiDialogCreateCustomCharacter), "ChangeClass")]

public class PatchGuiDialogCreateCustomCharacter
{
    /// <summary>
    /// Prevents the client from changing their class while remodelling.
    /// Note that the serverside also blocks class changing unless the "allowcharselonce" permission is active.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
  
    public static bool Prefix(GuiDialogCreateCustomCharacter __instance, int dir)
    {
        IClientPlayer player = ReModelClient.capi.World.Player;
        if (!ReModelClient.DidSelect)
        {
            return true;
        }
        if (!player.Entity.WatchedAttributes.GetBool("allowcharselonce") && player.WorldData.CurrentGameMode != EnumGameMode.Creative)
        {
            return false;
        }
        return true;
    }
}

