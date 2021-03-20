using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BattleTech;
using BattleTech.Save;
using BattleTech.UI;
using Harmony;
using HBS.Collections;
using LostInSpace.Framework;
using UnityEngine;

namespace LostInSpace.Patches
{
    class SGS_Patches
    {
        [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        public static class SimGameState_Rehydrate_Patch
        {

            
            private static Regex StarSystemTravel_Restrict =
                new Regex("^LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$",
                    RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            public static void Postfix(SimGameState __instance)
            {
                foreach (var system in LostInSpaceInit.modSettings.hiddenSystems)
                {
                    var starsystem = __instance.GetSystemById(system.Key);
                    if (starsystem == null)
                    {
                        LostInSpaceInit.modLog.LogMessage(
                            $"ERROR: Could not find system with systemId {system.Key}");
                        continue;
                    }

                    foreach (var travelReqs in system.Value)
                    {
                        LostInSpaceInit.modLog.LogMessage(
                            $"Adding {travelReqs} to {starsystem.Name} tags");
                        starsystem.Tags.Add(travelReqs);
                    }
                }

                // process tags into restrictions here!!

                foreach (var SGS_system in __instance.StarSystems)
                {
                    if (!SGS_system.Tags.Any(x => x.StartsWith("LiS__"))) continue;

                    foreach (var tag in SGS_system.Tags.Where(x => x.StartsWith("LiS__")))
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
                                    SGS_system.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) SGS_system.Def.TravelRequirements.Add(reqDef);
                                else companyReq.RequirementTags.Add(tag);
                                LostInSpaceInit.modLog.LogMessage(
                                    $"Added TravelRequirements to {SGS_system.Name}: Requirement Tag {tag}");
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
                                    SGS_system.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) SGS_system.Def.TravelRequirements.Add(excDef);
                                else companyReq.ExclusionTags.Add(tag);
                                LostInSpaceInit.modLog.LogMessage(
                                    $"Added TravelRequirements to {SGS_system.Name}: Exclusion Tag {tag}");
                                continue;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SimGameState), "Init")]
        public static class SimGameState_InitPatch
        {
            private static Regex StarSystemTravel_Restrict =
                new Regex("^LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$", RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            public static void Postfix(SimGameState __instance)
            {
                foreach (var system in LostInSpaceInit.modSettings.hiddenSystems)
                {
                    var starsystem = __instance.GetSystemById(system.Key);
                    if (starsystem == null)
                    {
                        LostInSpaceInit.modLog.LogMessage(
                            $"ERROR: Could not find system with systemId {system.Key}");
                        continue;
                    }

                    foreach (var travelReqs in system.Value)
                    {
                        LostInSpaceInit.modLog.LogMessage(
                            $"Adding {travelReqs} to {starsystem.Name} tags");
                        starsystem.Tags.Add(travelReqs);
                    }
                }

                // process tags into restrictions here!!

                foreach (var SGS_system in __instance.StarSystems)
                {
                    if (!SGS_system.Tags.Any(x => x.StartsWith("LiS__"))) continue;

                    foreach (var tag in SGS_system.Tags.Where(x=>x.StartsWith("LiS__")))
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
                                    SGS_system.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) SGS_system.Def.TravelRequirements.Add(reqDef);
                                else companyReq.RequirementTags.Add(tag);
                                LostInSpaceInit.modLog.LogMessage(
                                    $"Added TravelRequirements to {SGS_system.Name}: Requirement Tag {tag}");
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
                                    SGS_system.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) SGS_system.Def.TravelRequirements.Add(excDef);
                                else companyReq.ExclusionTags.Add(tag);
                                LostInSpaceInit.modLog.LogMessage(
                                    $"Added TravelRequirements to {SGS_system.Name}: Exclusion Tag {tag}");
                                continue;
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(SimGameState), "ApplySimGameEventResult",
            new Type[] {typeof(SimGameEventResult), typeof(List<object>), typeof(SimGameEventTracker)})]
        public static class SimGameState_ApplySimGameEventResult_Patch
        {
            private static Regex Travel_Restrict =
                new Regex("^ADD_LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$", RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            private static Regex Remove_Travel_Restrict =
                new Regex("^REMOVE_LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$", RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            public static void Prefix(SimGameState __instance, ref SimGameEventResult result)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (result.Scope == EventScope.Company && result.AddedTags != null)
                {
                    foreach (string addedTag in result.AddedTags.ToList())
                    {
                        try
                        {
                            MatchCollection matches = Travel_Restrict.Matches(addedTag);
                            if (matches.Count > 0)
                            {
                                var systemID = matches[0].Groups["system"].Value;
                                var system = sim.GetSystemById(systemID);
                                var type = matches[0].Groups["type"].Value;
                                var tag = addedTag.Remove(0, 4);
                                LostInSpaceInit.modLog.LogMessage(
                                    $"Adding tag {tag}: {systemID}");
                                system.Tags.Add(tag);

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
                                    LostInSpaceInit.modLog.LogMessage(
                                        $"Added TravelRequirements to {system.Name}: Requirement Tag {tag}");
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
                                    LostInSpaceInit.modLog.LogMessage(
                                        $"Added TravelRequirements to {system.Name}: Exclusion Tag {tag}");
                                    continue;
                                }
                            }

                            MatchCollection matches2 = Remove_Travel_Restrict.Matches(addedTag);
                            if (matches2.Count > 0)
                            {
                                var systemID = matches2[0].Groups["system"].Value;
                                var system = sim.GetSystemById(systemID);
                                var type = matches2[0].Groups["type"].Value;
                                var tag = addedTag.Remove(0, 7);
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
                                    LostInSpaceInit.modLog.LogMessage(
                                        $"Removed TravelRequirements from {system.Name}: Requirement Tag {tag}");
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
                                    LostInSpaceInit.modLog.LogMessage(
                                        $"Removed TravelRequirements from {system.Name}: Exclusion Tag {tag}");
                                    continue;
                                }

                            }

                        }
                        catch (Exception e)
                        {
                            LostInSpaceInit.modLog.LogException(e);
                        }
                    }
                }
            }
        }

