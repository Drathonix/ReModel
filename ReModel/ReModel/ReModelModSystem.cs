using HarmonyLib;
using ReModel.Client;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAllUncategorized(assembly);
            if (api.ModLoader.IsModEnabled("playermodellib"))
            {
                _harmony.PatchCategory(assembly,"playermodellib");
            }
            else
            {
                _harmony.PatchCategory(assembly,"not_playermodellib");
            }
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
