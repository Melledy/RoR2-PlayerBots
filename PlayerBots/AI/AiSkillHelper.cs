using RoR2.CharacterAI;
using UnityEngine;

namespace PlayerBots.AI
{
    abstract class AiSkillHelper
    {
        public abstract void InjectSkills(GameObject gameObject, BaseAI ai);

        public AISkillDriver AddCustomTargetLeash(GameObject gameObject, BaseAI ai)
        {
            AISkillDriver skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "CustomTargetLeash";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.Custom;
            skill.minDistance = 100;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = true;
            skill.driverUpdateTimerOverride = 3;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            return skill;
        }

        public void AddDefaultSkills(GameObject gameObject, BaseAI ai)
        {
            AISkillDriver skill;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "WaitNearCustomTarget";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.Custom;
            skill.minDistance = 0;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.Stop;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "ReturnToOwnerLeash";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            skill.minDistance = 50;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = true;
            skill.driverUpdateTimerOverride = 3;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "ChaseEnemy";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skill.minDistance = ai.minDistanceFromEnemy;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.MoveDirection;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "ReturnToLeader";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            skill.minDistance = 15;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;

            skill = gameObject.AddComponent<AISkillDriver>() as AISkillDriver;
            skill.customName = "WaitNearLeader";
            skill.skillSlot = RoR2.SkillSlot.None;
            skill.requireSkillReady = false;
            skill.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            skill.minDistance = 0;
            skill.maxDistance = float.PositiveInfinity;
            skill.selectionRequiresTargetLoS = false;
            skill.activationRequiresTargetLoS = false;
            skill.activationRequiresAimConfirmation = false;
            skill.movementType = AISkillDriver.MovementType.Stop;
            skill.aimType = AISkillDriver.AimType.AtMoveTarget;
            skill.ignoreNodeGraph = false;
            skill.resetCurrentEnemyOnNextDriverSelection = false;
            skill.noRepeat = false;
            skill.shouldSprint = true;
        }
    }
}
