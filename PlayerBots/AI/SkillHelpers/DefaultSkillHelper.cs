using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI.SkillHelpers
{
    [SkillHelperSurvivor(SurvivorIndex.None)]
    class DefaultSkillHelper : AiSkillHelper
    {
        public override void InjectSkills(GameObject gameObject, BaseAI ai)
        {
            // Skill
            AISkillDriver skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "Shoot";
            skill.skillSlot = RoR2.SkillSlot.Primary;
            skill.requireSkillReady = true;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = 0;
            skill.maxDistance = 50;
            skill.selectionRequiresTargetLoS = true;
            skill.activationRequiresTargetLoS = true;
            skill.activationRequiresAimConfirmation = true;
            skill.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = false;

            // Default skills
            AddDefaultSkills(gameObject, ai, 20);
        }
    }
}
