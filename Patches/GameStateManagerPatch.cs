using HarmonyLib;

namespace BeppyServer.Patches
{
    [HarmonyPatch(typeof(GameStateManager), "StartGame")]
    class GameStateManagerPatch
    {
        public delegate void StartGameFinishedCallback();
        public static StartGameFinishedCallback OnStartGameFinished;

        // Called right before "StartGame done" message
        static void Postfix()
        {
            OnStartGameFinished();
        }
    }
}
