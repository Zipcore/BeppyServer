using HarmonyLib;

namespace BeppyServer.Patches {
    [HarmonyPatch(typeof(GameStateManager), "StartGame")]
    internal class GameStateManagerPatch {
        public delegate void StartGameFinishedCallback();

        public static StartGameFinishedCallback OnStartGameFinished;

        // Called right before "StartGame done" message
        private static void Postfix() {
            OnStartGameFinished();
        }
    }
}