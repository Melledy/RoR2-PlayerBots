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
                switch (this.master.inventory.currentEquipmentIndex)
                {
                    case EquipmentIndex.CommandMissile:
                    case EquipmentIndex.Lightning:
                    case EquipmentIndex.DeathProjectile:
                        if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS)
                        {
                            FireEquipment();
                        }
                        break;
                    case EquipmentIndex.BFG: // Dont spam preon at random mobs
                        if (this.ai.currentEnemy != null && this.ai.currentEnemy.characterBody != null && this.ai.currentEnemy.hasLoS &&
                            (this.ai.currentEnemy.characterBody.isBoss || (this.master.GetBody().equipmentSlot.stock > 1 && this.ai.currentEnemy.characterBody.isElite)))
                        {
                            FireEquipment();
                        }
                        break;
                    case EquipmentIndex.CritOnUse:
                        if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(BuffIndex.FullCrit))
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
                        break;
                    case EquipmentIndex.TeamWarCry:
                        if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(BuffIndex.TeamWarCry))
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
                    case EquipmentIndex.LifestealOnHit:
                    case EquipmentIndex.PassiveHealing:
                    case EquipmentIndex.Fruit:
                        if (this.master.GetBody().healthComponent.combinedHealthFraction <= .5 && !this.HasBuff(BuffIndex.HealingDisabled))
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
                        if (this.HasBuff(BuffIndex.OnFire) || this.HasBuff(BuffIndex.HealingDisabled))
                        {
                            FireEquipment();
                        }
                        break;
                    default:
                        this.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
                        break;
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
