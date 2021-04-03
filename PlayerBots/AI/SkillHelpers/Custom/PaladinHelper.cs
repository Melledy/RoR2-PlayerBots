using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor("RobPaladinBody")]
    [CustomSurvivor("https://thunderstore.io/package/rob/PaladinMod/", "1.4.3")]
    class PaladinHelper : AiSkillHelper
    {
        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            AISkillDriver skill4 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill4.customName = "Special";
            skill4.skillSlot = RoR2.SkillSlot.Special;
            skill4.requireSkillReady = true;
            skill4.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            skill4.minDistance = 0;
            skill4.maxDistance = 50;
            skill4.minTargetHealthFraction = float.NegativeInfinity;
            skill4.maxTargetHealthFraction = .7f;
            skill4.selectionRequiresTargetLoS = false;
            skill4.activationRequiresTargetLoS = false;
            skill4.activationRequiresAimConfirmation = true;
            skill4.movementType = AISkillDriver.MovementType.Stop;
            skill4.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill4.ignoreNodeGraph = false;
            skill4.resetCurrentEnemyOnNextDriverSelection = false;
            skill4.noRepeat = true;
            skill4.shouldSprint = false;
            skill4.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            skill4.driverUpdateTimerOverride = 5f;

            AISkillDriver skill3 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill3.customName = "Utility";
            skill3.skillSlot = RoR2.SkillSlot.Utility;
            skill3.requireSkillReady = true;
            skill3.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill3.minDistance = 0;
            skill3.maxDistance = 50;
            skill3.selectionRequiresTargetLoS = true;
            skill3.activationRequiresTargetLoS = true;
            skill3.activationRequiresAimConfirmation = false;
            skill3.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill3.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill3.ignoreNodeGraph = false;
            skill3.resetCurrentEnemyOnNextDriverSelection = false;
            skill3.noRepeat = false;
            skill3.shouldSprint = true;

            AISkillDriver skill1_alt = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill1_alt.customName = "PrimaryBeam";
            skill1_alt.skillSlot = RoR2.SkillSlot.Primary;
            skill1_alt.requireSkillReady = true;
            skill1_alt.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill1_alt.minDistance = 0;
            skill1_alt.maxDistance = 30;
            skill1_alt.minUserHealthFraction = .9f;
            skill1_alt.selectionRequiresTargetLoS = true;
            skill1_alt.activationRequiresTargetLoS = true;
            skill1_alt.activationRequiresAimConfirmation = true;
            skill1_alt.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill1_alt.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill1_alt.ignoreNodeGraph = true;
            skill1_alt.resetCurrentEnemyOnNextDriverSelection = false;
            skill1_alt.noRepeat = false;
            skill1_alt.shouldSprint = false;

            AISkillDriver skill2 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill2.customName = "Secondary";
            skill2.skillSlot = RoR2.SkillSlot.Secondary;
            skill2.requireSkillReady = true;
            skill2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill2.minDistance = 0;
            skill2.maxDistance = 15;
            skill2.selectionRequiresTargetLoS = true;
            skill2.activationRequiresTargetLoS = true;
            skill2.activationRequiresAimConfirmation = true;
            skill2.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill2.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill2.ignoreNodeGraph = false;
            skill2.resetCurrentEnemyOnNextDriverSelection = false;
            skill2.noRepeat = false;
            skill2.shouldSprint = false;

            AISkillDriver chaseSkill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            chaseSkill.customName = "ChaseTarget";
            chaseSkill.skillSlot = RoR2.SkillSlot.None;
            chaseSkill.requireSkillReady = false;
            chaseSkill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseSkill.minDistance = 10;
            chaseSkill.maxDistance = 60;
            chaseSkill.selectionRequiresTargetLoS = true;
            chaseSkill.activationRequiresTargetLoS = true;
            chaseSkill.activationRequiresAimConfirmation = false;
            chaseSkill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseSkill.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseSkill.ignoreNodeGraph = false;
            chaseSkill.resetCurrentEnemyOnNextDriverSelection = false;
            chaseSkill.noRepeat = false;
            chaseSkill.shouldSprint = true;

            AISkillDriver skill1 = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill1.customName = "Primary";
            skill1.skillSlot = RoR2.SkillSlot.Primary;
            skill1.requireSkillReady = true;
            skill1.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill1.minDistance = 0;
            skill1.maxDistance = 10;
            skill1.selectionRequiresTargetLoS = true;
            skill1.activationRequiresTargetLoS = true;
            skill1.activationRequiresAimConfirmation = false;
            skill1.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill1.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill1.ignoreNodeGraph = true;
            skill1.resetCurrentEnemyOnNextDriverSelection = false;
            skill1.noRepeat = false;
            skill1.shouldSprint = false;

            // Add default skills
            AddDefaultSkills(gameObject, ai, 0);
        }
    }
}
