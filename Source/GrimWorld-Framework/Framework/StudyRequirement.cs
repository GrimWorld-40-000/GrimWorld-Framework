using Verse;

namespace GW_Frame
{
    public class StudyRequirement : IExposable
    {
        public ThingDef StudyObject;
        public int NumberRequired;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref StudyObject, "StudyObject");
            Scribe_Values.Look(ref NumberRequired, "NumberRequired");
        }
    }
}
