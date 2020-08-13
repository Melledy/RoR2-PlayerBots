using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlayerBots.Custom
{
    class PlayerBotSpawnCard : CharacterSpawnCard
    {
        public string playerbotName;
        public GameObject bodyPrefab;

        public PlayerBotSpawnCard() {
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
