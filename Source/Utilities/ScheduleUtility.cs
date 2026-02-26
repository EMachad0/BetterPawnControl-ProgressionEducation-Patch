using System.Collections.Generic;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.Utilities
{
    public static class ScheduleUtility
    {
        public readonly static int defaultClassPolicyId = 0;

        internal static void SetBPCAssignment(ScheduleLinkWrapper link, StudyGroup studyGroup, TimeAssignmentDef assignment = null)
        {
            TryRepairScheduleLink(link);
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
                    TimeAssignmentDef assignmentToSet = assignment ?? GetDefaultTimeAssignmentDef(hour);
                    if (!link.TrySetAssignment(hour, assignmentToSet))
                    {
                        Log.Error($"[BPC PE Patch] Invalid schedule index {hour} for pawn {link.colonist.LabelShort}.");
                        return;
                    }

                    EducationLog.Message($"Set timetable for pawn {link.colonist.LabelShort} on BPC policy {link.zone} at hour {hour} to {assignmentToSet.defName}");
                }
            }
        }

        internal static void TryRepairScheduleLink(ScheduleLinkWrapper scheduleLink)
        {
            if (scheduleLink.schedule is null)
            {
                Log.Error($"[BPC PE Patch] ScheduleLink without schedule for {scheduleLink.colonist.LabelShort} and policyId {scheduleLink.zone}.");
                return;
            }

            while (scheduleLink.schedule.Count < 24)
            {
                int hour = scheduleLink.schedule.Count;
                Log.Message($"Timetable for {scheduleLink.colonist.LabelShort} is incomplete. Appending hour {hour}.");
                scheduleLink.schedule.Add(GetDefaultTimeAssignmentDef(hour));
            }
        }


        public static TimeAssignmentDef GetDefaultTimeAssignmentDef(int hour)
        {
            return (hour > 5 && hour <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep;
        }

        public static void RemoveClassesFromPawnTimetables(List<Pawn> pawns)
        {
            if (pawns is null)
            {
                return;
            }

            foreach (var pawn in pawns)
            {
                if (pawn?.timetable == null)
                {
                    continue;
                }

                for (int hour = 0; hour < 24; hour++)
                {
                    var timeDef = pawn.timetable.GetAssignment(hour);
                    if (TimeAssignmentUtility.IsStudyGroupAssignment(timeDef))
                    {
                        var assignmentToSet = ScheduleUtility.GetDefaultTimeAssignmentDef(hour);
                        pawn.timetable.SetAssignment(hour, assignmentToSet);
                    }
                }
            }
        }
    }
}