        private static MethodInfo methodSetStarVis =
            AccessTools.Method(typeof(StarmapSystemRenderer), "SetStarVisibility");

        [HarmonyPatch(typeof(StarmapRenderer), "RefreshSystems")]
        public static class StarmapRenderer_RefreshSystems_Patch
        {
            static void Postfix(StarmapRenderer __instance, Dictionary<GameObject, StarmapSystemRenderer> ___systemDictionary)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;

                foreach (var starmapSystemRenderer in new List<StarmapSystemRenderer>( ___systemDictionary.Values))
                {

                    if (starmapSystemRenderer.system.System.Def.TravelRequirements.Any(x=>x.ExclusionTags.Any(y=>y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z=>z.EndsWith("__HIDDEN"))))
                    {
                        LostInSpaceInit.modLog.LogMessage(
                            $"Found hide system condition for {starmapSystemRenderer.system.System.Name}");

                        var xclTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        var reqTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        if(!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                        {
                            LostInSpaceInit.modLog.LogMessage(
                                $"Company tag for {starmapSystemRenderer.system.System.Name} travel/vision tag not found, hiding system. ");

                            if (Util.WIIC_Cleanup != null)
                            {
                                Util.WIIC_Cleanup.Invoke(null, new object[] { starmapSystemRenderer.system.System, });
                                LostInSpaceInit.modLog.LogMessage(
                                    $"WIIC found, removing flareup from {starmapSystemRenderer.system.System.Name}.");
                            }

                            methodSetStarVis.Invoke(starmapSystemRenderer, new object[] {false});
                        }
                    }
                }
            }
        }
    }
}