using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW40kHediffAura;

public class HediffComp_AuraAndMote : HediffComp
{
	public bool isToggleOn = true;

	private Mote mote;

	public HediffCompProperties_AuraAndMote Props => (HediffCompProperties_AuraAndMote)(object)base.props;

	public override void CompExposeData()
	{
		((HediffComp)this).CompExposeData();
		Scribe_Values.Look<bool>(ref isToggleOn, "isToggleOn", true, false);
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		((HediffComp)this).CompPostTick(ref severityAdjustment);
		if (((Thing)((HediffComp)this).Pawn).Map == null)
		{
			return;
		}
		if (isToggleOn && Gen.IsHashIntervalTick((Thing)(object)((HediffComp)this).Pawn, Props.tickInterval))
		{
			DoGiveHediff();
		}
		if (isToggleOn && Props.mote != null)
		{
			if (mote == null || ((Thing)mote).Destroyed)
			{
				mote = MoteMaker.MakeAttachedOverlay((Thing)(object)((Hediff)base.parent).pawn, Props.mote, Vector3.zero, 1f, -1f);
			}
			if (mote != null)
			{
				mote.Maintain();
			}
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmos()
	{
		Command_Toggle command_Toggle = new Command_Toggle();
		if (isToggleOn)
		{
			((Command)command_Toggle).defaultLabel = "Aura Enabled";
			((Command)command_Toggle).defaultDesc = "Aura is currently active";
			((Command)command_Toggle).icon = (Texture)(object)ContentFinder<Texture2D>.Get(Props.uiIconEnabled, true);
		}
		else
		{
			((Command)command_Toggle).defaultLabel = "Aura Disabled";
			((Command)command_Toggle).defaultDesc = "Aura is currently disabled";
			((Command)command_Toggle).icon = (Texture)(object)ContentFinder<Texture2D>.Get(Props.uiIconDisabled, true);
		}
		command_Toggle.isActive = () => isToggleOn;
		command_Toggle.toggleAction = delegate
		{
			isToggleOn = !isToggleOn;
		};
		yield return (Gizmo)(object)command_Toggle;
	}

	public void DoGiveHediff()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		foreach (Pawn item in GW40kUtility.GetNearbyPawnFriendAndFoe(((Thing)((HediffComp)this).Pawn).Position, ((Thing)((HediffComp)this).Pawn).Map, Props.radius))
		{
			if (ThingUtility.DestroyedOrNull((Thing)(object)item) || (!Props.affectWearer && item == ((HediffComp)this).Pawn))
			{
				continue;
			}
			if (Props.hostileHediff != null && (FactionUtility.HostileTo(((Thing)item).Faction, ((Thing)((HediffComp)this).Pawn).Faction) || GenHostility.HostileTo((Thing)(object)item, (Thing)(object)((HediffComp)this).Pawn) || GenHostility.HostileTo((Thing)(object)item, ((Thing)((HediffComp)this).Pawn).Faction)))
			{
				if (!item.health.hediffSet.HasHediff(Props.hostileHediff, false))
				{
					item.health.AddHediff(GW40kUtility.CreateHediff(Props.hostileHediff, item, Props.severityPerTrigger));
				}
				else
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(Props.hostileHediff, false);
					if (firstHediffOfDef != null)
					{
						firstHediffOfDef.Severity += Props.severityPerTrigger;
					}
				}
			}
			if (Props.allyOrNeutralHediff != null && (FactionUtility.AllyOrNeutralTo(((Thing)item).Faction, ((Thing)((HediffComp)this).Pawn).Faction) || WildManUtility.AnimalOrWildMan(item)) && ((Thing)item).Faction != ((Thing)((HediffComp)this).Pawn).Faction)
			{
				if (!item.health.hediffSet.HasHediff(Props.allyOrNeutralHediff, false))
				{
					item.health.AddHediff(GW40kUtility.CreateHediff(Props.allyOrNeutralHediff, item, Props.severityPerTrigger));
				}
				else
				{
					Hediff firstHediffOfDef2 = item.health.hediffSet.GetFirstHediffOfDef(Props.allyOrNeutralHediff, false);
					firstHediffOfDef2.Severity += Props.severityPerTrigger;
				}
			}
			if (Props.ownerFactionHediff != null && ((Thing)item).Faction == ((Thing)((HediffComp)this).Pawn).Faction)
			{
				if (!item.health.hediffSet.HasHediff(Props.ownerFactionHediff, false))
				{
					item.health.AddHediff(GW40kUtility.CreateHediff(Props.ownerFactionHediff, item, Props.severityPerTrigger));
					continue;
				}
				Hediff firstHediffOfDef3 = item.health.hediffSet.GetFirstHediffOfDef(Props.ownerFactionHediff, false);
				firstHediffOfDef3.Severity += Props.severityPerTrigger;
			}
		}
	}
}
