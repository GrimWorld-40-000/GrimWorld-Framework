// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// RimWorld.QuestGen.QuestNode_GetFaction
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace eridanus_quests
{
	// Checks if player is or isn't a membner/sub-member of a faction
	public class QuestNode_GWFaction : QuestNode
	{

		public bool inList;

        public List<FactionDef> factionList;

        public override bool TestRunInt(Slate slate)
		{
            if (Current.Game.GetComponent<GameComponent_PlayerFaction>().playerFactionIs != null) {
				foreach (var faction in factionList)
				{
					if (Current.Game.GetComponent<GameComponent_PlayerFaction>().CheckPlayerFaction(faction))
					{
						if (inList)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
				}
			}else if (!inList)
			{
				return true;
			}
			return false;
		}

		public override void RunInt()
		{
			Slate slate = QuestGen.slate;

		}
	}
}
