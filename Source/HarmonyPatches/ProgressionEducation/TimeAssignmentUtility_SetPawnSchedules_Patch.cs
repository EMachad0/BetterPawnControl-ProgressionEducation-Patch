using System.Collections.Generic;
using System.Linq;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch
{
    [HarmonyPatch(typeof(TimeAssignmentUtility), "SetPawnSchedules")]
    public static class TimeAssignmentUtility_SetPawnSchedules_Patch
    {
        public static bool Prefix(StudyGroup studyGroup, List<Pawn> participants, TimeAssignmentDef assignment = null)
        {
            var activePolicyId = ScheduleManagerWrapper.GetActivePolicyIdOrDefault();
            var scheduleLink = getDefaultScheduleLink();
            if (scheduleLink == null)
            {
                Log.Error($"[BPC PE Patch] ScheduleLink 0 not found.");
            }

            foreach (var participant in participants)
            {
                TimeAssignmentUtility.TryRepairTimetable(participant);
                for (int hour = 0; hour < 24; hour++)
                {
                    bool isScheduled;
                    if (studyGroup.startHour <= studyGroup.endHour)
                    {
                        isScheduled = hour >= studyGroup.startHour && hour <= studyGroup.endHour;
                    }
                    else
                    {
                        isScheduled = hour >= studyGroup.startHour || hour <= studyGroup.endHour;
                    }

                    if (isScheduled)
                    {
                        TimeAssignmentDef assignmentToSet = assignment ?? ((hour > 5 && hour <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);

                        SetBPCAssignment(participant, hour, assignmentToSet, scheduleLink);
                    }
                }
            }

            return false;
        }

        private static void SetBPCAssignment(Pawn pawn, int hour, TimeAssignmentDef assignment, ScheduleLinkWrapper link)
        {
            if (!link.TrySetAssignment(hour, assignment))
            {
                Log.Error($"[BPC PE Patch] Invalid schedule index {hour} for pawn {pawn.LabelShort}.");
                return;
            }

            EducationLog.Message($"Set timetable for pawn {pawn.LabelShort} on BPC policy {link.zone} at hour {hour} to {assignment.defName}");
        }

        private static ScheduleLinkWrapper getDefaultScheduleLink()
        {
            return ScheduleManagerWrapper.EnumerateScheduleLinks().First(l => l.zone == 0);
        }
    }
}

