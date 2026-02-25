using Verse;

namespace BetterPawnControlProgressionEducationPatch
{
    [StaticConstructorOnStartup]
    public static class HelloWorld
    {
        static HelloWorld()
        {
            Log.Message("[BPC PE Patch] Hello world! Mod assembly loaded.");
        }
    }
}
