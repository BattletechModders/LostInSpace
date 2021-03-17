using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
        public static class SimGameState_OnDayPassed_Patch
        {
            public static bool Prepare()
            {
                return false;
            }
            static void Postfix(SimGameState __instance)
            {
                foreach (var system in __instance.StarSystems)
                {

                    if (system.Def.TravelRequirements.Any(x =>
                        x.ExclusionTags.Any(y => y.EndsWith("HIDDEN")) ||
                        x.RequirementTags.Any(z => z.EndsWith("HIDDEN"))))
                    {
                        var xclTags =
                            system.Def.TravelRequirements.Where(x =>
                                x.ExclusionTags.Any(y => y.EndsWith("HIDDEN"))).SelectMany(x => x.ExclusionTags).ToList();

                        var reqTags =
                            system.Def.TravelRequirements.Where(x =>
                                    x.RequirementTags.Any(y => y.EndsWith("HIDDEN"))).SelectMany(x => x.RequirementTags)
                                .ToList();

                        if ((!__instance.CompanyTags.Any(x => reqTags.Contains(x))) ||
                            (__instance.CompanyTags.Any(x => xclTags.Contains(x))))
                        {
                            WIIC.cleanupSystem(system);
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
                            if (travelReqs.StartsWith("LiS_NavExc_"))
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

                            if (travelReqs.StartsWith("LiS_NavReq_"))
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

                    if (starmapSystemRenderer.system.System.Def.TravelRequirements.Any(x=>x.ExclusionTags.Any(y=>y.EndsWith("_HIDDEN")) || x.RequirementTags.Any(z=>z.EndsWith("_HIDDEN"))))
                    {
                        LostInSpaceInit.modLog.Debug?.Write(
                            $"Found travel restriction for {starmapSystemRenderer.system.System.Name}");

                        var xclTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("_HIDDEN"))).ToList();

                        var reqTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("_HIDDEN"))).ToList();

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