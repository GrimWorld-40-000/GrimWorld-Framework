using GW4KArmor.Data;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor.Patches
{
    internal static class HarmonyPatches
    {


        [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
        public static class ApparelGraphicRecordGetter_TryGetGraphicApparel
        {
            public static void Postfix(Apparel apparel, bool __result, ref ApparelGraphicRecord rec)
            {
                var flag = !__result;
                if (!flag)
                {
                    var triColorMaskComp = apparel?.GetComp<Comp_TriColorMask>();
                    var flag2 = triColorMaskComp == null;
                    if (!flag2) rec.graphic = TriMaskGraphicPool.GraphicFromComp<Graphic_TriColorMask>(triColorMaskComp);
                }
            }
        }

        [HarmonyPatch(typeof(GraphicData), "GraphicColoredFor")]
        public static class GraphicData_GraphicColoredFor
        {
            // Token: 0x06000069 RID: 105 RVA: 0x00004454 File Offset: 0x00002654
            public static bool Prefix(Thing t, ref Graphic __result)
            {

                if (!(t is ThingWithComps thingWithComps))
                {
                    return true;
                }

                var comp = thingWithComps.GetComp<Comp_TriColorMask>();
                if (comp == null)
                {
                    return true;
                }

                Graphic graphic;
                if (t is Apparel)
                    graphic = TriMaskGraphicPool.GraphicFromComp<Graphic_TriColorMask>(comp);
                else
                    graphic = TriMaskGraphicPool.GraphicFromComp<Graphic_TriColorMask_Single>(comp);
                __result = graphic;
                return false;
            }
        }

        [HarmonyPatch(typeof(ShaderUtility), "SupportsMaskTex")]
        public static class ShaderUtility_SupportsMaskTex
        {
            public static void Postfix(Shader shader, ref bool __result)
            {
                if (shader == Core.MaskShader)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(ThingIDMaker), "GiveIDTo")]
        public static class ThingIDPatch
        {
            [HarmonyPriority(800)]
            private static bool Prefix(Thing t)
            {
                var flag = active == 0;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    t.thingIDNumber = 69420;
                    result = false;
                }

                return result;
            }

            private static int active;

            public readonly ref struct Scope
            {
                public Scope(bool uselessFlag = true)
                {
                    active++;
                }

                public void Dispose()
                {
                    var flag = active > 0;
                    if (flag) active--;
                }
            }
        }
    }
}