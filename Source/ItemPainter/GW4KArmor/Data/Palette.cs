using System;
using GW4KArmor.UI;
using UnityEngine;
using Verse;

namespace GW4KArmor.Data
{
    public class Palette : IExposable
    {
        private static Color dummy;
        public bool canBeDeleted;
        public string name;
        public Color colorA;
        public Color colorB;
        public Color colorC;

        public override string ToString()
        {
            return name;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "customName");
            Scribe_Values.Look(ref canBeDeleted, "canBeDeleted");
            Scribe_Values.Look(ref colorA, "colA");
            Scribe_Values.Look(ref colorB, "colB");
            Scribe_Values.Look(ref colorC, "colC");
        }

        public ref Color GetColorForIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return ref colorA;
                case 1:
                    return ref colorB;
                case 2:
                    return ref colorC;
                default:
                    return ref dummy;
            }
        }

        public bool Draw(Rect rect, ref float height, Action delete = null)
        {
            Rect rect2 = rect;
            bool flag = this.name != null;
            if (flag)
            {
                height += 28f;

                using (new TempAnchor(TextAnchor.UpperLeft))
                {
                    Rect rect3 = rect;
                    rect3.x = rect.x + 2f;
                    Widgets.Label(rect3, this.name ?? "");
                    Rect rect4 = rect;
                    Color white = Color.white;
                    white.a = 0.1f;
                    Widgets.DrawBoxSolid(rect4, white);
                    rect.y += 28f;
                }
            }
            else
            {
                Rect rect5 = rect;
                Color white = Color.white;
                white.a = 0.1f;
                Widgets.DrawBoxSolid(rect5, white);
            }

            float num = rect.width / 3f;
            for (int i = 0; i < 3; i++)
            {
                Color solidColor = GetColorForIndex(i);
                Rect rect3 = rect;
                rect3.width = num;
                rect3.x = rect.x + num * (float)i;
                Rect rect6 = rect3;
                Widgets.DrawBoxSolidWithOutline(rect6, solidColor, Color.white, 1);
            }

            Widgets.DrawHighlightIfMouseover(rect);
            bool result = Widgets.ButtonInvisible(rect, true);
            bool flag2 = false;
            bool flag3 = this.canBeDeleted;
            if (flag3)
            {
                GUI.color = new Color(1f, 0.7f, 0.7f, 1f);
                flag2 = Widgets.ButtonText(rect2.RightPartPixels(100f).TopPartPixels(30f).ExpandedBy(0f, -2f),
                    "<color=white>DELETE</color>", true, true, true, null);
                GUI.color = Color.white;
            }

            bool flag4 = flag2 && delete != null;
            if (flag4)
            {
                delete();
            }

            return result;
        }
    }
}