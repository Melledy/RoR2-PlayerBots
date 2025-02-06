using EntityStates.AI.Walker;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using PlayerBots.AI;

namespace PlayerBots.Custom
{
    class PlayerBotController : MonoBehaviour
    {
        public CharacterMaster master;
        public CharacterBody body;
        public EntityStateMachine stateMachine;
        public BaseAI ai;
        public Interactor bodyInteractor;
        public Run.FixedTimeStamp lastEquipmentUse;
        public Stage currentStage;

        // Cached variables
        private AISkillDriver customTargetSkillDriver;
        private StageCache stageCache;
        private InfiniteTowerRun infiniteTowerRun;
        private int infiniteTowerWave;

        // AI Skill Helper
        private AiSkillHelper skillHelper;

        public void SetSkillHelper(AiSkillHelper skillHelper)
        {
            this.skillHelper = skillHelper;
            skillHelper.controller = this;
        }

        public void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.ai = base.GetComponent<BaseAI>();
            this.stateMachine = base.GetComponent<EntityStateMachine>();

            if (Run.instance is InfiniteTowerRun) 
            {
                infiniteTowerRun = (InfiniteTowerRun) Run.instance;
            }

            if (ai is PlayerBotBaseAI) {
                customTargetSkillDriver = ai.skillDrivers.First(driver => driver.customName.Equals("CustomTargetLeash"));
                body = master.GetBody();
                bodyInteractor = master.GetBody().GetComponent<Interactor>();
                this.stageCache = new StageCache();
            }
        }

        public bool CanInteract()
        {
            return customTargetSkillDriver != null && PlayerBotManager.BotsUseInteractables.Value;
        }

        public void FixedUpdate()
        {
            // Skip if no body object or if bot is dead
            if (!this.master.GetBody() || this.master.IsDeadAndOutOfLivesServer())
            {
                return;
            }
            // Fix bunny hopping
            this.ai.localNavigator.SetFieldValue("walkFrustration", 0f);
            // Check if body interactor has changed
            if (this.body != this.master.GetBody())
            {
                this.body = master.GetBody();
                this.bodyInteractor = master.GetBody().GetComponent<Interactor>();
                this.skillHelper.OnBodyChange();
            }
            // Remove the default combat delay with ai
            if (this.stateMachine.state is Combat)
            {
                ((Combat) this.stateMachine.state).SetFieldValue("aiUpdateTimer", 0);
            }
            // Infinite tower override (Simulacrum)
            if (infiniteTowerRun)
            {
                InfiniteTowerRunLogic();
            }
            else 
            {
                // Use interactables
                if (CanInteract())
                {
                    // Check if stage has changed
                    if (Stage.instance != this.currentStage || this.currentStage == null)
                    {
                        this.currentStage = Stage.instance;
                        try
                        {
                            this.stageCache.Update();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    // Clear
                    this.ai.customTarget.gameObject = null;
                    this.customTargetSkillDriver.minDistance = 0;
                    // Pickups
                    PickupItems();
                    // Check interactables
                    CheckInteractables();
                    // Force custom skill driver if not in combat
                    ForceCustomSkillDriver();
                }
                else if (PlayerBotManager.allRealPlayersDead && PlayerBotManager.ContinueAfterDeath.Value)
                {
                    // Clear
                    this.ai.customTarget.gameObject = null;
                    this.customTargetSkillDriver.minDistance = 0;
                    // Force bot to use teleporter after player dies
                    CheckTeleporter();
                    // Force custom skill driver if not in combat
                    ForceCustomSkillDriver();
                }
            }
            // Check if bot should use equipment
            ProcessEquipment();
            // Run skill helper callback
            this.skillHelper.OnFixedUpdate();
        }

        public void InfiniteTowerRunLogic() 
        {
            // Safe ward cached variable
            InfiniteTowerSafeWardController safeWardController = infiniteTowerRun.GetFieldValue<InfiniteTowerSafeWardController>("safeWardController");
            // Respawn after each wave
            if (infiniteTowerWave != infiniteTowerRun.waveIndex)
            {
                infiniteTowerWave = infiniteTowerRun.waveIndex;
                if (PlayerBotManager.RespawnAfterWave.Value && this.master.IsDeadAndOutOfLivesServer()) 
                {
                    if (safeWardController)
                        this.master.Respawn(safeWardController.transform.position, safeWardController.transform.rotation);
                    else
                        this.master.Respawn(this.master.transform.position, this.master.transform.rotation);
                }
            }
            // Clear
            this.ai.customTarget.gameObject = null;
            this.customTargetSkillDriver.minDistance = 0;
            if (safeWardController) 
            {
                this.ai.customTarget.gameObject = safeWardController.gameObject;
            }
        }

        public void PickupItems()
        {
            if (!master.inventory)
            {
                return;
            }

            List<GenericPickupController> pickups = InstanceTracker.GetInstancesList<GenericPickupController>();

            for (int i = 0; i < pickups.Count; i++)
            {
                GenericPickupController pickup = pickups[i];

                // Skip lunar coins
                if (PickupCatalog.GetPickupDef(pickup.pickupIndex).coinValue > 0)
                {
                    continue;
                }

                // Skip these
                ItemDef def = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickup.pickupIndex).itemIndex);
                if (def != null && def.tier == ItemTier.Lunar)
                {
                    continue;
                }
                EquipmentIndex equipmentIndex = PickupCatalog.GetPickupDef(pickup.pickupIndex).equipmentIndex;
                if (equipmentIndex != EquipmentIndex.None)
                {
                    if (EquipmentCatalog.GetEquipmentDef(equipmentIndex).isLunar)
                    {
                        continue;
                    }
                    if (master.inventory.currentEquipmentIndex != EquipmentIndex.None)
                    {
                        continue;
                    }
                }

                if (pickup.GetInteractability(this.bodyInteractor) == Interactability.Available)
                {
                    // Move to pickup item if within 60 meters
                    float dist = PlayerBotUtils.GetFastDist(master.GetBody().transform.position, pickup.gameObject.transform.position);
                    if (dist <= (60f * 60f))
                    {
                        this.ai.customTarget.gameObject = pickup.gameObject;
                        return;
                    }
                }
            }
        }

