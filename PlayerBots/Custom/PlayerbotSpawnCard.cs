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
        }

        public override GameObject DoSpawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest)
        {
            MasterSummon summon = new MasterSummon
            {
                masterPrefab = this.prefab,
                position = position,
                rotation = rotation,
                summonerBodyObject = directorSpawnRequest.summonerBodyObject,
                teamIndexOverride = directorSpawnRequest.teamIndexOverride,
                ignoreTeamMemberLimit = directorSpawnRequest.ignoreTeamMemberLimit
            };
            if (playerbotName != null)
            {
                summon.preSpawnSetupCallback += OnPreSpawn;
            }
            CharacterMaster characterMaster = summon.Perform();
            if (characterMaster == null)
            {
                return null;
            }
            return characterMaster.gameObject;
        }

        private void OnPreSpawn(CharacterMaster master)
        {
            master.bodyPrefab = this.bodyPrefab;
            master.name = playerbotName;
        }
    }
}
