using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BattleTech;
using HBS.Collections;

namespace LostInSpace.Framework
{
    public static class Util
    {
        public static MethodInfo WIIC_Cleanup = null;
        public static void FinishedLoading(List<string> loadOrder)
        {
            detectWIIC();
        }

        public static void detectWIIC()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("WarTechIIC"))
                {
                    Type helperType = assembly.GetType("WarTechIIC.WIIC");
                    if (helperType != null) {
                        WIIC_Cleanup = helperType.GetMethod("cleanupSystem", BindingFlags.Static | BindingFlags.Public);
                    }
                    LostInSpaceInit.modLog?.Info?.Write(
                        $"WarTechIIC detected");
                }
            }
        }

        private static Regex StarSystemTravel_Restrict =
            new Regex("^LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$",
                RegexOptions.Compiled); //shamelessly stolen from BlueWinds

        public static void RemoveSystemRestrictions(string systemID, List<string> tags = null) //systemID needs to have starsystemdef_// // tags can be null, which will clear all custom restrictions from the def//
        {

            var sim = UnityGameInstance.BattleTechGame.Simulation;
            var system = sim.GetSystemById(systemID);

            if (tags == null)
            {
                LostInSpaceInit.modLog?.Info?.Write(
                    $"RemoveSystemRestrictions: Removing all custom travel restrictions from {systemID}.");

                foreach (var tag in new List<string>(system.Tags))
                {
                    MatchCollection matches2 = StarSystemTravel_Restrict.Matches(tag); 
                    if (matches2.Count > 0)
                    {
                        var type = matches2[0].Groups["type"].Value;
                        system.Tags.Remove(tag);

                        if (type == "NavReq")
                        {
                            var reqDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                RequirementTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) continue;
                            else companyReq.RequirementTags.Remove(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"RemoveSystemRestrictions: Removed TravelRequirements from {system.Name}: Requirement Tag {tag}");
                            continue;
                        }

                        if (type == "NavExc")
                        {
                            var excDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                ExclusionTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) continue;
                            else companyReq.ExclusionTags.Remove(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"RemoveSystemRestrictions: Removed TravelRequirements from {system.Name}: Exclusion Tag {tag}");
                            continue;
                        }
                    }
                    system.Tags.Remove(tag);
                    LostInSpaceInit.modLog?.Info?.Write($"RemoveSystemRestrictions: Removed tag: {tag} from {system.Name}.");
                }
                return;
            }

            if (tags.Count > 0)
            {
                foreach (var tag in tags)
                { 
                    LostInSpaceInit.modLog?.Info?.Write(
                        $"RemoveSystemRestrictions: Removing custom travel restriction: {tag} from {systemID}.");
                    MatchCollection matches2 = StarSystemTravel_Restrict.Matches(tag); 
                    if (matches2.Count > 0)
                    {
                        var type = matches2[0].Groups["type"].Value;
                        system.Tags.Remove(tag);

                        if (type == "NavReq")
                        {
                            var reqDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                RequirementTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) continue;
                            else companyReq.RequirementTags.Remove(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"RemoveSystemRestrictions: Removed TravelRequirements from {system.Name}: Requirement Tag {tag}");
                            continue;
                        }

                        if (type == "NavExc")
                        {
                            var excDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                ExclusionTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) continue;
                            else companyReq.ExclusionTags.Remove(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"RemoveSystemRestrictions: Removed TravelRequirements from {system.Name}: Exclusion Tag {tag}");
                            continue;
                        }
                    }
                    system.Tags.Remove(tag);
                    LostInSpaceInit.modLog?.Info?.Write($"RemoveSystemRestrictions: Removed tag: {tag} from {system.Name}.");
                }
            }
        }

        public static void AddSystemRestrictions(string systemID, List<string> tags) //systemID needs to have starsystemdef_
        {
            var sim = UnityGameInstance.BattleTechGame.Simulation;
            var system = sim.GetSystemById(systemID);

            foreach (var tag in tags)
            {
                LostInSpaceInit.modLog?.Info?.Write($"AddSystemRestrictions: Adding tag: {tag} to {system.Name}.");
                system.Tags.Add(tag);

                {
                    MatchCollection matches = StarSystemTravel_Restrict.Matches(tag);
                    if (matches.Count > 0)
                    {
                        var type = matches[0].Groups["type"].Value;

                        if (type == "NavReq")
                        {
                            var reqDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                RequirementTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) system.Def.TravelRequirements.Add(reqDef);
                            else companyReq.RequirementTags.Add(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"AddSystemRestrictions: Added TravelRequirements to {system.Name}: Requirement Tag {tag}");
                            continue;
                        }

                        if (type == "NavExc")
                        {
                            var excDef = new RequirementDef()
                            {
                                Scope = EventScope.Company,
                                ExclusionTags = new TagSet(tag)
                            };

                            var companyReq =
                                system.Def.TravelRequirements.FirstOrDefault(x =>
                                    x.Scope == EventScope.Company);
                            if (companyReq == null) system.Def.TravelRequirements.Add(excDef);
                            else companyReq.ExclusionTags.Add(tag);
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"AddSystemRestrictions: Added TravelRequirements to {system.Name}: Exclusion Tag {tag}");
                            continue;
                        }
                    }
                }
            }
        }
    }
}
