using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers.Custom
{
    [SkillHelperSurvivor("Bandit2Body")]
    class BanditHelper : AiSkillHelper
    {
        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            AISkillDriver skill3 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill3.customName = "Utility";
            skill3.skillSlot = RoR2.SkillSlot.Utility;
            skill3.requireSkillReady = true;
            skill3.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill3.minDistance = 0;
            skill3.maxDistance = 80;
            skill3.maxUserHealthFraction = .4f;
            skill3.selectionRequiresTargetLoS = true;
            skill3.activationRequiresTargetLoS = false;
            skill3.activationRequiresAimConfirmation = false;
            skill3.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            skill3.aimType = AISkillDriver.AimType.MoveDirection;
            skill3.ignoreNodeGraph = false;
            skill3.resetCurrentEnemyOnNextDriverSelection = false;
            skill3.noRepeat = false;
            skill3.shouldSprint = true;

            AISkillDriver skill2 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill2.customName = "Secondary";
            skill2.skillSlot = RoR2.SkillSlot.Secondary;
            skill2.requireSkillReady = true;
            skill2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill2.minDistance = 0;
            skill2.maxDistance = 5;
            skill2.selectionRequiresTargetLoS = true;
            skill2.activationRequiresTargetLoS = true;
            skill2.activationRequiresAimConfirmation = true;
            skill2.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill2.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill2.ignoreNodeGraph = true;
            skill2.resetCurrentEnemyOnNextDriverSelection = false;
            skill2.noRepeat = false;
            skill2.shouldSprint = false;

            AISkillDriver skill3_alt = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill3_alt.customName = "Utility Alt";
            skill3_alt.skillSlot = RoR2.SkillSlot.Utility;
            skill3_alt.requireSkillReady = true;
            skill3_alt.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill3_alt.minDistance = 0;
            skill3_alt.maxDistance = 5;
            skill3_alt.selectionRequiresTargetLoS = true;
            skill3_alt.activationRequiresTargetLoS = false;
            skill3_alt.activationRequiresAimConfirmation = false;
            skill3_alt.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            skill3_alt.aimType = AISkillDriver.AimType.MoveDirection;
            skill3_alt.ignoreNodeGraph = false;
            skill3_alt.resetCurrentEnemyOnNextDriverSelection = false;
            skill3_alt.noRepeat = false;
            skill3_alt.shouldSprint = true;

            AISkillDriver skill4 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill4.customName = "Special";
            skill4.skillSlot = RoR2.SkillSlot.Special;
            skill4.requireSkillReady = true;
            skill4.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill4.minDistance = 0;
            skill4.maxDistance = 60;
            skill4.selectionRequiresTargetLoS = true;
            skill4.activationRequiresTargetLoS = true;
            skill4.activationRequiresAimConfirmation = true;
            skill4.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill4.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill4.ignoreNodeGraph = false;
            skill4.resetCurrentEnemyOnNextDriverSelection = false;
            skill4.noRepeat = true;
            skill4.shouldSprint = false;

            AISkillDriver skill1 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill1.customName = "Shoot";
            skill1.skillSlot = RoR2.SkillSlot.Primary;
            skill1.requireSkillReady = true;
            skill1.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill1.minDistance = 0;
            skill1.maxDistance = 50;
            skill1.selectionRequiresTargetLoS = true;
            skill1.activationRequiresTargetLoS = true;
            skill1.activationRequiresAimConfirmation = true;
            skill1.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill1.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skill1.ignoreNodeGraph = false;
            skill1.resetCurrentEnemyOnNextDriverSelection = false;
            skill1.noRepeat = false;
            skill1.shouldSprint = false;
            skill1.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            // Add default skills
            AddDefaultSkills(gameObject, ai, 10);
        }
    }
}
