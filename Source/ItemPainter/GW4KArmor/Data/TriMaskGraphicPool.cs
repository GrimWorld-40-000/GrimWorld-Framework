using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor.Data
{
    public static class TriMaskGraphicPool
    {
        private static readonly Dictionary<Color, List<ShaderParameter>> colorToParameter = new Dictionary<Color, List<ShaderParameter>>();

        public static T GraphicFromComp<T>(Comp_TriColorMask comp) where T : Graphic
        {
            GraphicData graphicData = comp.Def.graphicData;
            ApparelProperties apparel = comp.Def.apparel;
            var text = (apparel != null ? apparel.wornGraphicPath : null) ?? comp.Def.graphicData.texPath;
            var flag = NeedsBodyType(comp.parent);
            if (flag)
            {
                var pawn = comp.ParentHolder as Pawn;
                var bodyTypeDef = (pawn != null ? pawn.story.bodyType : null) ?? BodyTypeDefOf.Male;
                text = text + "_" + bodyTypeDef.defName;
            }

            var drawSize = graphicData.drawSize;
            var maskPath = text + string.Format("_mask{0}", comp.MaskIndex + 1);
            return GetGraphic<T>(text, drawSize, comp.ColorOne, comp.ColorTwo,
                comp.ColorThree, graphicData, maskPath);
        }

        private static bool NeedsBodyType(Thing target)
        {
            if (!(target is Apparel apparel))
            {
                return false;
            }

            var flag2 = apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
                        apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover ||
                        PawnRenderer.RenderAsPack(apparel) ||
                        apparel.WornGraphicPath == BaseContent.PlaceholderImagePath ||
                        apparel.WornGraphicPath == BaseContent.PlaceholderGearImagePath;
            return !flag2;
        }

        public static T GetGraphic<T>(string texPath, Vector2 drawSize, Color? a, Color? b, Color? c, GraphicData gd,
            string maskPath) where T : Graphic
        {
            var color = c ?? Color.white;
            var colorParameter = GetColorParameter(color);
            return GraphicDatabase.Get(typeof(T), texPath, Core.MaskShader, drawSize, a ?? Color.white, b ?? Color.white,
                gd, colorParameter, maskPath) as T;
        }

        private static List<ShaderParameter> GetColorParameter(in Color color)
        {
            List<ShaderParameter> list;
            var flag = colorToParameter.TryGetValue(color, out list);
            List<ShaderParameter> result;
            if (flag)
            {
                result = list;
            }
            else
            {
                list = new List<ShaderParameter>(1)
                {
                    new ()
                    {
                        name = "_ColorThree",
                        type = ShaderParameter.Type.Vector,
                        value = color
                    }
                };
                colorToParameter.Add(color, list);
                result = list;
            }

            return result;
        }
    }
}
