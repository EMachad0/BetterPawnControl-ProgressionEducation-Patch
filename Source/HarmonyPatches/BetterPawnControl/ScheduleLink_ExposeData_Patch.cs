using System.Collections.Generic;
using BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.BetterPawnControl
{
    [HarmonyPatch(typeof(ScheduleLink), nameof(ScheduleLink.ExposeData))]
    public static class ScheduleLink_ExposeData_Patch
    {
        public static bool Prefix(ScheduleLink __instance)
        {
            Scribe_Values.Look(ref ScheduleLinkWrapper.ZoneRef(__instance), "zone", 0, true);
            Scribe_References.Look(ref ScheduleLinkWrapper.ColonistRef(__instance), "colonist");
            Scribe_References.Look(ref ScheduleLinkWrapper.AreaRef(__instance), "area");
            Scribe_Values.Look(ref ScheduleLinkWrapper.MapIdRef(__instance), "mapId", 0, true);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<string> scheduleDefNames = ScheduleSerializationHelper.GetScheduleDefNames(ScheduleLinkWrapper.ScheduleRef(__instance));
                Scribe_Collections.Look(ref scheduleDefNames, "schedule", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<string> scheduleDefNames = null;
                Scribe_Collections.Look(ref scheduleDefNames, "schedule", LookMode.Value);
                ScheduleSerializationHelper.StorePending(__instance, scheduleDefNames);
            }
            else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                var scheduleLink = new ScheduleLinkWrapper(__instance);
                scheduleLink.schedule = ScheduleSerializationHelper.ResolvePending(__instance);
                ScheduleUtility.TryRepairScheduleLink(scheduleLink);
            }

            return false;
        }

        private static class ScheduleSerializationHelper
        {
            private static readonly Dictionary<ScheduleLink, List<string>> PendingScheduleDefNames = new();

            public static List<string> GetScheduleDefNames(List<TimeAssignmentDef> schedule)
            {
                if (schedule == null)
                {
                    return null;
                }

                var scheduleDefNames = new List<string>(schedule.Count);
                foreach (var timeAssignment in schedule)
                {
                    scheduleDefNames.Add(timeAssignment?.defName);
                }

                return scheduleDefNames;
            }

            public static void StorePending(ScheduleLink scheduleLink, List<string> scheduleDefNames)
            {
                PendingScheduleDefNames[scheduleLink] = scheduleDefNames;
            }

            public static List<TimeAssignmentDef> ResolvePending(ScheduleLink scheduleLink)
            {
                return ResolveSchedule(TakePendingScheduleDefNames(scheduleLink));
            }

            private static List<string> TakePendingScheduleDefNames(ScheduleLink scheduleLink)
            {
                if (!PendingScheduleDefNames.TryGetValue(scheduleLink, out var scheduleDefNames))
                {
                    return null;
                }

                PendingScheduleDefNames.Remove(scheduleLink);
                return scheduleDefNames;
            }

            private static List<TimeAssignmentDef> ResolveSchedule(List<string> scheduleDefNames)
            {
                if (scheduleDefNames == null)
                {
                    return null;
                }

                var resolvedSchedule = new List<TimeAssignmentDef>(scheduleDefNames.Count);
                for (int hour = 0; hour < scheduleDefNames.Count; hour++)
                {
                    var defName = scheduleDefNames[hour];
                    TimeAssignmentDef assignment = null;

                    if (!string.IsNullOrEmpty(defName) && defName != "null")
                    {
                        string compatibleDefName = BackCompatibility.BackCompatibleDefName(typeof(TimeAssignmentDef), defName);
                        assignment = DefDatabase<TimeAssignmentDef>.GetNamedSilentFail(compatibleDefName);
                    }

                    resolvedSchedule.Add(assignment ?? ScheduleUtility.GetDefaultTimeAssignmentDef(hour));
                }

                return resolvedSchedule;
            }
        }
    }
}
