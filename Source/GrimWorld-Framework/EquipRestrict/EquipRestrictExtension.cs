using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GW_Frame
{
    public class EquipRestrictExtension : DefModExtension
    {
        // Any variation of these can be attached to an item

        // Gene options
        public List<GeneDef> requiredGenesToEquip; // Require all of these on the pawn
        public List<GeneDef> requireOneOfGenesToEquip; // Require any one of these on the pawn
        public List<GeneDef> forbiddenGenesToEquip; // Require none of these are on the pawn

        // Xenotype options
        public List<XenotypeDef> requireOneOfXenotypeToEquip; // Require one of these xenotypes
        public List<XenotypeDef> forbiddenXenotypesToEquip; // Require pawn is not xenotype

        // Hediff options
        public List<HediffDef> requiredHediffsToEquip; // Require all of these on the pawn
        public List<HediffDef> requireOneOfHediffsToEquip; // Require any one of these on the pawn
        public List<HediffDef> forbiddenHediffsToEquip; // Require none of these are on the pawn
    }
}
