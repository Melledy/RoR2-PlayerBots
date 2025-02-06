using IL.RoR2.Achievements.Bandit2;
using PlayerBots.Custom;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor("SeekerBody")]
    class SeekerHelper : AiSkillHelper
    {
        private SeekerController seekerController;
        private Run.FixedTimeStamp meditationLastCheck;
        private float meditationStepDelay;

        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            // Init
            AISkillDriver skill;

            // Class skills
            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Meditate";
            skill.skillSlot = RoR2.SkillSlot.Special;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            skill.maxTargetHealthFraction = 0.7f;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.Stop;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = true;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = true;
            skill.shouldSprint = false;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Meditate (Self)";
            skill.skillSlot = RoR2.SkillSlot.Special;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            skill.maxUserHealthFraction = 0.7f;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.Stop;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = true;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = true;
            skill.shouldSprint = false;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Sojourn";
            skill.skillSlot = RoR2.SkillSlot.Utility;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 50;
            skill.minUserHealthFraction = 0.6f;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = true;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Unseen Hand";
            skill.skillSlot = RoR2.SkillSlot.Secondary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 30;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Spirit Punch";
            skill.skillSlot = RoR2.SkillSlot.Primary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 40;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;
            skill.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            // Default skills
            AddDefaultSkills(gameObject, ai, 20);
        }

        // Events

        public override void OnBodyChange()
        {
            this.seekerController = null;
        }

        public override void OnFixedUpdate()
        {
            // Set seeker controller
            if (this.seekerController == null)
            {
                this.seekerController = controller.body.GetComponent<SeekerController>();
                this.meditationLastCheck = Run.FixedTimeStamp.now;
            }
            // Custom behaviors
            if (this.seekerController != null)
            {
                // Force meditation inputs
                var step = this.seekerController.meditationInputStep + 1;
                if (step < this.seekerController.meditationStepAndSequence.Length && this.meditationLastCheck.timeSince >= this.meditationStepDelay)
                {
                    this.seekerController.meditationInputStep = (sbyte) step;
                    this.meditationLastCheck = Run.FixedTimeStamp.now;
                    this.meditationStepDelay = Random.Range(0.25f, 0.75f);
                }
            }
        }

    }
}