        private void CheckInteractables()
        {
            if (this.currentStage == null || !this.master.isActiveAndEnabled || this.ai.customTarget.gameObject != null)
            {
                return;
            }

            GameObject closest = null;
            float closestDist = 0f;

            // Chests, etc
            for (var i = this.stageCache.interactablesItem.Count - 1; i >= 0; i--)
            {
                BotInteractable<PurchaseInteraction> interactable = this.stageCache.interactablesItem[i];
                Interactability interactability = interactable.Value.GetInteractability(this.bodyInteractor);
                if (interactability == Interactability.Available)
                {
                    if (master.money < interactable.Value.cost)
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(this.master.GetBody().transform.position, interactable.gameObject.transform.position);
                    if (dist <= bodyInteractor.maxInteractionDistance)
                    {
                        bodyInteractor.AttemptInteraction(interactable.gameObject);
                    }
                    if (closest == null || dist < closestDist)
                    {
                        closest = interactable.gameObject;
                        closestDist = dist;
                    }
                }
                else if (interactability == Interactability.Disabled)
                {
                    this.stageCache.interactablesItem.RemoveAt(i);
                }
            }

            // Teleporter
            if (CheckTeleporter())
            {
                return;
            }

            if (closest)
            {
                this.ai.customTarget.gameObject = closest;
                this.ai.customTarget.Update();
            }
        }

