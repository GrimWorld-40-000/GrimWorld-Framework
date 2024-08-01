using System.Collections.Generic;
using Verse;

namespace GrimworldItemLimit
{
    public class ItemsCraftedLibrary: GameComponent
    {
        
        public static ItemsCraftedLibrary GetCurrentLibrary()
        {
            if (Current.Game != InternalItemsCrafted.Game) _cachedItemsCrafted = null;
            return InternalItemsCrafted;
        }

        private static ItemsCraftedLibrary InternalItemsCrafted => Current.Game.GetComponent<ItemsCraftedLibrary>();
        private static ItemsCraftedLibrary _cachedItemsCrafted;
        
        private Dictionary<string, int> ItemsOfDefCrafted => _itemsOfDefCrafted ??= new Dictionary<string, int>();
        private Dictionary<string, int> _itemsOfDefCrafted;

        public Game Game;
        
        public ItemsCraftedLibrary(Game game)
        {
            Game = game;
        }

        public void Reset()
        {
            _itemsOfDefCrafted = null;
        }
        
        
        public int GetItemsOfDefCrafted(ThingDef thingDef)
        {
            return ItemsOfDefCrafted.GetValueOrDefault(thingDef.defName, 0);
        }

        public void NotifyLimitedItemCreated(ThingDef thingDef, int count = 1)
        {
            ItemsOfDefCrafted[thingDef.defName] = GetItemsOfDefCrafted(thingDef) + count;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _itemsOfDefCrafted, "itemsOfDefCrafted");
        }
    }
}