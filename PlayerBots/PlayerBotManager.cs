using BepInEx;
using BepInEx.Configuration;
using PlayerBots.AI;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayerBots
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.meledy.PlayerBots", "PlayerBots", "0.0.4")]
    public class PlayerBotManager : BaseUnityPlugin
    {
        public static List<GameObject> playerbots = new List<GameObject>();
        public static List<string> bodyList = new List<string> {
            "CommandoBody",
            "ToolbotBody",
            "HuntressBody",
            "EngiBody",
            "MageBody",
            "MercBody",
        };

        public static ConfigWrapper<int> MaxBotPurchasesPerStage { get; set; }
        private static ConfigWrapper<bool> AutoPurchaseItems { get; set; }
        private static ConfigWrapper<bool> HostOnlySpawnBots { get; set; }
        private static ConfigWrapper<bool> ShowNameplates { get; set; }
        private static ConfigWrapper<bool> TreatBotsAsPlayers { get; set; }

        public void Awake()
        {
            // Config
            AutoPurchaseItems = Config.Wrap("Bot Inventory", "AutoPurchaseItems", "Maximum amount of putchases a playerbot can do per stage.", true);
            MaxBotPurchasesPerStage = Config.Wrap("Bot Inventory", "MaxBotPurchasesPerStage", "Maximum amount of putchases a playerbot can do per stage.", 8);

            HostOnlySpawnBots = Config.Wrap("Misc", "HostOnlySpawnBots", "Set true so that only the host may spawn bots", true);
            ShowNameplates = Config.Wrap("Misc", "ShowNameplates", "Show player nameplates on playerbots.", true);

            TreatBotsAsPlayers = Config.Wrap("Experimental", "TreatBotsAsPlayers", "VERY BUGGY. ENABLE AT YOUR OWN RISK. Makes the game treat playerbots like how regular players are treated. The bots now show up on the scoreboard, can pick up items, influence the map scaling, etc.", false);

            // Hooks
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            // Ugh.
            On.RoR2.CharacterAI.BaseAI.OnBodyLost += (orig, self) =>
            {
                if (self.name.Equals("PlayerBot"))
                {
                    return;
                }
                orig(self);
            };

            // Super hacky but it works
            if (ShowNameplates.Value)
            {
                On.RoR2.TeamComponent.SetupIndicator += (orig, self) =>
                {
                    CharacterBody component = self.GetComponent<CharacterBody>();
                    if (component && component.master && component.master.name.Equals("PlayerBot"))
                    {
                        PlayerCharacterMasterController playerMaster = component.master.gameObject.AddComponent<PlayerCharacterMasterController>() as PlayerCharacterMasterController;
                        orig(self);
                        Destroy(playerMaster);
                    }
                    else
                    {
                        orig(self);
                    }
                };
            }

            if (!TreatBotsAsPlayers.Value && AutoPurchaseItems.Value)
            {
                // Give bots money
                On.RoR2.TeamManager.GiveTeamMoney += (orig, self, teamIndex, money) =>
                {
                    orig(self, teamIndex, money);

                    if (playerbots.Count > 0)
                    {
                        int num = Run.instance ? Run.instance.livingPlayerCount : 0;
                        if (num != 0)
                        {
                            money = (uint)Mathf.CeilToInt(money / (float)num);
                        }
                        foreach (GameObject playerbot in playerbots)
                        {
                            if (!playerbot)
                            {
                                continue;
                            }
                            CharacterMaster master = playerbot.GetComponent<CharacterMaster>();
                            if (master && master.alive && master.teamIndex == teamIndex)
                            {
                                master.GiveMoney(money);
                            }
                        }
                    }
                };

                On.RoR2.Run.BeginStage += (orig, self) =>
                {
                    foreach (GameObject playerbot in playerbots.ToArray())
                    {
                        if (!playerbot)
                        {
                            playerbots.Remove(playerbot);
                            continue;
                        }

                        ItemManager itemManager = playerbot.GetComponent<ItemManager>();
                        if (itemManager)
                        {
                            itemManager.ResetPurchases();
                            itemManager.master.money = 0;
                        }
                    }
                    orig(self);
                };
            }

            if (TreatBotsAsPlayers.Value)
            {
                On.RoR2.Stage.Start += (orig, self) => {
                    orig(self);
                    if (NetworkServer.active)
                    {
                        foreach (GameObject playerbot in playerbots.ToArray())
                        {
                            if (!playerbot)
                            {
                                playerbots.Remove(playerbot);
                                continue;
                            }

                            CharacterMaster master = playerbot.GetComponent<CharacterMaster>();
                            if (master)
                            {
                                Stage.instance.RespawnCharacter(master);
                            }
                        }
                    }
                };
            }
        }

        // Also really hacky
        public static void SpawnPlayerbot(CharacterBody body, int characterType)
        {
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(bodyList[characterType]);
            if (bodyPrefab == null)
            {
                return;
            }

            SpawnCard card = (SpawnCard)Resources.Load("SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly");
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(card, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = body.transform
            }, RoR2Application.rng);

            if (gameObject)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                AIOwnership aiOwnership = gameObject.AddComponent<AIOwnership>() as AIOwnership;
                BaseAI ai = gameObject.GetComponent<BaseAI>();
                AISkillDriver[] skillDrivers = gameObject.GetComponents<AISkillDriver>();

                PlayerCharacterMasterController playerMaster = null;
                if (TreatBotsAsPlayers.Value)
                {
                    playerMaster = gameObject.AddComponent<PlayerCharacterMasterController>() as PlayerCharacterMasterController;
                }

                if (master)
                {
                    master.bodyPrefab = bodyPrefab;
                    master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);

                    master.teamIndex = TeamComponent.GetObjectTeam(body.gameObject);

                    master.GiveMoney(body.master.money);
                    master.inventory.CopyItemsFrom(body.master.inventory);

                    master.inventory.GiveItem(ItemIndex.DrizzlePlayerHelper, 1);

                    // Allow the bots to spawn in the next stage
                    master.destroyOnBodyDeath = false;
                    master.gameObject.AddComponent<SetDontDestroyOnLoad>();
                }
                if (playerMaster && TreatBotsAsPlayers.Value)
                {
                    playerMaster.name = master.GetBody().GetDisplayName();
                }
                if (aiOwnership)
                {
                    aiOwnership.ownerMaster = body.master;
                }
                if (ai)
                {
                    ai.name = "PlayerBot";
                    ai.leader.gameObject = body.gameObject;

                    ai.fullVision = true;
                    ai.aimVectorDampTime = .01f;
                    ai.aimVectorMaxSpeed = 180f;
                }

                if (skillDrivers != null)
                {
                    // Strip skills
                    StripSkills(skillDrivers);
                }

                // Add skill drivers based on class
                switch (characterType)
                {
                    case 0:
                        CommandoHelper.InjectSkills(gameObject, ai);
                        break;
                    case 1:
                        ToolbotHelper.InjectSkills(gameObject, ai);
                        break;
                    case 2:
                        HuntressHelper.InjectSkills(gameObject, ai);
                        break;
                    case 3:
                        EngineerHelper.InjectSkills(gameObject, ai);
                        break;
                    case 4:
                        ArtificerHelper.InjectSkills(gameObject, ai);
                        break;
                    case 5:
                        MercenaryHelper.InjectSkills(gameObject, ai);
                        break;
                }

                // Set skill drivers
                AISkillDriver[] skills = ai.GetFieldValue<AISkillDriver[]>("skillDrivers");
                ai.SetFieldValue("skillDrivers", gameObject.GetComponents<AISkillDriver>());

                if (AutoPurchaseItems.Value)
                {
                    // Add item manager
                    ItemManager itemManager = gameObject.AddComponent<ItemManager>() as ItemManager;
                }

                // Add to playerbot list
                playerbots.Add(gameObject);
            }
        }

        private static void StripSkills(AISkillDriver[] skillDrivers)
        {
            foreach (AISkillDriver skill in skillDrivers)
            {
                DestroyImmediate(skill);
            }
        }

        private static void DumpAllSkills(GameObject gameObject)
        {
            Chat.AddMessage("All Skills:");
            AISkillDriver[] component4 = gameObject.GetComponents<AISkillDriver>();
            foreach (AISkillDriver skill in component4)
            {
                Chat.AddMessage("Name: " + skill.customName);
                DumpSkill(skill);
                Chat.AddMessage(" ");
            }
        }

        public static void DumpSkill(AISkillDriver skill)
        {
            Chat.AddMessage(" ");
            Chat.AddMessage("Name: " + skill.customName);
            Chat.AddMessage("Slot: " + skill.skillSlot);
            Chat.AddMessage("Ready: " + skill.requireSkillReady);

            Chat.AddMessage("TargetMove: " + skill.moveTargetType);
            Chat.AddMessage("MinUHp: " + skill.minUserHealthFraction);
            Chat.AddMessage("MaxUHp: " + skill.maxUserHealthFraction);
            Chat.AddMessage("MinTHP: " + skill.minTargetHealthFraction);
            Chat.AddMessage("MaxTHp: " + skill.maxTargetHealthFraction);

            Chat.AddMessage("MinDist: " + skill.minDistance);
            Chat.AddMessage("MaxDist: " + skill.maxDistance);

            Chat.AddMessage("SelectionTarget: " + skill.selectionRequiresTargetLoS);
            Chat.AddMessage("AimTarget: " + skill.activationRequiresTargetLoS);
            Chat.AddMessage("AimConfirm: " + skill.activationRequiresAimConfirmation);

            Chat.AddMessage("MoveType: " + skill.movementType);
            Chat.AddMessage("AimType: " + skill.aimType);
            Chat.AddMessage("MoveScale: " + skill.moveInputScale);

            Chat.AddMessage("DriverTimer: " + skill.driverUpdateTimerOverride);
            Chat.AddMessage("IgnoreNode: " + skill.ignoreNodeGraph);
            Chat.AddMessage("ResetEnemy: " + skill.resetCurrentEnemyOnNextDriverSelection);
            Chat.AddMessage("NoRepeat: " + skill.noRepeat);
            Chat.AddMessage("Sprint: " + skill.shouldSprint);
            Chat.AddMessage(" ");
        }

        [ConCommand(commandName = "addbot", flags = ConVarFlags.ExecuteOnServer, helpText = "Adds a playerbot. Usage: addbot [character index] [amount] [network user index]")]
        private static void CCAddBot(ConCommandArgs args)
        {
            NetworkUser user = args.sender;
            if (HostOnlySpawnBots.Value)
            {
                if (NetworkUser.readOnlyInstancesList[0].netId != user.netId)
                {
                    return;
                }
            }

            if (Stage.instance == null || bodyList.Count == 0)
            {
                return;
            }

            int characterType = 0;
            if (args.userArgs.Count > 0)
            {
                string classString = args.userArgs[0];
                Int32.TryParse(classString, out characterType);

                // Clamp
                characterType = Math.Max(Math.Min(characterType, bodyList.Count - 1), 0);
            }

            int amount = 1;
            if (args.userArgs.Count > 1)
            {
                string amountString = args.userArgs[1];
                Int32.TryParse(amountString, out amount);
            }

            if (args.userArgs.Count > 2)
            {
                int userIndex = 0;
                string userString = args.userArgs[2];
                Int32.TryParse(userString, out userIndex);
                if (userIndex >= 0 && userIndex < NetworkUser.readOnlyInstancesList.Count)
                {
                    user = NetworkUser.readOnlyInstancesList[userIndex];
                }
            }

            if (!user || !user.master.alive)
            {
                return;
            }

            CharacterBody body = user.master.GetBody();
            System.Random random = new System.Random();

            for (int i = 0; i < amount; i++)
            {
                SpawnPlayerbot(body, characterType);
            }
        }

        [ConCommand(commandName = "addrandombot", flags = ConVarFlags.ExecuteOnServer, helpText = "Adds a random playerbot. Usage: addrandombot [amount] [network user index]")]
        private static void CCAddRandomBot(ConCommandArgs args)
        {
            NetworkUser user = args.sender;
            if (HostOnlySpawnBots.Value)
            {
                if (NetworkUser.readOnlyInstancesList[0].netId != user.netId)
                {
                    return;
                }
            }

            if (Stage.instance == null || bodyList.Count == 0)
            {
                return;
            }

            int amount = 1;
            if (args.userArgs.Count > 0)
            {
                string amountString = args.userArgs[0];
                Int32.TryParse(amountString, out amount);
            }

            if (args.userArgs.Count > 1)
            {
                int userIndex = 0;
                string userString = args.userArgs[1];
                Int32.TryParse(userString, out userIndex);
                if (userIndex >= 0 && userIndex < NetworkUser.readOnlyInstancesList.Count)
                {
                    user = NetworkUser.readOnlyInstancesList[userIndex];
                }
            }

            if (!user || !user.master.alive)
            {
                return;
            }

            CharacterBody body = user.master.GetBody();
            System.Random random = new System.Random();

            int lastCharacterType = -1;
            for (int i = 0; i < amount; i++)
            {
                int characterType = -1;
                do
                {
                    characterType = random.Next(0, bodyList.Count);
                }
                while (characterType == lastCharacterType && bodyList.Count > 1);

                SpawnPlayerbot(body, characterType);

                lastCharacterType = characterType;
            }
        }

        [ConCommand(commandName = "removebots", flags = ConVarFlags.SenderMustBeServer, helpText = "Removes all bots")]
        private static void CCRemoveBots(ConCommandArgs args)
        {
            foreach (GameObject gameObject in playerbots)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                BaseAI ai = gameObject.GetComponent<BaseAI>();
                ai.name = "";

                master.TrueKill();

                Destroy(gameObject);
            }

            playerbots.Clear();
        }

        [ConCommand(commandName = "killbots", flags = ConVarFlags.SenderMustBeServer, helpText = "Removes all bots")]
        private static void CCKillBots(ConCommandArgs args)
        {
            foreach (GameObject gameObject in playerbots)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                master.TrueKill();
            }

        }

        [ConCommand(commandName = "testbots", flags = ConVarFlags.SenderMustBeServer, helpText = "Testing Command")]
        private static void CCTestBots(ConCommandArgs args)
        {
            if (Stage.instance == null)
            {
                return;
            }

            NetworkUser user = args.sender;
            CharacterBody body = user.master.GetBody();

            if (!user.master.alive)
            {
                return;
            }

            foreach (GameObject gameObject in playerbots)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                AIOwnership aiOwnership = gameObject.GetComponent<AIOwnership>();
                string name = master.GetBody().GetDisplayName();

                if (aiOwnership.ownerMaster)
                {
                    Debug.Log(name + "'s master: " + aiOwnership.ownerMaster.GetBody().GetUserName());
                }
                else
                {
                    Debug.Log(name + " has no master");
                }

                Debug.Log(name + "'s money: " + master.money);
            }
        }
    }
}
