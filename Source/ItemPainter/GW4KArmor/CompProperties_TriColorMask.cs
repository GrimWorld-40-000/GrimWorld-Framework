using System.Collections.Generic;
using GW4KArmor.Data;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace GW4KArmor
{
    public class CompProperties_TriColorMask : CompProperties
    {
        public PaletteDef palettePresets;
        public Palette defaultPalette;

        //TODO: discover masks auomatically
        public int maskCount;
        private MaskTextureStorage _masks;

        public MaskTextureStorage Masks
        {
            get
            {
                return _masks ??= MaskTextureStorage.GetOrCreate(TexPath);
            }
        }

        public ThingDef Def { get; internal set; }

        public string TexPath
        {
            get
            {
                var apparel = Def.apparel;
                return apparel?.wornGraphicPath ?? Def.graphicData.texPath;
            }
        }

        public CompProperties_TriColorMask()
        {
            this.compClass = typeof(Comp_TriColorMask);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;
            if (maskCount <= 0)
                yield return "<maskCount> must be at least 1!";
        }
    }
}