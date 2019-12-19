using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.AI.Walker;
using MonoMod.Cil;
using PlayerBots.AI;
using PlayerBots.Custom;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayerBots
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.meledy.PlayerBots", "PlayerBots", "1.3.0")]
    public class PlayerBotManager : BaseUnityPlugin
    {
        public static System.Random random = new System.Random();

        public static List<GameObject> playerbots = new List<GameObject>();

        public static SurvivorIndex[] RandomSurvivors = new SurvivorIndex[] { SurvivorIndex.Commando, SurvivorIndex.Toolbot, SurvivorIndex.Huntress, SurvivorIndex.Engi, SurvivorIndex.Mage, SurvivorIndex.Merc, SurvivorIndex.Treebot, SurvivorIndex.Loader, SurvivorIndex.Croco};
        public static Dictionary<string, SurvivorIndex> SurvivorDict = new Dictionary<string, SurvivorIndex>();

        // Config options
        private static ConfigWrapper<int> InitialRandomBots { get; set; }
        private static ConfigWrapper<int>[] InitialBots = new ConfigWrapper<int>[RandomSurvivors.Length];

        public static ConfigWrapper<int> MaxBotPurchasesPerStage { get; set; }
        private static ConfigWrapper<bool> AutoPurchaseItems { get; set; }
        public static ConfigWrapper<float> Tier1ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier1ChestBotCost { get; set; }
        public static ConfigWrapper<float> Tier2ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier2ChestBotCost { get; set; }
        public static ConfigWrapper<float> Tier3ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier3ChestBotCost { get; set; }
        private static ConfigWrapper<bool> HostOnlySpawnBots { get; set; }
        private static ConfigWrapper<bool> ShowNameplates { get; set; }
        private static ConfigWrapper<bool> PlayerMode { get; set; }
        private static ConfigWrapper<bool> TpFix { get; set; }
        private static ConfigWrapper<bool> DontScaleInteractables { get; set; }

        public void Awake()
        {
            SurvivorDict.Add("commando", SurvivorIndex.Commando);
            SurvivorDict.Add("mult", SurvivorIndex.Toolbot);
            SurvivorDict.Add("mul-t", SurvivorIndex.Toolbot);
            SurvivorDict.Add("toolbot", SurvivorIndex.Toolbot);
            SurvivorDict.Add("hunt", SurvivorIndex.Huntress);
            SurvivorDict.Add("huntress", SurvivorIndex.Huntress);
            SurvivorDict.Add("engi", SurvivorIndex.Engi);
            SurvivorDict.Add("engineer", SurvivorIndex.Engi);
            SurvivorDict.Add("mage", SurvivorIndex.Mage);
            SurvivorDict.Add("arti", SurvivorIndex.Mage);
            SurvivorDict.Add("artificer", SurvivorIndex.Mage);
            SurvivorDict.Add("merc", SurvivorIndex.Merc);
            SurvivorDict.Add("mercenary", SurvivorIndex.Merc);
            SurvivorDict.Add("rex", SurvivorIndex.Treebot);
            SurvivorDict.Add("treebot", SurvivorIndex.Treebot);
            SurvivorDict.Add("loader", SurvivorIndex.Loader);
            SurvivorDict.Add("acrid", SurvivorIndex.Croco);
            SurvivorDict.Add("croco", SurvivorIndex.Croco);

            // Config
            InitialRandomBots = Config.Wrap("Starting Bots", "StartingBots.Random", "Starting amount of bots to spawn at the start of a run. (Random)", 0);
            for (int i = 0; i < RandomSurvivors.Length; i++)
            {
                string name = RandomSurvivors[i].ToString();
                InitialBots[i] = Config.Wrap("Starting Bots", "StartingBots." + name, "Starting amount of bots to spawn at the start of a run. (" + name + ")", 0);
            }

            AutoPurchaseItems = Config.Wrap("Bot Inventory", "AutoPurchaseItems", "Maximum amount of purchases a playerbot can do per stage. Items are purchased directly instead of from chests. (Default: true)", true);
            MaxBotPurchasesPerStage = Config.Wrap("Bot Inventory", "MaxBotPurchasesPerStage", "Maximum amount of putchases a playerbot can do per stage. (Default: 8)", 8);
            Tier1ChestBotWeight = Config.Wrap("Bot Inventory", "Tier1ChestBotWeight", "Weight of a bot picking an item from a small chest's loot table. (Default: 0.8)", 0.8f);
            Tier2ChestBotWeight = Config.Wrap("Bot Inventory", "Tier2ChestBotWeight", "Weight of a bot picking an item from a large chest's loot table. (Default: 0.2)", 0.2f);
            Tier3ChestBotWeight = Config.Wrap("Bot Inventory", "Tier3ChestBotWeight", "Weight of a bot picking an item from a legendary chest's loot table. (Default: 0)", 0f);
            Tier1ChestBotCost = Config.Wrap("Bot Inventory", "Tier1ChestBotCost", "Base price of a small chest for the bot. (Default: 25)", 25);
            Tier2ChestBotCost = Config.Wrap("Bot Inventory", "Tier2ChestBotCost", "Base price of a large chest for the bot. (Default: 50)", 50);
            Tier3ChestBotCost = Config.Wrap("Bot Inventory", "Tier3ChestBotCost", "Base price of a legendary chest for the bot. (Default: 400)", 400);

            HostOnlySpawnBots = Config.Wrap("Misc", "HostOnlySpawnBots", "Set true so that only the host may spawn bots", true);
            ShowNameplates = Config.Wrap("Misc", "ShowNameplates", "Show player nameplates on playerbots if SpawnAsPlayers == false. (Host only)", true);

            PlayerMode = Config.Wrap("Experimental", "SpawnAsPlayers", "Makes the game treat playerbots like how regular players are treated. The bots now show up on the scoreboard, can pick up items, influence the map scaling, etc.", false);
            TpFix = Config.Wrap("Player Mode", "Teleport Fix", "Fixes long teleporter charging times by making the bots count towards the charging timer. Only active is PlayerMode is true.", true);
            DontScaleInteractables = Config.Wrap("Player Mode", "DontScaleInteractables", "Prevents interactables spawn count from scaling with bots. Only active is PlayerMode is true.", false);

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            // Ugh.
            On.RoR2.CharacterAI.BaseAI.OnBodyLost += (orig, self, body) =>
            {
                if (self.name.Equals("PlayerBot"))
                {
                    return;
                }
                orig(self, body);
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

            if (!PlayerMode.Value && AutoPurchaseItems.Value)
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
            }

            if (AutoPurchaseItems.Value)
            {
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

            if (PlayerMode.Value && TpFix.Value)
            {
                On.RoR2.TeleporterInteraction.GetPlayerCountInRadius += (orig, self) =>
                {
                    Vector3 position = self.gameObject.transform.position;
                    float num2 = self.clearRadius * self.clearRadius;
                    return PlayerCharacterMasterController.instances.Count((PlayerCharacterMasterController c) => c.master.alive && (c.master.GetBody().transform.position - position).sqrMagnitude <= num2);
                };
            }

            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active)
                {
                    if (PlayerMode.Value)
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
                    // Spawn starting bots
                    if (Run.instance.stageClearCount == 0)
                    {
                        if (InitialRandomBots.Value > 0)
                        {
                            SpawnRandomPlayerbots(NetworkUser.readOnlyInstancesList[0].master, InitialRandomBots.Value);
                        }
                        for (int randomSurvivorsIndex = 0; randomSurvivorsIndex < InitialBots.Length; randomSurvivorsIndex++)
                        {
                            if (InitialBots[randomSurvivorsIndex].Value > 0)
                            {
                                SpawnPlayerbots(NetworkUser.readOnlyInstancesList[0].master, RandomSurvivors[randomSurvivorsIndex], InitialBots[randomSurvivorsIndex].Value);
                            }
                        }
                    }
                }
            };

            if (PlayerMode.Value)
            {
                On.RoR2.SceneDirector.PlaceTeleporter += (orig, self) =>
                {
                    if (DontScaleInteractables.Value)
                    {
                        int count = PlayerCharacterMasterController.instances.Count((PlayerCharacterMasterController v) => v.networkUser);
                        float num = 0.5f + (float)count * 0.5f;
                        ClassicStageInfo component = SceneInfo.instance.GetComponent<ClassicStageInfo>();
                        self.SetFieldValue("interactableCredit", (int)((float)component.sceneDirectorInteractibleCredits * num));
                    }
                    else if (Run.instance.stageClearCount == 0 && GetInitialBotCount() > 0)
                    {
                        int count = PlayerCharacterMasterController.instances.Count((PlayerCharacterMasterController v) => v.networkUser);
                        count += GetInitialBotCount();
                        float num = 0.5f + (float)count * 0.5f;
                        ClassicStageInfo component = SceneInfo.instance.GetComponent<ClassicStageInfo>();
                        self.SetFieldValue("interactableCredit", (int)((float)component.sceneDirectorInteractibleCredits * num));
                    }

                    orig(self);
                };

                IL.RoR2.UI.AllyCardManager.PopulateCharacterDataSet += il =>
                {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(x => x.MatchCallvirt<CharacterMaster>("get_playerCharacterMasterController"));
                    c.Index += 2;
                    c.EmitDelegate<Func<bool, bool>>(x => false);
                };
            }
        }

        public void Start()
        {
            AiSkillHelperCatalog.Populate();
        }

        private static int GetInitialBotCount()
        {
            int count = InitialRandomBots.Value;
            for (int randomSurvivorsIndex = 0; randomSurvivorsIndex < InitialBots.Length; randomSurvivorsIndex++)
            {
                count += InitialBots[randomSurvivorsIndex].Value;
            }
            return count;
        }

        public static void SpawnPlayerbot(CharacterMaster owner, SurvivorIndex survivorIndex)
        {
            if (PlayerMode.Value)
            {
                SpawnPlayerbotAsPlayer(owner, survivorIndex);
            }
            else
            {
                SpawnPlayerbotAsSummon(owner, survivorIndex);
            }
        }

        private static void SpawnPlayerbotAsPlayer(CharacterMaster owner, SurvivorIndex survivorIndex)
        {
            SurvivorDef def = SurvivorCatalog.GetSurvivorDef(survivorIndex);
            if (def == null)
            {
                return;
            }

            GameObject bodyPrefab = def.bodyPrefab;
            if (bodyPrefab == null)
            {
                return;
            }

            // Card
            PlayerBotSpawnCard card = ScriptableObject.CreateInstance<PlayerBotSpawnCard>();
            card.hullSize = HullClassification.Human;
            card.nodeGraphType = MapNodeGroup.GraphType.Ground;
            card.occupyPosition = false;
            card.sendOverNetwork = true;
            card.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            card.prefab = Resources.Load<GameObject>("prefabs/charactermasters/CommandoMaster");
            card.playerbotName = bodyPrefab.GetComponent<CharacterBody>().GetDisplayName();
            card.bodyPrefab = bodyPrefab;

            // Spawn
            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(card, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = owner.GetBody().transform
            }, RoR2Application.rng);
            spawnRequest.ignoreTeamMemberLimit = true;
            //spawnRequest.summonerBodyObject = owner.GetBody().gameObject;
            spawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);

            GameObject gameObject = DirectorCore.instance.TrySpawnObject(spawnRequest);

            if (gameObject)
            {
                // Add components
                EntityStateMachine stateMachine = gameObject.AddComponent<PlayerBotStateMachine>() as EntityStateMachine;
                BaseAI ai = gameObject.AddComponent<PlayerBotBaseAI>() as BaseAI;
                AIOwnership aiOwnership = gameObject.AddComponent<AIOwnership>() as AIOwnership;
                aiOwnership.ownerMaster = owner;

                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                PlayerCharacterMasterController playerMaster = gameObject.GetComponent<PlayerCharacterMasterController>();

                // Random skin
                SetRandomSkin(master, bodyPrefab);

                // Set commponent values
                master.SetFieldValue("aiComponents", gameObject.GetComponents<BaseAI>());
                master.GiveMoney(owner.money);
                master.inventory.CopyItemsFrom(owner.inventory);
                master.inventory.GiveItem(ItemIndex.DrizzlePlayerHelper, 1);
                master.destroyOnBodyDeath = false; // Allow the bots to spawn in the next stage

                playerMaster.name = master.GetBody().GetDisplayName();

                // Add custom skills
                InjectSkillDrivers(gameObject, ai, survivorIndex);

                if (AutoPurchaseItems.Value)
                {
                    // Add item manager
                    ItemManager itemManager = gameObject.AddComponent<ItemManager>() as ItemManager;
                }

                // Add to playerbot list
                playerbots.Add(gameObject);

                // Cleanup
                Destroy(card);
            }
        }

        // A hacky method. Don't ask questions.
        private static void SpawnPlayerbotAsSummon(CharacterMaster owner, SurvivorIndex survivorIndex)
        {
            SurvivorDef def = SurvivorCatalog.GetSurvivorDef(survivorIndex);
            if (def == null)
            {
                return;
            }

            GameObject bodyPrefab = def.bodyPrefab;
            if (bodyPrefab == null)
            {
                return;
            }

            // Get card
            SpawnCard card = (SpawnCard)Resources.Load("SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly");

            // Spawn request
            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(card, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = owner.GetBody().transform
            }, RoR2Application.rng);
            spawnRequest.ignoreTeamMemberLimit = true;
            spawnRequest.summonerBodyObject = owner.GetBody().gameObject;

            // Spawn
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(spawnRequest);

            if (gameObject)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                AIOwnership aiOwnership = gameObject.AddComponent<AIOwnership>() as AIOwnership;
                BaseAI ai = gameObject.GetComponent<BaseAI>();

                if (master)
                {
                    master.name = "PlayerBot";
                    master.bodyPrefab = bodyPrefab;
                    SetRandomSkin(master, bodyPrefab);

                    master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);
                    master.teamIndex = TeamIndex.Player;

                    master.GiveMoney(owner.money);
                    master.inventory.CopyItemsFrom(owner.inventory);
                    master.inventory.GiveItem(ItemIndex.DrizzlePlayerHelper, 1);

                    // Allow the bots to spawn in the next stage
                    master.destroyOnBodyDeath = false;
                    master.gameObject.AddComponent<SetDontDestroyOnLoad>();
                }
                if (aiOwnership)
                {
                    aiOwnership.ownerMaster = owner;
                }
                if (ai)
                {
                    ai.name = "PlayerBot";
                    ai.leader.gameObject = owner.GetBody().gameObject;

                    ai.fullVision = true;
                    ai.aimVectorDampTime = .01f;
                    ai.aimVectorMaxSpeed = 180f;
                }

                InjectSkillDrivers(gameObject, ai, survivorIndex);

                if (AutoPurchaseItems.Value)
                {
                    // Add item manager
                    ItemManager itemManager = gameObject.AddComponent<ItemManager>() as ItemManager;
                }

                // Add to playerbot list
                playerbots.Add(gameObject);
            }
        }

        private static void SetRandomSkin(CharacterMaster master, GameObject bodyPrefab)
        {
            int bodyIndex = bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            SkinDef[] skins = BodyCatalog.GetBodySkins(bodyIndex);
            master.loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, (uint) UnityEngine.Random.Range(0, skins.Length));
        }

        private static void InjectSkillDrivers(GameObject gameObject, BaseAI ai, SurvivorIndex survivorIndex)
        {
            AISkillDriver[] skillDrivers = gameObject.GetComponents<AISkillDriver>();
            if (skillDrivers != null)
            {
                // Strip skills
                StripSkills(skillDrivers);
            }

            // Add skill drivers based on class
            AiSkillHelper skillHelper = AiSkillHelperCatalog.GetSkillHelperByIndex(survivorIndex);
            skillHelper.InjectSkills(gameObject, ai);

            // Set skill drivers
            PropertyInfo property = typeof(BaseAI).GetProperty("skillDrivers");
            property.DeclaringType.GetProperty("skillDrivers");
            property.SetValue(ai, gameObject.GetComponents<AISkillDriver>(), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            //ai.SetFieldValue2("skillDrivers", gameObject.GetComponents<AISkillDriver>());

            // Combat update timer fix
            gameObject.AddComponent<PlayerBotCombatFix>();
        }

        public static void SpawnPlayerbots(CharacterMaster owner, SurvivorIndex characterType, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnPlayerbot(owner, characterType);
            }
        }

        public static void SpawnRandomPlayerbots(CharacterMaster owner, int amount)
        {
            int lastCharacterType = -1;
            for (int i = 0; i < amount; i++)
            {
                int randomSurvivorIndex = -1;
                do
                {
                    randomSurvivorIndex = random.Next(0, RandomSurvivors.Length);
                }
                while (randomSurvivorIndex == lastCharacterType && RandomSurvivors.Length > 1);

                SpawnPlayerbot(owner, RandomSurvivors[randomSurvivorIndex]);

                lastCharacterType = randomSurvivorIndex;
            }
        }

        private static void StripSkills(AISkillDriver[] skillDrivers)
        {
            foreach (AISkillDriver skill in skillDrivers)
            {
                DestroyImmediate(skill);
            }
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

            if (Stage.instance == null)
            {
                return;
            }

            int characterType = 0;
            if (args.userArgs.Count > 0)
            {
                string classString = args.userArgs[0];
                if (!Int32.TryParse(classString, out characterType))
                {
                    SurvivorIndex index;
                    if (SurvivorDict.TryGetValue(classString.ToLower(), out index))
                    {
                        characterType = (int) index;
                    }
                    else
                    {
                        characterType = 0;
                    }
                }
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
                if (Int32.TryParse(userString, out userIndex))
                {
                    userIndex--;
                    if (userIndex >= 0 && userIndex < NetworkUser.readOnlyInstancesList.Count)
                    {
                        user = NetworkUser.readOnlyInstancesList[userIndex];
                    }
                }
                else
                {
                    return;
                }
            }

            if (!user || !user.master.alive)
            {
                return;
            }

            SpawnPlayerbots(user.master, (SurvivorIndex) characterType, amount);

            Debug.Log(user.userName + " spawned " + amount + " bots for " + user.userName);
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

            if (Stage.instance == null)
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
                if (Int32.TryParse(userString, out userIndex))
                {
                    userIndex--;
                    if (userIndex >= 0 && userIndex < NetworkUser.readOnlyInstancesList.Count)
                    {
                        user = NetworkUser.readOnlyInstancesList[userIndex];
                    }
                }
                else
                {
                    return;
                }
            }

            if (!user || !user.master.alive)
            {
                return;
            }

            SpawnRandomPlayerbots(user.master, amount);

            Debug.Log(user.userName + " spawned " + amount + " bots for " + user.userName);
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

        [ConCommand(commandName = "pb_startingbots", flags = ConVarFlags.None, helpText = "Set initial bot count [character type] [amount]")]
        private static void CCInitialBot(ConCommandArgs args)
        {
            int characterType = 0;
            if (args.userArgs.Count > 0)
            {
                string classString = args.userArgs[0];
                if (!Int32.TryParse(classString, out characterType))
                {
                    SurvivorIndex index;
                    if (SurvivorDict.TryGetValue(classString.ToLower(), out index))
                    {
                        characterType = (int)index;
                    }
                    else
                    {
                        characterType = 0;
                    }
                }

                // Clamp
                characterType = Math.Max(Math.Min(characterType, RandomSurvivors.Length - 1), 0);
            }
            else
            {
                return;
            }

            int amount = 0;
            if (args.userArgs.Count > 1)
            {
                string amountString = args.userArgs[1];
                Int32.TryParse(amountString, out amount);
            }
            else
            {
                return;
            }

            InitialBots[characterType].Value = amount;
            Debug.Log("Set StartingBots." + RandomSurvivors[characterType].ToString() + " to " + amount);
        }

        [ConCommand(commandName = "pb_startingbots_random", flags = ConVarFlags.None, helpText = "Set initial random bot count [amount]")]
        private static void CCInitialRandomBot(ConCommandArgs args)
        {
            int amount = 0;
            if (args.userArgs.Count > 0)
            {
                string amountString = args.userArgs[0];
                Int32.TryParse(amountString, out amount);
            }
            else
            {
                return;
            }

            InitialRandomBots.Value = amount;
            Debug.Log("Set StartingBots.Random to " + amount);
        }

        [ConCommand(commandName = "pb_startingbots_reset", flags = ConVarFlags.None, helpText = "Resets all initial bots to 0")]
        private static void CCClearInitialBot(ConCommandArgs args)
        {
            InitialRandomBots.Value = 0;
            for (int i = 0; i < InitialBots.Length; i++)
            {
                InitialBots[i].Value = 0;
            }
            Debug.Log("Reset all StartingBots values to 0");
        }

        [ConCommand(commandName = "pb_maxpurchases", flags = ConVarFlags.None, helpText = "Sets the MaxBotPurchasesPerStage value.")]
        private static void CCSetMaxPurchases(ConCommandArgs args)
        {
            int amount = 0;
            if (args.userArgs.Count > 0)
            {
                string amountString = args.userArgs[0];
                Int32.TryParse(amountString, out amount);
            }
            else
            {
                return;
            }

            MaxBotPurchasesPerStage.Value = amount;
            Debug.Log("Set MaxBotPurchasesPerStage to " + amount);
        }

        [ConCommand(commandName = "pb_listbots", flags = ConVarFlags.SenderMustBeServer, helpText = "Lists bots in console.")]
        private static void CCTestBots(ConCommandArgs args)
        {
            if (Stage.instance == null)
            {
                return;
            }

            NetworkUser user = args.sender;

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

        [ConCommand(commandName = "pb_listsurvivors", flags = ConVarFlags.None, helpText = "Lists survivor indexes.")]
        private static void CCListSurvivors(ConCommandArgs args)
        {
            Debug.Log("Listing all registered survivors and their indexes.");
            foreach (SurvivorDef def in SurvivorCatalog.allSurvivorDefs)
            {
                Debug.Log(def.bodyPrefab.GetComponent<CharacterBody>().GetDisplayName() + " : " + (int) def.survivorIndex);
            }
        }
    }
}
