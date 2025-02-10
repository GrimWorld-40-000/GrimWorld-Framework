using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GW_Frame
{
    public class WorldComponent_StudyManager : WorldComponent
    {
        public IEnumerable<ResearchProjectDef> ResearchProjects { get; }
        
        private Dictionary<ResearchProjectDef, ScribeDictionary<ThingDef, bool>> _extraRequisitesTracker = new();
        
        public WorldComponent_StudyManager(World world) : base(world)
        {
            ResearchProjects = DefDatabase<ResearchProjectDef>.AllDefs
                .Where(p => p
                    .GetModExtension<DefModExtension_ExtraPrerequisiteActions>() != null);
        }
        
        private void AddCompletedRequirement(ResearchProjectDef project, ThingDef itemCompleted)
        {
            if (!_extraRequisitesTracker.ContainsKey(project))
            {
                ScribeDictionary<ThingDef, bool> dict =
                    new ScribeDictionary<ThingDef, bool>(LookMode.Def, LookMode.Value);
                
                _extraRequisitesTracker.Add(project, dict);
            }
            
            ScribeDictionary<ThingDef, bool> projectRequisites = _extraRequisitesTracker[project];
            if (projectRequisites.TryAdd(itemCompleted, true))
            {
                return;
            }
            
            projectRequisites[itemCompleted] = true;
        }
        
        public void CompleteAllRequirements(ResearchProjectDef project)
        {
            DefModExtension_ExtraPrerequisiteActions extraReqExtension = project
                .GetModExtension<DefModExtension_ExtraPrerequisiteActions>();

            foreach (StudyRequirement req in extraReqExtension.ItemStudyRequirements)
            {
                AddCompletedRequirement(project, req.StudyObject);
            }
        }
        
        public bool CompletedAllRequirements(ResearchProjectDef project)
        {
            DefModExtension_ExtraPrerequisiteActions extraReqExtension = project
                .GetModExtension<DefModExtension_ExtraPrerequisiteActions>();
            
            if (!_extraRequisitesTracker.TryGetValue(project, out ScribeDictionary<ThingDef, bool> tracked))
                return false;

            bool completed = true;
            foreach (StudyRequirement req in extraReqExtension.ItemStudyRequirements)
            {
                if (tracked.TryGetValue(req.StudyObject, out bool foundResult))
                    completed &= foundResult;
                else return false;
            }
            return completed;
        }
        
        public bool CompletedRequirement(ResearchProjectDef project, ThingDef itemCompleted)
        {
            if (!_extraRequisitesTracker.TryGetValue(project, 
                    out ScribeDictionary<ThingDef, bool> projectTracker))
                return false;
            
            return projectTracker.TryGetValue(itemCompleted, 
                out bool foundResult) && foundResult;
        }
        
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _extraRequisitesTracker, "ExtraRequisitesTracker", LookMode.Def, LookMode.Deep);
            base.ExposeData();
        }
    }
}