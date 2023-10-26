using UnityEngine;
using Verse;

namespace GW4KArmor;

[StaticConstructorOnStartup]
internal static class PaintContent
{
    public static Texture2D PaintIcon = ContentFinder<Texture2D>.Get("UI/Icon_PaintTool", true);
    public static Texture2D PaintIconMulti = ContentFinder<Texture2D>.Get("UI/Icon_PaintToolMulti", true);
}