using HarmonyLib;
using ReModel.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
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
    public static bool PreOnCharacterSelection(CharacterSystem __instance, IServerPlayer fromPlayer, CharacterSelectionPacket p)
    {
        bool modData = fromPlayer.GetModData<bool>("createCharacter");
        bool allowChangeClass = !modData || fromPlayer.Entity.WatchedAttributes.GetBool("allowcharselonce") || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative;
        if (p.DidSelect)
        {
            fromPlayer.SetModData<bool>("createCharacter", true);
            bool hasChangedClass = (allowChangeClass || !modData) && (p.CharacterClass != fromPlayer.Entity.WatchedAttributes.GetString("characterClass"));
            if (allowChangeClass)
            {
                __instance.setCharacterClass(fromPlayer.Entity, p.CharacterClass, !modData || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative);
            }
            else 
            { 
                __instance.setCharacterClass(fromPlayer.Entity, fromPlayer.Entity.WatchedAttributes.GetString("characterClass"), !modData || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative);
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
            else if (hasChangedClass)
            {
                //Usually this is always removed but I'm preventing that from occuring if the class was unchanged.
                fromPlayer.Entity.WatchedAttributes.RemoveAttribute("allowcharselonce");
            }

            IServerNetworkChannel channel = ReModelModSystem.sapi.Network.GetChannel("charselection");
            CharacterSelectedState message = new()
            {
                DidSelect = true
            };
            channel.SendPacket<CharacterSelectedState>(message, [fromPlayer]);
        }
        fromPlayer.Entity.WatchedAttributes.MarkPathDirty("skinConfig");
        fromPlayer.BroadcastPlayerData(true);
        
        return false;
    }

    public static IEnumerable<CodeInstruction> OnCharSelCmdTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return Transpilers.MethodReplacer(instructions,
            typeof(TreeAttribute).GetMethod("GetBool"), typeof(PatchCharacterSystem).GetMethod("OverrideGetBool"));
    }

    /// <summary>
    /// Always allow class changing even without perms.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool OverrideGetBool(SyncedTreeAttribute __instance, string key, bool defaultValue)
    {
        return true;
    }

    public static void PostOnSelectedState(CharacterSystem __instance, CharacterSelectedState p)
    {
        ReModelClient.DidSelect = p.DidSelect;
    }
}
