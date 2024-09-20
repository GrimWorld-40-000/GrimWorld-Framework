using System.Linq;
using Verse;

namespace GrimworldItemLimit
{
    public static class ItemCraftingLimitExtensions
    {
        
        public static bool DoesRecipeHaveAnyProductThatIsAtLimit(this RecipeDef recipe)
        {
            return (from thingDefCountClass in recipe.products let comp = thingDefCountClass.thingDef.GetCompProperties<CompProperties_ItemCraftingLimit>() where comp != null where Comp_ItemCraftingLimit.IsEnabled(thingDefCountClass.thingDef) where ItemsCraftedLibrary.GetCurrentLibrary().GetItemsOfDefCrafted(thingDefCountClass.thingDef) + thingDefCountClass.count > comp.maxNumberCraftable select thingDefCountClass).Any();
        }
        
        
        public static void NotifyRecipeFinished(this RecipeDef recipe)
        {
            foreach (var thingDefCountClass in from thingDefCountClass in recipe.products let comp = thingDefCountClass.thingDef.GetCompProperties<CompProperties_ItemCraftingLimit>() where comp != null select thingDefCountClass)
            {
                ItemsCraftedLibrary.GetCurrentLibrary().NotifyLimitedItemCreated(thingDefCountClass.thingDef, thingDefCountClass.count);
            }
        }
    }
}