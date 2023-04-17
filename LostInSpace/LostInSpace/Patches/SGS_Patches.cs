using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.UI;
using HBS.Collections;
using LostInSpace.Framework;

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
            {//
             //   foreach (var system in LostInSpaceInit.modSettings.hiddenSystems)
             //   {
             //       var starsystem = __instance.GetSystemById(system.Key);
             //       if (starsystem == null)
             //       {
             //           LostInSpaceInit.modLog?.Info?.Write(
             //               $"ERROR: Could not find system with systemId {system.Key}");
             //           continue;
             //       }
             //
             //       foreach (var travelReqs in system.Value)
             //       {
             //           LostInSpaceInit.modLog?.Info?.Write(
             //               $"Adding {travelReqs} to {starsystem.Name} tags");
             //           starsystem.Tags.Add(travelReqs);
             //       }
             //   }

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
                                LostInSpaceInit.modLog?.Info?.Write(
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
                                LostInSpaceInit.modLog?.Info?.Write(
                                    $"Added TravelRequirements to {SGS_system.Name}: Exclusion Tag {tag}");
                                continue;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SGCharacterCreationCareerBackgroundSelectionPanel), "Done")]
        public static class SGCharacterCreationCareerBackgroundSelectionPanel_Done_Patch
        {
            private static Regex StarSystemTravel_Restrict =
                new Regex("^LiS__(?<type>.*?)__(?<ident>.*?)__(?<system>.*?)__(?<hidden>.*)$", RegexOptions.Compiled); //shamelessly stolen from BlueWinds

            public static void Postfix(SGCharacterCreationCareerBackgroundSelectionPanel __instance)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (sim == null) return;
                foreach (var system in LostInSpaceInit.modSettings.hiddenSystems)
                {
                    var starsystem = sim.GetSystemById(system.Key);
                    if (starsystem == null)
                    {
                        LostInSpaceInit.modLog?.Info?.Write(
                            $"ERROR: Could not find system with systemId {system.Key}");
                        continue;
                    }

                    foreach (var travelReqs in system.Value)
                    {
                        LostInSpaceInit.modLog?.Info?.Write(
                            $"Adding {travelReqs} to {starsystem.Name} tags");
                        starsystem.Tags.Add(travelReqs);
                    }
                }

                // process tags into restrictions here!!

                foreach (var SGS_system in sim.StarSystems)
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
                                LostInSpaceInit.modLog?.Info?.Write(
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
                                LostInSpaceInit.modLog?.Info?.Write(
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
                                LostInSpaceInit.modLog?.Info?.Write(
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
                                    LostInSpaceInit.modLog?.Info?.Write(
                                        $"Added TravelRequirements to {system.Name}: Requirement Tag {tag}");
                                    result.AddedTags.Remove(addedTag);
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
                                        $"Added TravelRequirements to {system.Name}: Exclusion Tag {tag}");
                                    result.AddedTags.Remove(addedTag);
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
                                    LostInSpaceInit.modLog?.Info?.Write(
                                        $"Removed TravelRequirements from {system.Name}: Requirement Tag {tag}");
                                    result.AddedTags.Remove(addedTag);
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
                                        $"Removed TravelRequirements from {system.Name}: Exclusion Tag {tag}");
                                    result.AddedTags.Remove(addedTag);
                                    continue;
                                }

                            }

                        }
                        catch (Exception e)
                        {
                            LostInSpaceInit.modLog?.Error?.Write(e);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SGNavigationScreen), "OnSystemHovered")]
        public static class SGNavigationScreen_OnSystemHovered
        {
            static void Prefix(ref bool __runOriginal, SGNavigationScreen __instance, StarSystem hoveredSystem)
            {
                if (!__runOriginal) return;
                if (hoveredSystem != null)
                {
                    var sim = UnityGameInstance.BattleTechGame.Simulation;
                    if (hoveredSystem.Def.TravelRequirements.Any(x => x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                    {
                        LostInSpaceInit.modLog?.Trace?.Write(
                            $"Found hide system condition for {hoveredSystem.Name}");

                        var xclTags =
                            hoveredSystem.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        var reqTags =
                            hoveredSystem.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                        {
                            LostInSpaceInit.modLog?.Trace?.Write(
                                $"Company tag for {hoveredSystem.Name} travel/vision tag not found, not displaying hover tooltipicon. ");

                            __runOriginal = false;
                            return;
                        }
                    }
                }
                __runOriginal = true;
                return;
            }
        }

        [HarmonyPatch(typeof(StarmapRenderer), "SetSelectedSystemRenderer")]
        public static class StarmapRenderer_SetSelectedSystemRenderer
        {
            static void Prefix(ref bool __runOriginal, StarmapRenderer __instance, StarmapSystemRenderer systemRenderer)
            {
                if (!__runOriginal) return;
                if (systemRenderer != null)
                {
                    var sim = UnityGameInstance.BattleTechGame.Simulation;
                    if (systemRenderer.system.System.Def.TravelRequirements.Any(x =>
                            x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) ||
                            x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                    {
                        LostInSpaceInit.modLog?.Trace?.Write(
                            $"Found hide system condition for {systemRenderer.system.System.Name}");

                        var xclTags =
                            systemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        var reqTags =
                            systemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                        {
                            LostInSpaceInit.modLog?.Trace?.Write(
                                $"Company tag for {systemRenderer.system.System.Name} travel/vision tag not found, not selecting system for routing. ");

                            __runOriginal = false;
                            return;
                        }
                    }
                }
                __runOriginal = true;
                return;
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class StarmapSystemRenderer_SetStarVisibility
        {
            static void Postfix(StarmapSystemRenderer __instance, bool starOn)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;

                if (__instance.system.System.Def.TravelRequirements.Any(x => x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                {
                    LostInSpaceInit.modLog?.Debug?.Write(
                        $"Found hide system condition for {__instance.system.System.Name}");

                    var xclTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    var reqTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                    {
                        LostInSpaceInit.modLog?.Info?.Write(
                            $"Company tag for {__instance.system.System.Name} travel/vision tag not found, hiding system. ");

                        if (Util.WIIC_Cleanup != null)
                        {
                            Util.WIIC_Cleanup.Invoke(null, new object[] { __instance.system.System, });
                            LostInSpaceInit.modLog?.Debug?.Write(
                                $"WIIC found, removing flareup from {__instance.system.System.Name}.");
                        }
                        __instance.starInner.gameObject.SetActive(false);
                        __instance.starInnerUnvisited.gameObject.SetActive(false);
                        //methodSetStarVis.Invoke(__instance, new object[] { false });
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetBiome")]
        public static class StarmapSystemRenderer_SetBiome
        {
            static void Prefix(ref bool __runOriginal, StarmapSystemRenderer __instance, Biome.BIOMESKIN theBiome)
            {
                if (!__runOriginal) return;
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (__instance.system.System.Def.TravelRequirements.Any(x => x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                {
                    LostInSpaceInit.modLog?.Trace?.Write(
                        $"Found hide system condition for {__instance.system.System.Name}");

                    var xclTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    var reqTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                    {
                        LostInSpaceInit.modLog?.Trace?.Write(
                            $"Company tag for {__instance.system.System.Name} travel/vision tag not found, not displaying biome icon. ");

                        __runOriginal = false;
                        return;
                    }
                }
                __runOriginal = true;
                return;
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStore")]
        public static class StarmapSystemRenderer_SetStore
        {
            static void Prefix(ref bool __runOriginal, StarmapSystemRenderer __instance, string store)
            {
                if (!__runOriginal) return;
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (__instance.system.System.Def.TravelRequirements.Any(x => x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                {
                    LostInSpaceInit.modLog?.Trace?.Write(
                        $"Found hide system condition for {__instance.system.System.Name}");

                    var xclTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    var reqTags =
                        __instance.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                    {
                        LostInSpaceInit.modLog?.Trace?.Write(
                            $"Company tag for {__instance.system.System.Name} travel/vision tag not found, not displaying store icon. ");

                        __runOriginal = false;
                        return;
                    }
                }
                __runOriginal = true;
                return;
            }
        }

        [HarmonyPatch(typeof(SGNavStarSystemCallout), "ShowDifficultyWidget")]
        public static class SGNavStarSystemCallout_ShowDifficultyWidget
        {
            static void Prefix(ref bool __runOriginal, SGNavStarSystemCallout __instance, bool active)
            {
                if (!__runOriginal) return;
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (__instance.SystemRenderer.system.System.Def.TravelRequirements.Any(x => x.ExclusionTags.Any(y => y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z => z.EndsWith("__HIDDEN"))))
                {
                    LostInSpaceInit.modLog?.Trace?.Write(
                        $"Found hide system condition for {__instance.SystemRenderer.system.System.Name}");

                    var xclTags =
                        __instance.SystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    var reqTags =
                        __instance.SystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                            x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                    if (!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                    {
                        LostInSpaceInit.modLog?.Trace?.Write(
                            $"Company tag for {__instance.SystemRenderer.system.System.Name} travel/vision tag not found, not displaying difficulty callout. ");

                        __runOriginal = false;
                        return;
                    }
                }
                __runOriginal = true;
                return;
            }
        }


        //deprecated below
        //private static MethodInfo methodSetStarVis = AccessTools.Method(typeof(StarmapSystemRenderer), "SetStarVisibility");

        [HarmonyPatch(typeof(StarmapRenderer), "RefreshSystems")]
        public static class StarmapRenderer_RefreshSystems_Patch
        {
            static bool Prepare() => false; //disable, move patch to SetStarVisibility
            static void Postfix(StarmapRenderer __instance)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;

                foreach (var starmapSystemRenderer in new List<StarmapSystemRenderer>( __instance.systemDictionary.Values))
                {

                    if (starmapSystemRenderer.system.System.Def.TravelRequirements.Any(x=>x.ExclusionTags.Any(y=>y.EndsWith("__HIDDEN")) || x.RequirementTags.Any(z=>z.EndsWith("__HIDDEN"))))
                    {
                        LostInSpaceInit.modLog?.Info?.Write(
                            $"Found hide system condition for {starmapSystemRenderer.system.System.Name}");

                        var xclTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.ExclusionTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        var reqTags =
                            starmapSystemRenderer.system.System.Def.TravelRequirements.SelectMany(x =>
                                x.RequirementTags.Where(y => y.EndsWith("__HIDDEN"))).ToList();

                        if(!sim.CompanyTags.Intersect(reqTags).Any() || sim.CompanyTags.Intersect(xclTags).Any())
                        {
                            LostInSpaceInit.modLog?.Info?.Write(
                                $"Company tag for {starmapSystemRenderer.system.System.Name} travel/vision tag not found, hiding system. ");

                            if (Util.WIIC_Cleanup != null)
                            {
                                Util.WIIC_Cleanup.Invoke(null, new object[] { starmapSystemRenderer.system.System, });
                                LostInSpaceInit.modLog?.Info?.Write(
                                    $"WIIC found, removing flareup from {starmapSystemRenderer.system.System.Name}.");
                            }
                            starmapSystemRenderer.SetStarVisibility(false);
                            //methodSetStarVis.Invoke(starmapSystemRenderer, new object[] {false});
                        }
                    }
                }
            }
        }
    }
}