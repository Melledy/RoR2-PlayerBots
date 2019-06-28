using EntityStates;
using EntityStates.AI.Walker;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerBots.Custom
{
    class PlayerBotStateMachine : EntityStateMachine
    {
        PlayerBotStateMachine()
        {
            this.customName = "AI";
            this.initialStateType = new SerializableEntityStateType(typeof(Wander));
            this.mainStateType = new SerializableEntityStateType(typeof(Wander));
        }
    }
}
