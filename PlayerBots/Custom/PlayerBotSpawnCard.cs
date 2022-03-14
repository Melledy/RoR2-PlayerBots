using RoR2;
using System;
using UnityEngine;

namespace PlayerBots.Custom
{
    class PlayerBotSpawnCard : CharacterSpawnCard
    {
        public GameObject bodyPrefab;

        public PlayerBotSpawnCard()
        {
            this.loadout = new SerializableLoadout(); // Prevent errors
            this.runtimeLoadout = new Loadout();
        }

        protected override Action<CharacterMaster> GetPreSpawnSetupCallback()
        {
            return master =>
            {
                master.bodyPrefab = this.bodyPrefab;
            };
        }
    }
}
