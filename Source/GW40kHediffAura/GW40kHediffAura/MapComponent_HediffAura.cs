using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GW40kHediffAura;

public class MapComponent_HediffAura : MapComponent
{
	private const int ScanIntervalTicks = 60;

	private readonly Dictionary<int, int> lastAuraTickBySource = new();

	public MapComponent_HediffAura(Map map) : base(map)
	{
	}

	public override void MapComponentTick()
	{
		base.MapComponentTick();
		if (Find.TickManager.TicksGame % ScanIntervalTicks != 0)
		{
			return;
		}

		foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
		{
			if (pawn?.health?.hediffSet?.hediffs == null || pawn.Dead || pawn.Destroyed)
			{
				continue;
			}

			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				HediffComp_AuraAndMote aura = hediff.TryGetComp<HediffComp_AuraAndMote>();
				if (aura?.Props == null || !aura.isToggleOn)
				{
					continue;
				}

				int tickInterval = aura.Props.tickInterval > 0 ? aura.Props.tickInterval : 250;
				int sourceKey = Gen.HashCombineInt(pawn.thingIDNumber, hediff.def.shortHash);
				if (lastAuraTickBySource.TryGetValue(sourceKey, out int lastTick) && Find.TickManager.TicksGame - lastTick < tickInterval)
				{
					continue;
				}

				lastAuraTickBySource[sourceKey] = Find.TickManager.TicksGame;
				ApplyAura(pawn, aura.Props);
			}
		}
	}

	private void ApplyAura(Pawn source, HediffCompProperties_AuraAndMote props)
	{
		if (source?.Map == null)
		{
			return;
		}

		foreach (Pawn target in GW40kUtility.GetNearbyPawnFriendAndFoe(source.Position, source.Map, props.radius))
		{
			if (target?.health?.hediffSet == null || target.Dead || target.Destroyed)
			{
				continue;
			}

			if (!props.affectWearer && target == source)
			{
				continue;
			}

			if (props.hostileHediff != null && IsHostile(target, source))
			{
				AddOrRefreshHediff(target, props.hostileHediff, props.severityPerTrigger);
			}

			if (props.allyOrNeutralHediff != null && IsAllyOrNeutralNonOwner(target, source))
			{
				AddOrRefreshHediff(target, props.allyOrNeutralHediff, props.severityPerTrigger);
			}

			if (props.ownerFactionHediff != null && source.Faction != null && target.Faction == source.Faction)
			{
				AddOrRefreshHediff(target, props.ownerFactionHediff, props.severityPerTrigger);
			}
		}
	}

	private static void AddOrRefreshHediff(Pawn target, HediffDef hediffDef, float severityPerTrigger)
	{
		if (target?.health?.hediffSet == null || hediffDef == null)
		{
			return;
		}

		Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(hediffDef, false);
		if (hediff == null)
		{
			float initialSeverity = severityPerTrigger > 0f ? severityPerTrigger : hediffDef.initialSeverity;
			target.health.AddHediff(GW40kUtility.CreateHediff(hediffDef, target, initialSeverity));
			return;
		}

		hediff.Severity = Math.Min(hediff.Severity + severityPerTrigger, hediff.def.maxSeverity);
	}

	private static bool IsHostile(Pawn target, Pawn source)
	{
		return target.Faction != null && source.Faction != null && FactionUtility.HostileTo(target.Faction, source.Faction)
			|| GenHostility.HostileTo((Thing)(object)target, (Thing)(object)source)
			|| source.Faction != null && GenHostility.HostileTo((Thing)(object)target, source.Faction);
	}

	private static bool IsAllyOrNeutralNonOwner(Pawn target, Pawn source)
	{
		return target.Faction != source.Faction
			&& (target.Faction != null && source.Faction != null && FactionUtility.AllyOrNeutralTo(target.Faction, source.Faction)
				|| WildManUtility.AnimalOrWildMan(target));
	}
}
