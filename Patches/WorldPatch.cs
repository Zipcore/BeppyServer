using HarmonyLib;

namespace BeppyServer.Patches
{
    [HarmonyPatch(typeof(World))]
    class WorldPatch
    {
        public delegate void SaveCallback();
        public static SaveCallback OnSaveWorld;

        [HarmonyPrefix]
        [HarmonyPatch("Cleanup")]
        static void Cleanup() => OnSaveWorld();
    }
}
