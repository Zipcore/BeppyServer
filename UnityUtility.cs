using UnityEngine;
using BepInEx;

namespace BeppyServer {
    public static class UnityUtility
    {
        public static void OutputComponents(string gameObjectName)
        {
            GameObject obj = GameObject.Find(gameObjectName);
            if (obj == null)
            {
                BeppyServer.Log($"No game object by name {gameObjectName}");
                return;
            }

            Component[] components = obj.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
            {
                BeppyServer.Log(components[i].name);
            }
        }

        public static void OutputGameObjects()
        {
            UnityEngine.Object[] gameObjects = GameObject.FindObjectsOfType(typeof(MonoBehaviour)); //returns Object[]
            foreach (var obj in gameObjects)
            {
                BeppyServer.Log($"{obj.GetType().FullName} {obj.name}");
            }
        }
    }
}