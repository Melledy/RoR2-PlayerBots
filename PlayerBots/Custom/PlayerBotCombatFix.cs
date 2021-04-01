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
        private Stage currentStage;

        public void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.ai = base.GetComponent<BaseAI>();
            this.stateMachine = base.GetComponent<EntityStateMachine>();
        }

        public void FixedUpdate()
        {
            // Check if stage has changed
            if (Stage.instance != this.currentStage)
            {
                this.currentStage = Stage.instance;
                if (this.currentStage.sceneDef.baseSceneName.Equals("moon"))
                {
                    ChildLocator childLocator = SceneInfo.instance.GetComponent<ChildLocator>();
                    if (childLocator)
                    {
                        Transform transform = childLocator.FindChild("CenterOfArena");
                        if (transform)
                        {
                            ai.customTarget.gameObject = transform.gameObject;
                            TeleportHelper.TeleportBody(master.GetBody(), transform.position);
                        }
                    }
                }
            }
            // Fix bunny hopping
            this.ai.localNavigator.SetFieldValue("walkFrustration", 0f);
            // Remove the default combat delay with ai
            if (this.stateMachine.state is Combat)
            {
                ((Combat)this.stateMachine.state).SetFieldValue("aiUpdateTimer", 0);
            }
            // Equipment
            if (this.master.GetBody() && !this.master.IsDeadAndOutOfLivesServer())
            {
                ProcessEquipment();
            }
        }

        private bool HasBuff(BuffIndex buff)
        {
            return this.master.GetBody().HasBuff(buff);
        }

        private void ProcessEquipment()
        {
            if (this.master.inventory.currentEquipmentIndex == EquipmentIndex.None || this.master.GetBody().equipmentSlot.stock == 0)
            {
                return;
            }

            try
            {
                // TODO Cleanup later
                EquipmentIndex e = this.master.inventory.currentEquipmentIndex;
                if (e == IndexManager.CommandMissile || e == IndexManager.Lightning || e == IndexManager.DeathProjectile)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS)
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.BFG)
                {
                    // Dont spam preon at random mobs
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.characterBody != null && this.ai.currentEnemy.hasLoS &&
                              (this.ai.currentEnemy.characterBody.isBoss || (this.master.GetBody().equipmentSlot.stock > 1 && this.ai.currentEnemy.characterBody.isElite)))
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.CritOnUse)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(IndexManager.BuffFullCrit))
                    {
                        if (this.master.GetBody().crit >= 100f)
                        {
                            // 100% crit already - no need for this item anymore
                            this.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
                        }
                        else
                        {
                            FireEquipment();
                        }
                    }
                }
                else if (e == IndexManager.TeamWarCry)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(IndexManager.BuffTeamWarCry))
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.Blackhole)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && this.lastEquipmentUse.timeSince >= 10f)
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.LifestealOnHit || e == IndexManager.PassiveHealing || e == IndexManager.Fruit)
                {
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .5 && !this.HasBuff(IndexManager.BuffHealingDisabled))
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.GainArmor)
                {
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .35 && this.master.GetBody().healthComponent.timeSinceLastHit <= 1.0f)
                    {
                        FireEquipment();
                    }
                }
                else if (e == IndexManager.Cleanse)
                {
                    if (this.HasBuff(IndexManager.BuffOnFire) || this.HasBuff(IndexManager.BuffHealingDisabled))
                    {
                        FireEquipment();
                    }
                }
                else 
                {
                    this.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.Log("Error when bot is using: " + this.master.inventory.currentEquipmentIndex);
                this.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
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
