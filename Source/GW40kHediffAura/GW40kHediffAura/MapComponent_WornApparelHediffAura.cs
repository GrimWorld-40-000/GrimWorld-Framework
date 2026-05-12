using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GW40kHediffAura;

public class MapComponent_WornApparelHediffAura : MapComponent
{
	private const int ScanIntervalTicks = 60;

	private readonly Dictionary<int, int> lastAuraTickByThingId = new();

	private readonly Dictionary<WearerHediffKey, ActiveWearerHediff> activeWearerHediffs = new();

	public MapComponent_WornApparelHediffAura(Map map) : base(map)
	{
	}

	public override void MapComponentTick()
	{
		base.MapComponentTick();
		if (Find.TickManager.TicksGame % ScanIntervalTicks != 0)
		{
			return;
		}

		HashSet<WearerHediffKey> seenWearerHediffs = new();
		foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
		{
			if (pawn?.apparel?.WornApparel == null || pawn.Dead || pawn.Destroyed)
			{
				continue;
			}

			foreach (Apparel apparel in pawn.apparel.WornApparel)
			{
				Comp_WornApparelHediffAura aura = apparel.TryGetComp<Comp_WornApparelHediffAura>();
				if (aura?.Props == null)
				{
					continue;
				}

				if (aura.Props.wearerHediff != null)
				{
					WearerHediffKey key = new(pawn.thingIDNumber, aura.Props.wearerHediff.shortHash);
					seenWearerHediffs.Add(key);
					activeWearerHediffs[key] = new ActiveWearerHediff(pawn, aura.Props.wearerHediff);
					EnsureWearerHediff(pawn, aura.Props.wearerHediff);
				}

				int tickInterval = aura.Props.tickInterval > 0 ? aura.Props.tickInterval : 250;
				int thingId = apparel.thingIDNumber;
				if (lastAuraTickByThingId.TryGetValue(thingId, out int lastTick) && Find.TickManager.TicksGame - lastTick < tickInterval)
				{
					continue;
				}

				lastAuraTickByThingId[thingId] = Find.TickManager.TicksGame;
				ApplyAura(pawn, aura.Props);
			}
		}

		RemoveInactiveWearerHediffs(seenWearerHediffs);
	}

	private void ApplyAura(Pawn source, CompProperties_WornApparelHediffAura props)
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

	private void RemoveInactiveWearerHediffs(HashSet<WearerHediffKey> seenWearerHediffs)
	{
		List<WearerHediffKey> inactiveKeys = null;
		foreach (KeyValuePair<WearerHediffKey, ActiveWearerHediff> entry in activeWearerHediffs)
		{
			if (seenWearerHediffs.Contains(entry.Key))
			{
				continue;
			}

			inactiveKeys ??= new List<WearerHediffKey>();
			inactiveKeys.Add(entry.Key);
			RemoveWearerHediff(entry.Value.Pawn, entry.Value.HediffDef);
		}

		if (inactiveKeys == null)
		{
			return;
		}

		foreach (WearerHediffKey key in inactiveKeys)
		{
			activeWearerHediffs.Remove(key);
		}
	}

	private static void EnsureWearerHediff(Pawn pawn, HediffDef hediffDef)
	{
		if (pawn?.health?.hediffSet == null || hediffDef == null || pawn.health.hediffSet.HasHediff(hediffDef, false))
		{
			return;
		}

		float initialSeverity = hediffDef.initialSeverity > 0f ? hediffDef.initialSeverity : 1f;
		pawn.health.AddHediff(GW40kUtility.CreateHediff(hediffDef, pawn, Math.Min(initialSeverity, hediffDef.maxSeverity)));
	}

	private static void RemoveWearerHediff(Pawn pawn, HediffDef hediffDef)
	{
		if (pawn?.health?.hediffSet == null || hediffDef == null)
		{
			return;
		}

		Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef, false);
		if (hediff != null)
		{
			pawn.health.RemoveHediff(hediff);
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

internal readonly struct WearerHediffKey : IEquatable<WearerHediffKey>
{
	private readonly int pawnThingId;

	private readonly ushort hediffShortHash;

	public WearerHediffKey(int pawnThingId, ushort hediffShortHash)
	{
		this.pawnThingId = pawnThingId;
		this.hediffShortHash = hediffShortHash;
	}

	public bool Equals(WearerHediffKey other)
	{
		return pawnThingId == other.pawnThingId && hediffShortHash == other.hediffShortHash;
	}

	public override bool Equals(object obj)
	{
		return obj is WearerHediffKey other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(pawnThingId, hediffShortHash);
	}
}

internal readonly struct ActiveWearerHediff
{
	public readonly Pawn Pawn;

	public readonly HediffDef HediffDef;

	public ActiveWearerHediff(Pawn pawn, HediffDef hediffDef)
	{
		Pawn = pawn;
		HediffDef = hediffDef;
	}
}
