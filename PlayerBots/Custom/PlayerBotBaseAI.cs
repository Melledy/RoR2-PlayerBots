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

        /*
        public override void OnBodyStart(CharacterBody newBody)
        {
            base.OnBodyStart(newBody);
            this.name = newBody.GetDisplayName();
            this.master.name = newBody.GetDisplayName();
        }
        */

        public override void OnBodyLost()
        {

        }
    }
}
