# Better Pawn Control + Progression: Education Patch

Compatibility patch for RimWorld `1.6` that resolves schedule interactions between:

- Better Pawn Control (`VouLT.BetterPawnControl`)
- Progression: Education (`ferny.ProgressionEducation`)

## Why This Mod Exists

ProgressionEducation dynamically updates each pawn’s live schedule so they attend classes at the correct times.

BetterPawnControl, however, treats the live schedule as a secondary source. It saves and restores schedules from its own internal list. This leads to conflicts with ProgressionEducation, such as class assignments being removed, or remaining even after a class has finished.

## Design Goal

To ensure both mods work together, ProgressionEducation needs to update BetterPawnControl’s internal schedules dynamically.
This mod takes the simplest approach: ProgressionEducation always updates the default schedule, making it the only schedule that includes classes.

If you have a use case where this is not sufficient, let me know.

## Installation

### Steam

Subscribe to all required mods and this patch.

## Scope

This patch is focused on schedule compatibility between the two target mods.
It does not attempt to change unrelated behaviors from either dependency.

