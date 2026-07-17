using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ReModel.Common.Patches;

[HarmonyPatch(typeof(CharacterSystem), "onCharacterSelection")]
public class PatchCharacterSystem
{    
    /// <summary>
    /// Admittedly unsafe overwrite of this method. Will make more targetted if a conflict occurs in the future.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="fromPlayer"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static bool Prefix(CharacterSystem __instance, IServerPlayer fromPlayer, CharacterSelectionPacket p)
    {
        bool modData = fromPlayer.GetModData<bool>("createCharacter");
        bool allowChangeClass = fromPlayer.Entity.WatchedAttributes.GetBool("allowcharselonce") || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative;
        if (!modData || !allowChangeClass)
        {
            fromPlayer.Entity.WatchedAttributes.MarkPathDirty("skinConfig");
            fromPlayer.BroadcastPlayerData(true);
        }
        else
        {
            if (p.DidSelect)
            {
                fromPlayer.SetModData<bool>("createCharacter", true);
                bool hasChangedClass = (allowChangeClass || !modData) && (p.CharacterClass == fromPlayer.Entity.WatchedAttributes.GetString("characterClass"));
                if (allowChangeClass)
                {
                    __instance.setCharacterClass(fromPlayer.Entity, p.CharacterClass, !modData || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative);
                }
                EntityBehaviorExtraSkinnable behavior = fromPlayer.Entity.GetBehavior<EntityBehaviorExtraSkinnable>();
                behavior.ApplyVoice(p.VoiceType, p.VoicePitch, false);
                foreach (KeyValuePair<string, string> skinPart in p.SkinParts)
                    behavior.selectSkinPart(skinPart.Key, skinPart.Value, false);
                DateTime utcNow = DateTime.UtcNow;
                fromPlayer.ServerData.LastCharacterSelectionDate = $"{utcNow.ToShortDateString()} {utcNow.ToShortTimeString()}";
                bool flag = ReModelModSystem.sapi.World.Config.GetBool("allowOneFreeClassChange");
                if (!modData & flag)
                    fromPlayer.ServerData.LastCharacterSelectionDate = (string)null;
                else if (!hasChangedClass)
                {
                    //Usually this is always removed but I'm preventing that from occuring if the class was unchanged.
                    fromPlayer.Entity.WatchedAttributes.RemoveAttribute("allowcharselonce");
                }
            }
            fromPlayer.Entity.WatchedAttributes.MarkPathDirty("skinConfig");
            fromPlayer.BroadcastPlayerData(true);
        }
        return false;
    }

}
