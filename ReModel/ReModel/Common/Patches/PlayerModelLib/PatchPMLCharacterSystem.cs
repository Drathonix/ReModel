using HarmonyLib;
using PlayerModelLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ReModel.Common.Patches.PlayerModelLib;

[HarmonyPatch(typeof(CharacterSystem), "onCharacterSelection"),HarmonyPriority(401)]
public class PatchPMLCharacterSystem
{
    public static bool Prefix(CharacterSystem __instance, IServerPlayer fromPlayer, CharacterSelectionPacket p)
    {
        bool didSelectBefore = fromPlayer.GetModData<bool>("createCharacter", false);

        bool allowChangeClass = !didSelectBefore || fromPlayer.Entity.WatchedAttributes.GetBool("allowcharselonce") || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative;

        if (p.DidSelect)
        {
            fromPlayer.SetModData<bool>("createCharacter", true);

            bool hasChangedClass = (allowChangeClass || !didSelectBefore) && (p.CharacterClass != fromPlayer.Entity.WatchedAttributes.GetString("characterClass"));

            if (allowChangeClass)
            {
                __instance.setCharacterClass(fromPlayer.Entity, p.CharacterClass, !didSelectBefore || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative);
            }
            else
            {
                __instance.setCharacterClass(fromPlayer.Entity, fromPlayer.Entity.WatchedAttributes.GetString("characterClass"), !didSelectBefore || fromPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative);
            }
            PlayerSkinBehavior? bh = fromPlayer.Entity.GetBehavior<PlayerSkinBehavior>();
            bh?.ApplyVoice(p.VoiceType, p.VoicePitch, false);

            foreach (KeyValuePair<string, string> skinpart in p.SkinParts)
            {
                bh?.SelectSkinPart(skinpart.Key, skinpart.Value, false);
            }

            DateTime date = DateTime.UtcNow;
            fromPlayer.ServerData.LastCharacterSelectionDate = date.ToShortDateString() + " " + date.ToShortTimeString();

            // allow players that just joined to immediately re select the class
            bool allowOneFreeClassChange = fromPlayer.Entity.Api.World.Config.GetBool("allowOneFreeClassChange");

            if (!didSelectBefore && allowOneFreeClassChange)
            {
                fromPlayer.ServerData.LastCharacterSelectionDate = null;
            }
            else if(hasChangedClass)
            {
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
}
