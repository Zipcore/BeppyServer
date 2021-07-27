using System.Collections.Generic;

namespace BeppyServer {
    public class WorldManager {
        private static World World => GameManager.Instance.World;

        public static List<TraderArea> Traders => World.TraderAreas;

        // Tested and working
        public static EntityPlayer GetPlayer(string name) {
            foreach (EntityPlayer player in World.Players.list) {
                if (player.EntityName.EqualsCaseInsensitive(name))
                    return player;
            }

            return null;
        }

        // Tested and working
        public static EntityPlayer GetPlayer(int entityid) {
            foreach (EntityPlayer player in World.Players.list) {
                if (player.entityId == entityid)
                    return player;
            }

            return null;
        }
    }
}