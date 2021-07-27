using System.Collections.Generic;
using UnityEngine;

namespace BeppyServer {
    public static class UnityUtility {
        public static List<string> OutputComponents(string gameObjectName) {
            List<string> componentNames = new List<string>();
            GameObject obj = GameObject.Find(gameObjectName);
            if (obj != null) {
                Component[] components = obj.GetComponents(typeof(Component));
                for (int i = 0; i < components.Length; i++) componentNames.Add(components[i].name);
            }

            return componentNames;
        }

        public static List<string> OutputGameObjects() {
            List<string> gameObjectNames = new List<string>();
            Object[] gameObjects = Object.FindObjectsOfType(typeof(MonoBehaviour)); //returns Object[]
            foreach (Object obj in gameObjects) gameObjectNames.Add($"{obj.GetType().FullName} {obj.name}");

            return gameObjectNames;
        }
    }
}