using EntityStates;
using EntityStates.AI.Walker;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerBots.Custom
{
    class PlayerBotBaseAI : BaseAI
    {
        PlayerBotBaseAI()
        {
            this.scanState = new SerializableEntityStateType(typeof(Wander));
            this.fullVision = true;
            this.aimVectorDampTime = .01f;
            this.aimVectorMaxSpeed = 180f;
            this.enemyAttentionDuration = 5;
            this.nodegraphType = MapNodeGroup.GraphType.Ground;
            this.navigationType = BaseAI.NavigationType.Nodegraph;
            this.selectedSkilldriverName = "";
        }

        public override void OnBodyDeath()
        {
            if (this.body)
            {
                int num = UnityEngine.Random.Range(0, 37);
                string baseToken = "PLAYER_DEATH_QUOTE_" + num;
                Chat.SendBroadcastChat(new Chat.PlayerDeathChatMessage
                {
                    subjectNetworkUser = null,
                    subject = this.body.masterObject,
                    baseToken = baseToken,
                    paramTokens = new string[]
                        {
                        this.master.name
                        }
                });
            }
        }

        public override void OnBodyLost()
        {

        }
    }
}
