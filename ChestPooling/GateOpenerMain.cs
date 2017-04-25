﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace GateOpener
{
    public class GateOpenerMainClass : Mod
    {
        /*********
        ** Properties
        *********/
        private readonly Dictionary<Vector2, Fence> OpenGates = new Dictionary<Vector2, Fence>();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameEvents.FourthUpdateTick += this.GameEvents_FourthUpdateTick;
        }

        private void MyLog(String theString)
        {
#if DEBUG
            Monitor.Log(theString);
#endif
        }


        /*********
        ** Private methods
        *********/
        private void DebugThing(object theObject, string descriptor = "")
        {
            String thing = JsonConvert.SerializeObject(theObject, Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            File.WriteAllText("debug.json", thing);
            Console.WriteLine(descriptor + "\n" + thing);
        }

        private Fence GetGate(BuildableGameLocation location, Vector2 pos)
        {
            if (!location.objects.TryGetValue(pos, out StardewValley.Object obj))
                return null;

            if (obj is Fence fence && fence.isGate && !this.OpenGates.ContainsKey(pos))
            {
                this.OpenGates[pos] = fence;
                return fence;
            }
            return null;
        }

        private Fence LookAround(BuildableGameLocation location, List<Vector2> list)
        {
            foreach (Vector2 pos in list)
            {
                Fence gate = this.GetGate(location, pos);
                if (gate != null)
                    return gate;
            }
            return null;
        }

        private void GameEvents_FourthUpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation is BuildableGameLocation location)
            {
                List<Vector2> adj = Game1.player.getAdjacentTiles();
                Fence gate = this.LookAround(location, adj);
                if (gate != null)
                {
                    //MyLog(gate.ToString());
                    gate.gatePosition = 88;
                    Game1.playSound("doorClose");
                }

                //need to close it now...
                foreach (KeyValuePair<Vector2, Fence> gateObj in OpenGates)
                {
                    if (Game1.player.getTileLocation() != gateObj.Key && !adj.Contains(gateObj.Key))
                    {
                        gateObj.Value.gatePosition = 0;
                        Game1.playSound("doorClose");
                        OpenGates.Remove(gateObj.Key);
                        break;
                    }
                }
            }
        }
    }
}
