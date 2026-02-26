using System.Collections.Generic;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.RimWorld
{
    // Class timetable defs can persist on pawns that leave their original map context,
    // which can later reintroduce class-only slots in non-class situations.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExitMap))]
    public static class Pawn_ExitMap_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance == null || !__instance.IsColonist)
            {
                return;
            }

            ScheduleUtility.RemoveClassesFromPawnTimetables(new List<Pawn> { __instance });
        }
    }
}
