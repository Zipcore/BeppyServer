using HarmonyLib;

namespace BeppyServer.Patches {
    [HarmonyPatch(typeof(World))]
    internal class WorldPatch {
        public delegate void SaveCallback();

        public static SaveCallback OnSaveWorld;

        // Is occasionally called before "World" type is loaded?
        [HarmonyPrefix]
        [HarmonyPatch("Cleanup")]
        private static void Cleanup() {
            if (OnSaveWorld != null)
                OnSaveWorld();
        }
    }
}