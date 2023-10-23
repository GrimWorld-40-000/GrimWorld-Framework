using System.Collections.Generic;
using Verse;

namespace GW_Frame
{
    public class DefModExtension_ExtraPrerequisiteActions : DefModExtension, IExposable
    {
        public ThingDef StudyLocation;
        public List<StudyRequirement> ItemStudyRequirements;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref StudyLocation, "StudyLocation");
            Scribe_Collections.Look(ref ItemStudyRequirements, "StudyRequirements", LookMode.Deep);
        }
    }
}