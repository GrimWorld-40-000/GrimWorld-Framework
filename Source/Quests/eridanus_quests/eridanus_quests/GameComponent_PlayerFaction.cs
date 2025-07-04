// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// Verse.GameComponent_DebugTools
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace eridanus_quests
{
	public class GameComponent_PlayerFaction : GameComponent
	{
		public FactionDef playerFactionIs = null;
		//subfaction? = null;

		public GameComponent_PlayerFaction(Game game)
		{
        }

		public void SetPlayerFaction(FactionDef inFaction)
		{
			playerFactionIs = inFaction;
		}

		public bool CheckPlayerFaction(FactionDef inFaction)
		{
            if (playerFactionIs != null && playerFactionIs.Equals(inFaction))
			{
				return true;
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref playerFactionIs, "playerFactionIs");
		}
	}
}