        private bool CheckTeleporter()
        {
            if (TeleporterInteraction.instance)
            {
                // Skip if bots can interact + iteractables on the map
                if (CanInteract() && this.stageCache.interactablesItem.Count > 0)
                {
                    return false;
                }
                // State checking
                if (TeleporterInteraction.instance.isCharging)
                {
                    this.ai.customTarget.gameObject = TeleporterInteraction.instance.gameObject;
                    this.customTargetSkillDriver.minDistance = TeleporterInteraction.instance.holdoutZoneController.currentRadius;
                }
                if (TeleporterInteraction.instance.isIdle || (TeleporterInteraction.instance.isCharged && PlayerBotManager.allRealPlayersDead))
                {
                    // Try to activate teleporter
                    float dist = Vector3.Distance(this.master.GetBody().transform.position, TeleporterInteraction.instance.gameObject.transform.position);
                    if (dist <= bodyInteractor.maxInteractionDistance)
                    {
                        bodyInteractor.AttemptInteraction(TeleporterInteraction.instance.gameObject);
                    }
                    // Move to teleporter
                    this.ai.customTarget.gameObject = TeleporterInteraction.instance.gameObject;
                    return true;
                }
            }
            return false;
        }

        private void ForceCustomSkillDriver()
        {
            // Activate skill driver if not in combat
            if (master.GetBody() != null && master.GetBody().outOfCombat && master.GetBody().outOfDanger && this.ai.customTarget.gameObject != null && this.ai.skillDriverEvaluation.dominantSkillDriver != this.customTargetSkillDriver)
            {
                this.ai.skillDriverEvaluation = new BaseAI.SkillDriverEvaluation
                {
                    dominantSkillDriver = this.customTargetSkillDriver,
                    target = this.ai.customTarget,
                    aimTarget = this.ai.customTarget,
                    separationSqrMagnitude = float.PositiveInfinity
                };
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
                if (e == RoR2Content.Equipment.CommandMissile.equipmentIndex || e == RoR2Content.Equipment.Lightning.equipmentIndex || e == RoR2Content.Equipment.DeathProjectile.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS)
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.BFG.equipmentIndex)
                {
                    // Dont spam preon at random mobs
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.characterBody != null && this.ai.currentEnemy.hasLoS &&
                              (this.ai.currentEnemy.characterBody.isBoss || (this.master.GetBody().equipmentSlot.stock > 1 && this.ai.currentEnemy.characterBody.isElite)))
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.CritOnUse.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(RoR2Content.Buffs.FullCrit.buffIndex))
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
                else if (e == RoR2Content.Equipment.TeamWarCry.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.HasBuff(RoR2Content.Buffs.TeamWarCry.buffIndex))
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.Blackhole.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && this.lastEquipmentUse.timeSince >= 10f)
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.LifestealOnHit.equipmentIndex || e == RoR2Content.Equipment.PassiveHealing.equipmentIndex || e == RoR2Content.Equipment.Fruit.equipmentIndex)
                {
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .5 && !this.HasBuff(RoR2Content.Buffs.HealingDisabled.buffIndex))
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.GainArmor.equipmentIndex)
                {
                    if (this.master.GetBody().healthComponent.combinedHealthFraction <= .35 && this.master.GetBody().healthComponent.timeSinceLastHit <= 1.0f)
                    {
                        FireEquipment();
                    }
                }
                else if (e == RoR2Content.Equipment.Cleanse.equipmentIndex)
                {
                    if (this.HasBuff(RoR2Content.Buffs.OnFire.buffIndex) || this.HasBuff(RoR2Content.Buffs.HealingDisabled.buffIndex))
                    {
                        FireEquipment();
                    }
                }
                else if (e == DLC1Content.Equipment.Molotov.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && !this.ai.currentEnemy.characterBody.isFlying && this.lastEquipmentUse.timeSince >= 10f)
                    {
                        FireEquipment();
                    }
                }
                else if (e == DLC1Content.Equipment.BossHunter.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && this.ai.currentEnemy.characterBody.isBoss)
                    {
                        FireEquipment();
                    }
                }
                else if (e == DLC1Content.Equipment.GummyClone.equipmentIndex)
                {
                    if (this.ai.currentEnemy != null && this.ai.currentEnemy.hasLoS && this.master.GetBody().healthComponent.combinedHealthFraction <= .9 && this.lastEquipmentUse.timeSince >= 5f)
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
