using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor("VoidSurvivorBody")]
    class VoidSurvivorHelper : AiSkillHelper
    {
        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            AISkillDriver skill3_chase = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill3_chase.customName = "UtilityChase";
            skill3_chase.skillSlot = RoR2.SkillSlot.Utility;
            skill3_chase.requireSkillReady = true;
            skill3_chase.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill3_chase.minDistance = 50;
            skill3_chase.maxDistance = 100;
            skill3_chase.selectionRequiresTargetLoS = true;
            skill3_chase.activationRequiresTargetLoS = true;
            skill3_chase.activationRequiresAimConfirmation = false;
            skill3_chase.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill3_chase.aimType = AISkillDriver.AimType.MoveDirection;
            skill3_chase.ignoreNodeGraph = false;
            skill3_chase.resetCurrentEnemyOnNextDriverSelection = false;
            skill3_chase.noRepeat = false;
            skill3_chase.shouldSprint = true;

            AISkillDriver skill3 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill3.customName = "UtilityDefensive";
            skill3.skillSlot = RoR2.SkillSlot.Utility;
            skill3.requireSkillReady = true;
            skill3.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill3.minDistance = 0;
            skill3.maxDistance = 20;
            //skill3.maxUserHealthFraction = .25f;
            skill3.selectionRequiresTargetLoS = true;
            skill3.activationRequiresTargetLoS = false;
            skill3.activationRequiresAimConfirmation = false;
            skill3.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            skill3.aimType = AISkillDriver.AimType.MoveDirection;
            skill3.ignoreNodeGraph = false;
            skill3.resetCurrentEnemyOnNextDriverSelection = false;
            skill3.noRepeat = false;
            skill3.shouldSprint = true;
            skill3.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            AISkillDriver skill2 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill2.customName = "Secondary";
            skill2.skillSlot = RoR2.SkillSlot.Secondary;
            skill2.requireSkillReady = true;
            skill2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill2.minDistance = 0;
            skill2.maxDistance = 50;
            skill2.selectionRequiresTargetLoS = true;
            skill2.activationRequiresTargetLoS = true;
            skill2.activationRequiresAimConfirmation = true;
            skill2.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill2.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill2.ignoreNodeGraph = false;
            skill2.resetCurrentEnemyOnNextDriverSelection = false;
            skill2.noRepeat = false;
            skill2.shouldSprint = false;
            skill2.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            // Skills
            AISkillDriver skill1 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill1.customName = "Shoot";
            skill1.skillSlot = RoR2.SkillSlot.Primary;
            skill1.requireSkillReady = true;
            skill1.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill1.minDistance = 0;
            skill1.maxDistance = 40;
            skill1.selectionRequiresTargetLoS = true;
            skill1.activationRequiresTargetLoS = true;
            skill1.activationRequiresAimConfirmation = true;
            skill1.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill1.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill1.ignoreNodeGraph = false;
            skill1.resetCurrentEnemyOnNextDriverSelection = false;
            skill1.noRepeat = false;
            skill1.shouldSprint = false;
            skill1.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            // Add default skills
            AddDefaultSkills(gameObject, ai, 20);
        }
    }
}
