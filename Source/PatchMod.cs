using HarmonyLib;
using Verse;

namespace BetterPawnControlProgressionEducationPatch
{
    public class PatchMod : Mod
    {
        public PatchMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                new Harmony("PatchMod").PatchAll();
                Harmony.DEBUG = true;
            });
        }
    }
}

