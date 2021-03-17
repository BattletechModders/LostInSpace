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
using WarTechIIC;

namespace LostInSpace.Patches
{
    class SGS_Patches
    {
        [HarmonyPatch(typeof(SimGameState), "ApplySimGameEventResult",
            new Type[] {typeof(SimGameEventResult), typeof(List<object>), typeof(SimGameEventTracker)})]
        public static class SimGameState_ApplySimGameEventResult_Patch
        {
            private static Regex Travel_Restrict =
                new Regex("^LiS__(?<type>.*?)__(?<system>.*?)__(?<hidden>.*)$", RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            public static void Prefix(SimGameState __instance, ref SimGameEventResult result)
            {
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
                                var system = __instance.GetSystemById(systemID);
                                var type = matches[0].Groups["type"].Value;

                                if (type == "NavReq")
                                {
                                    var reqDef = new RequirementDef()
                                    {
                                        Scope = EventScope.Company,
                                        RequirementTags = new TagSet(addedTag)
                                    };

                                    var companyReq =
                                        system.Def.TravelRequirements.FirstOrDefault(x =>
                                            x.Scope == EventScope.Company);
                                    if (companyReq == null) system.Def.TravelRequirements.Add(reqDef);
                                    else companyReq.RequirementTags.Add(addedTag);
                                    LostInSpaceInit.modLog.Debug?.Write(
                                        $"Added TravelRequirements to {system.Name}: Requirement Tag {addedTag}");
                                    result.AddedTags.Remove(addedTag);
                                    continue;
                                }

                                if (type == "NavExc")
                                {
                                    var reqDef = new RequirementDef()
                                    {
                                        Scope = EventScope.Company,
                                        RequirementTags = new TagSet(addedTag)
                                    };

                                    var companyReq =
                                        system.Def.TravelRequirements.FirstOrDefault(x =>
                                            x.Scope == EventScope.Company);
                                    if (companyReq == null) system.Def.TravelRequirements.Add(reqDef);
                                    else companyReq.ExclusionTags.Add(addedTag);
                                    LostInSpaceInit.modLog.Debug?.Write(
                                        $"Added TravelRequirements to {system.Name}: Exclusion Tag {addedTag}");
                                    result.AddedTags.Remove(addedTag);
                                    continue;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LostInSpaceInit.modLog.Error?.Write(e);
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
                if (Util.HolderInstance.hiddenSystemsDict.Count > 0)
                {
                    foreach (var system in Util.HolderInstance.hiddenSystemsDict)
                    {
                        var starsystem = sim.GetSystemById(system.Key);
                        if (starsystem == null)
                        {
                            LostInSpaceInit.modLog.Debug?.Write(
                                $"ERROR: Could not find system with systemId {system.Key}");
                            continue;
                        }

                        foreach (var travelReqs in system.Value)
                        {
                            if (travelReqs.StartsWith("LiS__NavExc__"))
                            {
                                var reqDef = new RequirementDef()
                                {
                                    Scope = EventScope.Company,
                                    ExclusionTags = new TagSet(travelReqs)
                                };

                                var companyReq =
                                    starsystem.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) starsystem.Def.TravelRequirements.Add(reqDef);
                                else companyReq.ExclusionTags.Add(travelReqs);
                                LostInSpaceInit.modLog.Debug?.Write(
                                    $"Added TravelRequirements to {starsystem.Name}: Exclusion Tag {travelReqs}");
                            }

                            if (travelReqs.StartsWith("LiS__NavReq__"))
                            {
                                var reqDef = new RequirementDef()
                                {
                                    Scope = EventScope.Company,
                                    RequirementTags = new TagSet(travelReqs)
                                };

                                var companyReq =
                                    starsystem.Def.TravelRequirements.FirstOrDefault(x =>
                                        x.Scope == EventScope.Company);
                                if (companyReq == null) starsystem.Def.TravelRequirements.Add(reqDef);
                                else companyReq.RequirementTags.Add(travelReqs);
                                LostInSpaceInit.modLog.Debug?.Write(
                                    $"Added TravelRequirements to {starsystem.Name}: Requirement Tag {travelReqs}");
                            }
                        }
                    }
                }

                foreach (var starmapSystemRenderer in new List<StarmapSystemRenderer>( ___systemDictionary.Values))
                {

                    if (starmapSystemRenderer.system.System.Def.TravelRequirements.Any(x=>x.ExclusionTags.Any(y=>y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z=>z.EndsWith("__HIDDEN"))))
                    {
                        LostInSpaceInit.modLog.Debug?.Write(
                            $"Found travel restriction for {starmapSystemRenderer.system.System.Name}");

                        var xclTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        var reqTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        if(!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                        {
                            LostInSpaceInit.modLog.Debug?.Write(
                                $"Company tag for {starmapSystemRenderer.system.System.Name} travel restriction not found, hiding system and removing flareups. ");
                            WIIC.cleanupSystem(starmapSystemRenderer.system.System);
                            methodSetStarVis.Invoke(starmapSystemRenderer, new object[] {false});
                        }
                    }
                }
            }
        }
    }
}