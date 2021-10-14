using Exiled.API.Features;
using System;

namespace MiniGames
{
    public class MainClass : Plugin<MinigameConfig>
    {
        public EventManager emanager;
        public override string Author { get; } = "Killers0992";
        public override string Prefix { get; } = "minigames";
        public override string Name { get; } = "Minigames";

        public override void OnDisabled()
        {
            Log.Info("Plugin MiniGames disabled.");
        }

        private HarmonyLib.Harmony harmony;

        public override void OnEnabled()
        {
            try
            {
                harmony = new HarmonyLib.Harmony($"minigames.{DateTime.Now.Ticks}");
                harmony.PatchAll();
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }

            emanager = new EventManager(this);
        }
    }
}
