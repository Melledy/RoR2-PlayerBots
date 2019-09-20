using EntityStates.AI.Walker;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.Custom
{
    class PlayerBotCombatFix : MonoBehaviour
    {
        private CharacterMaster master;
        private EntityStateMachine stateMachine;
        private BaseAI ai;

        public void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.ai = base.GetComponent<BaseAI>();
            this.stateMachine = base.GetComponent<EntityStateMachine>();
        }

        public void FixedUpdate()
        {
            if (this.stateMachine.state is Combat)
            {
                ((Combat)this.stateMachine.state).SetFieldValue("updateTimer", 0);
            }
            /*
            if (this.master.GetBody() && this.master.alive)
            {
                ProcessEquipment();
            }
            */
        }

        private void ProcessEquipment()
        {
            if (this.master.inventory.currentEquipmentIndex != EquipmentIndex.None || this.master.GetBody().equipmentSlot.stock == 0)
            {
                return;
            }

            switch (this.master.inventory.currentEquipmentIndex)
            {
                case EquipmentIndex.Scanner:
                    FireEquipment();
                    break;
                case EquipmentIndex.Lightning:
                    FireEquipment();
                    break;
                case EquipmentIndex.Fruit:
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .5)
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.CommandMissile:
                    //FireEquipment();
                    break;
            }
        }

        private void FireEquipment()
        {
            this.master.GetBody().equipmentSlot.ExecuteIfReady();
        }
    }
}
