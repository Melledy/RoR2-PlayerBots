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
        private Run.FixedTimeStamp lastEquipmentUse;

        public void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.ai = base.GetComponent<BaseAI>();
            this.stateMachine = base.GetComponent<EntityStateMachine>();
        }

        public void FixedUpdate()
        {
            // Remove the default combat delay with ai
            if (this.stateMachine.state is Combat)
            {
                ((Combat)this.stateMachine.state).SetFieldValue("updateTimer", 0);
            }
            // Equipment
            if (this.master.GetBody() && this.master.alive)
            {
                ProcessEquipment();
            }
        }

        private void ProcessEquipment()
        {
            if (this.master.inventory.currentEquipmentIndex == EquipmentIndex.None || this.master.GetBody().equipmentSlot.stock == 0)
            {
                return;
            }

            switch (this.master.inventory.currentEquipmentIndex)
            {
                case EquipmentIndex.CommandMissile:
                case EquipmentIndex.BFG:
                case EquipmentIndex.Lightning:
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS)
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.CritOnUse:
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.master.GetBody().HasBuff(BuffIndex.FullCrit))
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.Blackhole:
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && this.lastEquipmentUse.timeSince >= 10f)
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.Fruit:
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .5)
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.GainArmor:
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .35 && this.master.GetBody().healthComponent.timeSinceLastHit <= 1.0f)
                    {
                        FireEquipment();
                    }
                    break;
                case EquipmentIndex.Cleanse:
                    if (this.master.GetBody().HasBuff(BuffIndex.OnFire) || this.master.GetBody().HasBuff(BuffIndex.HealingDisabled))
                    {
                        FireEquipment();
                    }
                    break;
            }
        }

        private void FireEquipment()
        {
            if (this.master.GetBody().equipmentSlot.ExecuteIfReady())
            {
                lastEquipmentUse = Run.FixedTimeStamp.now;
            }
        }
    }
}
