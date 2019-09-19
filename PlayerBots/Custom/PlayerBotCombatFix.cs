using EntityStates.AI.Walker;
using RoR2;
using UnityEngine;

namespace PlayerBots.Custom
{
    class PlayerBotCombatFix : MonoBehaviour
    {
        private EntityStateMachine stateMachine;

        public void Awake()
        {
            this.stateMachine = base.GetComponent<EntityStateMachine>();
        }

        public void LateUpdate()
        {
            if (this.stateMachine.state is Combat)
            {
                ((Combat)this.stateMachine.state).SetFieldValue("updateTimer", 0);
            }
        }
    }
}
