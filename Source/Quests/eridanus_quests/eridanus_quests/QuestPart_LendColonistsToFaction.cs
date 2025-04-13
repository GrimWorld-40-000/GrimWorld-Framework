// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// RimWorld.QuestPart_LendColonistsToFaction
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace eridanus_quests
{
	public class QuestPart_LendAstartesToFaction : QuestPartActivable
	{
		public Thing shuttle;

		public Faction lendColonistsToFaction;

		public int returnLentColonistsInTicks = -1;

		public MapParent returnMap;

		public string outSignalColonistsDied;

		private int returnColonistsOnTick;

		private List<Thing> lentColonists = new List<Thing>();

		private List<int> lentColonistDeadTick = new List<int>(3);

		public List<Thing> LentColonistsListForReading => lentColonists;

		public int ReturnPawnsInDurationTicks => Mathf.Max(returnColonistsOnTick - GenTicks.TicksGame, 0);

		private int GenerateDeadTick()
		{
			return Rand.Range(600000, returnLentColonistsInTicks - 600000);
		}
		private int GetSurvivalAndDeadTick(Pawn pawn)
		{
			float score = 0;
			if (pawn.story.traits.HasTrait(TraitDefOf.Brawler) || pawn.skills.GetSkill(SkillDefOf.Melee).Level > pawn.skills.GetSkill(SkillDefOf.Shooting).Level)
			{
				score += pawn.skills.GetSkill(SkillDefOf.Melee).Level;
				score += pawn.skills.GetSkill(SkillDefOf.Shooting).Level / 2;
			}
			else
			{
				score += pawn.skills.GetSkill(SkillDefOf.Melee).Level / 2;
				score += pawn.skills.GetSkill(SkillDefOf.Shooting).Level;
			}
			score += pawn.skills.GetSkill(SkillDefOf.Medicine).Level / 4;
			score += pawn.skills.GetSkill(SkillDefOf.Intellectual).Level / 4;

			if (pawn.story.traits.HasTrait(TraitDef.Named("Tough")))
			{
				score += 1;
			}
			if (pawn.story.traits.HasTrait(TraitDef.Named("Nimble")))
			{
				score += 1;
			}
			if (pawn.story.traits.HasTrait(TraitDef.Named("Nerves")))
			{
				score += pawn.story.traits.GetTrait(TraitDef.Named("Nerves")).Degree;
			}

			if (score < 20)
			{
				if (Rand.Chance(score / 40))
				{
					return -1;
				}
			}
			else
			{
				if (score > 40) { score = 40; }
				if (Rand.Chance((score / 40) * 95))
				{
					return -1;
				}
			}

			return GenerateDeadTick();
		}

		private void LevelPawnSkills(Pawn pawn)
		{
			if (pawn.story.traits.HasTrait(TraitDefOf.Brawler) || pawn.skills.GetSkill(SkillDefOf.Melee).Level > pawn.skills.GetSkill(SkillDefOf.Shooting).Level)
			{
				pawn.skills.Learn(SkillDefOf.Melee, Rand.Range(30000, 100000), true, true);
				pawn.skills.Learn(SkillDefOf.Shooting, Rand.Range(10000, 30000), true, true);
				if (!pawn.story.traits.HasTrait(TraitDef.Named("Nimble")) && Rand.Chance(0.5f))
				{
					pawn.story.traits.GainTrait(new Trait(TraitDef.Named("Nimble")));
				}
			}
			else
			{
				pawn.skills.Learn(SkillDefOf.Melee, Rand.Range(10000, 30000), true, true);
				pawn.skills.Learn(SkillDefOf.Shooting, Rand.Range(30000, 100000), true, true);
			}

			// check to add other traits later
		}

		private void UpgradeEquipment(Pawn pawn)
		{
			//pawn.EquippedWornOrInventoryThings;

			//change armor to deathwatch shoulder pad + upgrade it (Keep chapter shoulder color+palette)
			//pawn.Tools
			//give sanctified version of current weapon
		}

		public override string DescriptionPart
		{
			get
			{
				if (State == QuestPartState.Disabled || lentColonists.Count == 0)
				{
					return null;
				}
				return "PawnsLent".Translate(lentColonists.Select((Thing t) => t.LabelShort).ToCommaList(useAnd: true), ReturnPawnsInDurationTicks.ToStringTicksToDays("0.0"));
			}
		}

		public override void Enable(SignalArgs receivedArgs)
		{
			base.Enable(receivedArgs);
			CompTransporter compTransporter = shuttle.TryGetComp<CompTransporter>();
			if (lendColonistsToFaction == null || compTransporter == null)
			{
				return;
			}
			foreach (Thing item in (IEnumerable<Thing>)compTransporter.innerContainer)
			{
				if (item is Pawn { IsFreeColonist: not false } pawn)
				{
					//change armor/weapons
					lentColonists.Add(pawn);

					// calculate survival and dead tick
					lentColonistDeadTick.Add(GetSurvivalAndDeadTick(pawn));
					UpgradeEquipment(pawn);
				}
			}
			returnColonistsOnTick = GenTicks.TicksGame + returnLentColonistsInTicks;
		}

		public override void QuestPartTick()
		{
			base.QuestPartTick();
			if (Find.TickManager.TicksGame >= enableTick + returnLentColonistsInTicks)
			{
				Complete();
			}
			else
			{
				for (int i = 0; i < lentColonistDeadTick.Count; i++)
				{
					if (lentColonistDeadTick[i] > 0 && Find.TickManager.TicksGame >= enableTick + lentColonistDeadTick[i])
					{
						if (lentColonists[i] is Pawn { IsFreeColonist: not false } pawn)
						{
							Notify_PawnKilled(pawn, new DamageInfo(HealthUtility.RandomViolenceDamageType(), 100));
							lentColonistDeadTick.RemoveAt(i);
							break;
						}
					}
				}
			}
		}

		public override void Complete(SignalArgs signalArgs)
		{
			Map map = ((returnMap == null) ? Find.AnyPlayerHomeMap : returnMap.Map);
			if (map != null)
			{
				foreach (Thing item in (IEnumerable<Thing>)lentColonists)
				{
					if (item is Pawn { IsFreeColonist: not false } pawn)
					{
						LevelPawnSkills(pawn);

						//Give relation boost
					}
				}

				base.Complete(new SignalArgs(new LookTargets(lentColonists).Named("SUBJECT"), lentColonists.Select((Thing c) => c.LabelShort).ToCommaList(useAnd: true).Named("PAWNS")));
				if (lendColonistsToFaction != null && lendColonistsToFaction == Faction.OfEmpire)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.Shuttle);
					thing.SetFaction(Faction.OfEmpire);
					TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, lentColonists, thing);
					transportShip.ArriveAt(DropCellFinder.GetBestShuttleLandingSpot(map, Faction.OfEmpire), map.Parent);
					transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
				}
				else
				{
					DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, lentColonists, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false, forbid: false);
				}
			}
		}

		private void ReturnDead(Corpse corpse)
		{
			Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
			if (anyPlayerHomeMap != null)
			{
				DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(anyPlayerHomeMap), anyPlayerHomeMap, Gen.YieldSingle(corpse), 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false, forbid: false);
			}
		}

		public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
			if (lentColonists.Contains(pawn))
			{
				Building_Grave assignedGrave = null;
				if (pawn.ownership != null)
				{
					assignedGrave = pawn.ownership.AssignedGrave;
				}
				Corpse corpse = pawn.MakeCorpse(assignedGrave, null);
				lentColonists.Remove(pawn);
				ReturnDead(corpse);
				if (!outSignalColonistsDied.NullOrEmpty() && lentColonists.Count == 0)
				{
					Find.SignalManager.SendSignal(new Signal(outSignalColonistsDied));
				}
			}
		}

		public override void DoDebugWindowContents(Rect innerRect, ref float curY)
		{
			if (State == QuestPartState.Enabled)
			{
				Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
				if (Widgets.ButtonText(rect, "End " + ToString(), drawBackground: true, doMouseoverSound: true, active: true, null))
				{
					Complete();
				}
				curY += rect.height + 4f;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_References.Look(ref lendColonistsToFaction, "lendColonistsToFaction");
			Scribe_Values.Look(ref returnLentColonistsInTicks, "returnLentColonistsInTicks", 0);
			Scribe_Values.Look(ref returnColonistsOnTick, "colonistsReturnOnTick", 0);
			Scribe_Collections.Look(ref lentColonists, "lentPawns", LookMode.Reference);
            Scribe_Collections.Look(ref lentColonistDeadTick, "lentPawnsDeadTicks", LookMode.Reference);
            Scribe_References.Look(ref returnMap, "returnMap");
			Scribe_Values.Look(ref outSignalColonistsDied, "outSignalColonistsDied");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				lentColonists.RemoveAll((Thing x) => x == null);
			}
		}
	}
}