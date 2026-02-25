using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.Utilities
{
    public static class ScheduleUtility
    {
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

        internal static ScheduleLinkWrapper GetScheduleLink(Pawn pawn, int policyId)
        {
            return ScheduleManagerWrapper
                .EnumerateScheduleLinks()
                .FirstOrFallback(l => l.zone == policyId && l.colonist != null && pawn.Equals(l.colonist) && l.mapId == pawn.Map.uniqueID);
        }
    }
}
