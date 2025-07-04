using System.Collections.Generic;
using GW4KArmor.Data;
using Verse;

namespace GW4KArmor
{
    public class PaletteStorage : GameComponent
    {
        public List<Palette> Palettes => _backField;
        
        private List<Palette> _backField;
        
        public static PaletteStorage Current => Verse.Current.Game.GetComponent<PaletteStorage>();
        
        public PaletteStorage(Game _)
        {
            _backField = [];
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Collections.Look(ref _backField, "gw4k_palettes", LookMode.Deep, []);
            
            if (Palettes == null)
            {
                _backField = [];
            }
        }
    }
}