using System.Collections.Generic;
using Verse;

namespace GW_Frame
{
    public class DefModExtension_ExtraPrerequisiteActions : DefModExtension, IExposable
    {
        public ThingDef StudyLocation;
        public List<StudyRequirement> ItemStudyRequirements;
        public bool longLabel = false; // Set this to true if a two line label is causing display issues

        public void ExposeData()
        {
            Scribe_Defs.Look(ref StudyLocation, "StudyLocation");
            Scribe_Collections.Look(ref ItemStudyRequirements, "StudyRequirements", LookMode.Deep);
        }
    }
}