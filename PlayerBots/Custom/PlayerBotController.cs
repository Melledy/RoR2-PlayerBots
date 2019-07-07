using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayerBots.Custom
{
    class PlayerBotController : MonoBehaviour
    {
        public CharacterMaster master;
        public PlayerBotBaseAI baseAI;

        public PingerController pinger;

        public void Awake()
        {
            this.master = base.gameObject.GetComponent<CharacterMaster>();
            this.baseAI = base.gameObject.GetComponent<PlayerBotBaseAI>();
            this.pinger = base.gameObject.GetComponent<PingerController>();
        }

        public void FixedUpdate()
        {
            // Bad
            /*
            if (BossGroup.GetTotalBossCount() > 0)
            {
                BaseAI.Target enemy = (BaseAI.Target)typeof(PlayerBotBaseAI).BaseType.GetField("currentEnemy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(baseAI);

                if (enemy.characterBody && enemy.characterBody.isBoss)
                {
                    return;
                }

                // Target random boss
                ReadOnlyCollection<CharacterMaster> bosses = BossGroup.instance.combatSquad.readOnlyMembersList;
                if (bosses.Count > 0)
                {
                    foreach (CharacterMaster bossMaster in bosses)
                    {
                        if (bossMaster.GetBody())
                        {
                            enemy.gameObject = bossMaster.GetBody().gameObject;
                            enemy.bestHurtBox = bossMaster.GetBody().mainHurtBox;
                            // Debug
                            PingHurtBox(enemy.bestHurtBox);
                            break;
                        }
                    }
                }

            }
            */
        }

        public void PingHurtBox(HurtBox hurtBox)
        {
            if (this.pinger)
            {
                PingerController.PingInfo pingInfo = new PingerController.PingInfo
                {
                    active = true
                };

                Transform transform =hurtBox.healthComponent.transform;
                pingInfo.origin = transform.position;
                pingInfo.targetNetworkIdentity = hurtBox.healthComponent.GetComponent<NetworkIdentity>();

                Reflection.InvokeMethod(this.pinger, "SetCurrentPing", new object[] {
                                    pingInfo
                                });
            }
        }
    }
}
