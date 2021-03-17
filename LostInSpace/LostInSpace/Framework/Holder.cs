using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using HBS.Collections;

namespace LostInSpace.Framework
{
    public class Util
    {
        private static Util _instance;
        public Dictionary<string, List<string>> hiddenSystemsDict;


        public static Util HolderInstance
        {
            get
            {
                if (_instance == null) _instance = new Util();
                return _instance;
            }
        }

        internal void Initialize()
        {
            hiddenSystemsDict = LostInSpaceInit.modSettings.hiddenSystems;
            LostInSpaceInit.modLog.Debug?.Write(
                $"Initializing hiddenSystems from settings.");
        }

        public void RemoveSystemRestrictions(string systemID, List<string> tags = null) //systemID needs to have starsystemdef_// // tags can be null, which will clear all custom restrictions from the def//
        {
            if (!hiddenSystemsDict.ContainsKey(systemID))
            {
                LostInSpaceInit.modLog.Debug?.Write(
                    $"No system with id {systemID} found in hiddenSystemsDict, ignoring.");
                return;
            }
            var sim = UnityGameInstance.BattleTechGame.Simulation;
            var system = sim.GetSystemById(systemID);

            if (tags == null)
            {
                LostInSpaceInit.modLog.Debug?.Write(
                    $"Removing all custom travel restrictions from {systemID}.");

                foreach (var tag in new List<string>(hiddenSystemsDict[systemID]))
                {
                    var cReqs = system.Def.TravelRequirements.Where(x => x.Scope == EventScope.Company);
                    foreach (var cReq in cReqs)
                    {
                        cReq.ExclusionTags.Remove(tag);
                        cReq.RequirementTags.Remove(tag);
                    }
                }
                hiddenSystemsDict.Remove(systemID);
                return;
            }

            if (tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    LostInSpaceInit.modLog.Debug?.Write(
                        $"Removing travel restriction tag: {tag} from {systemID}.");

                    var cReqs = system.Def.TravelRequirements.Where(x => x.Scope == EventScope.Company);
                    foreach (var cReq in cReqs)
                    {
                        cReq.ExclusionTags.Remove(tag);
                        cReq.RequirementTags.Remove(tag);
                    }
                    hiddenSystemsDict[systemID].Remove(tag);
                }
                return;
            }

        }

        public void AddSystemRestriction(string systemID, string tagType, bool hidesSystem, List<string> tags) //systemID needs to have starsystemdef_ // tagType is either RequirementTags or ExclusionTags // hidesSystem = bool
        {
            var sim = UnityGameInstance.BattleTechGame.Simulation;
            var system = sim.GetSystemById(systemID);

            if (!hiddenSystemsDict.ContainsKey(systemID))
            {
                hiddenSystemsDict.Add(systemID, new List<string>());
            }

            foreach (var tag in tags)
            {
                string finalTag;
                if (hidesSystem) finalTag = tag + "_HIDDEN";
                
                if (tagType == "RequirementTags")
                {
                    finalTag = tag.Insert(0, "LiS_NavReq_");
                    hiddenSystemsDict[systemID].Add(finalTag);
                    LostInSpaceInit.modLog.Debug?.Write(
                        $"Added TravelRequirements to {system.Name}: Requirement Tag {finalTag}");
                }
                if (tagType=="ExclusionTags")
                {
                    finalTag = tag.Insert(0, "LiS_NavExc_");
                    hiddenSystemsDict[systemID].Add(finalTag);
                    LostInSpaceInit.modLog.Debug?.Write(
                        $"Added TravelRequirements to {system.Name}: Exclusion Tag {finalTag}");
                }
            }
            
        }
    }


}
