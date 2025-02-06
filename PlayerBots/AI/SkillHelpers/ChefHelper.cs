using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor("ChefBody")]
    class ChefHelper : AiSkillHelper
    {
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
            skill.maxDistance = 30;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.MoveDirection;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Special";
            skill.skillSlot = RoR2.SkillSlot.Special;
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
            skill.noRepeat = true;
            skill.shouldSprint = false;
            skill.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Secondary";
            skill.skillSlot = RoR2.SkillSlot.Secondary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 20;
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
            skill.customName = "Primary";
            skill.skillSlot = RoR2.SkillSlot.Primary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 25;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;
            skill.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            // Default skills
            AddDefaultSkills(gameObject, ai, 0);
        }
    }
}
