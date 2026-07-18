using HarmonyLib;
using ReModel.Client;
using ReModel.Common.Patches;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ReModel
{
    public class ReModelModSystem : ModSystem
    {

        private const string HarmonyId = "remodel";

        private Harmony _harmony;
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            _harmony = new Harmony(HarmonyId);
            if (api.ModLoader.IsModEnabled("playermodellib"))
            {
                PMLPatches.Run(_harmony);
            }
            else
            {
                _harmony.Patch(AccessTools.Method(typeof(CharacterSystem),"onCharacterSelection"), AccessTools.Method(typeof(PatchCharacterSystem),"PreOnCharacterSelection"));
                _harmony.Patch(AccessTools.Method(typeof(GuiDialogCreateCharacter),"changeClass"),AccessTools.Method(typeof(PatchGuiDialogCreateCharacter),"Prefix"));
            }
            _harmony.Patch(AccessTools.Method(typeof(CharacterSystem),"onCharSelCmd"),null,null,AccessTools.Method(typeof(PatchCharacterSystem),"OnCharSelCmdTranspiler"));
            _harmony.Patch(AccessTools.Method(typeof(CharacterSystem),"onSelectedState"),null,AccessTools.Method(typeof(PatchCharacterSystem), "PostOnSelectedState"));
            Mod.Logger.Notification("Remodel running on " + api.Side);
        }

        public static ICoreServerAPI? sapi;
        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ReModelClient.Init(api);
        }

        public override void Dispose()
        {
            _harmony?.UnpatchAll(HarmonyId);
        }
    }
}
