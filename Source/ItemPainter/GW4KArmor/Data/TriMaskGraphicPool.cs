using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor.Data
{
    public static class TriMaskGraphicPool
    {
        private static readonly Dictionary<Color, List<ShaderParameter>> colorToParameter = new();
        
        public static T GraphicFromComp<T>(Comp_TriColorMask comp) where T : Graphic
        {
            GraphicData graphicData = comp.Def.graphicData;
            ApparelProperties apparel = comp.Def.apparel;
            string text = (apparel?.wornGraphicPath) ?? comp.Def.graphicData.texPath;
            bool flag = NeedsBodyType(comp.parent);
            
            if (flag)
            {
                BodyTypeDef bodyTypeDef = (comp.ParentHolder is Pawn pawn ? pawn.story.bodyType : null) ?? BodyTypeDefOf.Male;
                text = text + "_" + bodyTypeDef.defName;
            }

            Vector2 drawSize = graphicData.drawSize;
            string maskPath = text + $"_mask{comp.MaskIndex + 1}";
            return GetGraphic<T>(text, drawSize, comp.ColorOne, comp.ColorTwo,
                comp.ColorThree, graphicData, maskPath);
        }

        private static bool NeedsBodyType(Thing target)
        {
            if (target is not Apparel apparel)
            {
                return false;
            }

            bool flag2 = apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
                         apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover ||
                         (apparel.RenderAsPack()) ||
                         apparel.WornGraphicPath == BaseContent.PlaceholderImagePath ||
                         apparel.WornGraphicPath == BaseContent.PlaceholderGearImagePath;
            return !flag2;
        }

        private static T GetGraphic<T>(string texPath, Vector2 drawSize, Color? a, Color? b, Color? c, GraphicData gd,
            string maskPath) where T : Graphic
        {
            Color color = c ?? Color.white;
            var colorParameter = GetColorParameter(color);
            return GraphicDatabase.Get(typeof(T), texPath, Core.MaskShader, drawSize, a ?? Color.white, b ?? Color.white,
                gd, colorParameter, maskPath) as T;
        }

        private static List<ShaderParameter> GetColorParameter(in Color color)
        {
            bool flag = colorToParameter.TryGetValue(color, out List<ShaderParameter> list);
            List<ShaderParameter> result;
            
            if (flag)
            {
                result = list;
            }
            else
            {
                list =
                [
                    new ShaderParameter
                    {
                        name = "_ColorThree",
                        type = ShaderParameter.Type.Vector,
                        value = color
                    }
                ];
                colorToParameter.Add(color, list);
                result = list;
            }
            
            return result;
        }
    }
}