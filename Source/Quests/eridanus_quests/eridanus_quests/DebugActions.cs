using System.Collections.Generic;
using System;
using LudeonTK;
using RimWorld;
using Verse;
using System.Linq;

namespace eridanus_quests
{
    public class DebugActions
    {
        [DebugAction("Grimworld Framework", "Set player parent faction", allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 5)]
        public static void ChangeParent()
        {
            if (Current.Game.GetComponent<GameComponent_PlayerFaction>().playerFactionIs == null)
            {
                Current.Game.components.Add(new GameComponent_PlayerFaction(Current.Game));
            }

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (Faction item2 in Find.FactionManager.AllFactions)
            {
                Faction localFac = item2;
                FloatMenuOption item = new FloatMenuOption(string.Concat(localFac), delegate
                {
                    Current.Game.GetComponent<GameComponent_PlayerFaction>().SetPlayerFaction(localFac.def);
                    Log.Message("Player parent faction switched to: " + localFac.Name);
                });
                list.Add(item);
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}