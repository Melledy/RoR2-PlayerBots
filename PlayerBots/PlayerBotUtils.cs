using RoR2;
using UnityEngine;

namespace PlayerBots
{
    static class PlayerBotUtils
    {
        public static bool TryGetSurvivorIndexByBodyPrefabName(string bodyPrefabName, out SurvivorIndex index)
        {
            index = SurvivorIndex.None;

            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(bodyPrefabName);
            if (bodyPrefab == null) return false;

            SurvivorDef def = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
            if (def == null) return false;

            index = def.survivorIndex;
            return true;
        }
    }
}
