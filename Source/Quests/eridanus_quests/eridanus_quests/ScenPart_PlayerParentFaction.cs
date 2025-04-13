// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// RimWorld.ScenPart_PlayerFaction
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;


namespace eridanus_quests
{
	public class ScenPart_PlayerParentFaction : ScenPart
	{
		internal FactionDef factionDef;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref factionDef, "parentFactionDef");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && factionDef == null)
			{
				Randomize();
				Log.Error("ScenPart had null parent faction after loading. Changing to " + factionDef.ToStringSafe());
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), factionDef.LabelCap, drawBackground: true, doMouseoverSound: true, active: true, null))
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (FactionDef item in DefDatabase<FactionDef>.AllDefs.Where((FactionDef d) => !d.hidden))
			{
				FactionDef localFd = item;
				list.Add(new FloatMenuOption(localFd.LabelCap, delegate
				{
					factionDef = localFd;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PlayerParentFaction".Translate(factionDef.label);
		}

		public override void Randomize()
		{
			factionDef = DefDatabase<FactionDef>.AllDefs.Where((FactionDef fd) => fd.isPlayer).RandomElement();
		}

        public override void PreMapGenerate()
		{
            Current.Game.components.Add(new GameComponent_PlayerFaction(Current.Game));
        }

        public override void PostWorldGenerate()
		{
			Current.Game.GetComponent<GameComponent_PlayerFaction>().SetPlayerFaction(factionDef);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (factionDef == null)
			{
				yield return "factionDef is null";
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ ((factionDef != null) ? factionDef.GetHashCode() : 0);
		}
	}
}