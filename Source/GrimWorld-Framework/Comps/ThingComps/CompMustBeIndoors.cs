using Verse;

namespace GW_Frame.Comps.ThingComps
{
	public class CompProperties_MustBeIndoors : CompProperties
	{
		public bool alertIfOutdoors = true;
		
		public CompProperties_MustBeIndoors()
		{
			compClass = typeof(CompMustBeIndoors);
		}
	}
	
	public class CompMustBeIndoors: ThingComp
	{
		protected CompProperties_MustBeIndoors Props => (CompProperties_MustBeIndoors) props;
		
		public bool ShouldAlertNow => !IsIndoors() && Props.alertIfOutdoors;
		
		protected bool IsIndoors()
		{
			return IsIndoors(out _);
		}
		
		private bool IsIndoors(out Room room)
		{
			room = parent.GetRoom();
			return room is { ProperRoom: true, OutdoorsForWork: false };
		}
	}
}