using HarmonyLib;
using LivelyHair.Framework.Managers;
using LivelyHair.Framework.Patches.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace LivelyHair
{
    public class LivelyHair : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static TextureManager textureManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            textureManager = new TextureManager(monitor, modHelper);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply hair related patches
                new FarmerRendererPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Game1.player.modData["LivelyHair.CustomHair.Id"] = "1";
            Game1.player.modData["LivelyHair.Animation.HasAnimation"] = true.ToString();
        }
    }
}
