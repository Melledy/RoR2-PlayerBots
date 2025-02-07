using EntityStates.Railgunner.Reload;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor("RailgunnerBody")]
    class RailgunnerHelper : AiSkillHelper
    {
        private EntityStateMachine reload;

        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            // Init
            AISkillDriver skill;

            // Class skills
            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Utility";
            skill.skillSlot = RoR2.SkillSlot.Utility;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 15;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            skill.aimType =  AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = true;
            skill.noRepeat = true;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Special";
            skill.skillSlot = RoR2.SkillSlot.Special;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 60;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = true;
            skill.shouldSprint = false;
            AISkillDriver special = skill;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Secondary";
            skill.skillSlot = RoR2.SkillSlot.Secondary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 15;
            skill.maxDistance = 60;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;
            skill.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            AISkillDriver secondary = skill;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Shoot";
            skill.skillSlot = RoR2.SkillSlot.Primary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 60;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;
            skill.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            // Set overrides
            secondary.nextHighPriorityOverride = skill;
            special.nextHighPriorityOverride = skill;

            // Default skills
            AddDefaultSkills(gameObject, ai, 20);
        }

        // Events

        public override void OnBodyChange()
        {
            this.reload = null;
        }

        public override void OnFixedUpdate()
        {
            // Set seeker controller
            if (this.reload == null)
            {
                this.reload = controller.body.GetComponentsInChildren<EntityStateMachine>()
                    .ToList()
                    .Find(esm => esm.customName.Equals("Reload"));
            }
            // Force rail gunner to activate boost
            if (this.reload != null && this.reload.state is Reloading)
            {
                Reloading state = (Reloading) this.reload.state;
                if (state.IsInBoostWindow())
                {
                    int chance = Random.Range(1, 6);
                    if (chance == 1)
                    {
                        state.AttemptBoost();
                    }
                }
            }
        }
    }
}
