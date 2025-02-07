using BepInEx;
using BepInEx.Configuration;
using PlayerBots.AI;
using PlayerBots.AI.SkillHelpers;
using PlayerBots.Custom;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PlayerBots
{
    [BepInPlugin("com.meledy.PlayerBots", "PlayerBots", "1.7.1")]
    public class PlayerBotManager : BaseUnityPlugin
    {
        public static System.Random random = new System.Random();

        public static List<GameObject> playerbots = new List<GameObject>();

        public static List<SurvivorIndex> RandomSurvivorsList = new List<SurvivorIndex>();
        public static Dictionary<string, SurvivorIndex> SurvivorDict = new Dictionary<string, SurvivorIndex>();

        // Config options
        public static ConfigEntry<int> InitialRandomBots { get; set; }
        public static ConfigEntry<int>[] InitialBots;

        public static ConfigEntry<int> MaxBotPurchasesPerStage { get; set; }
        public static ConfigEntry<bool> AutoPurchaseItems { get; set; }
        public static ConfigEntry<float> Tier1ChestBotWeight { get; set; }
        public static ConfigEntry<int> Tier1ChestBotCost { get; set; }
        public static ConfigEntry<float> Tier2ChestBotWeight { get; set; }
        public static ConfigEntry<int> Tier2ChestBotCost { get; set; }
        public static ConfigEntry<float> Tier3ChestBotWeight { get; set; }
        public static ConfigEntry<int> Tier3ChestBotCost { get; set; }
        public static ConfigEntry<int> EquipmentBuyChance { get; set; }
        public static ConfigEntry<float> MinBuyingDelay { get; set; }
        public static ConfigEntry<float> MaxBuyingDelay { get; set; }
        public static ConfigEntry<bool> ShowBuyMessages { get; set; }
        public static ConfigEntry<bool> HostOnlySpawnBots { get; set; }
        public static ConfigEntry<bool> ShowNameplates { get; set; }
        public static ConfigEntry<bool> PlayerMode { get; set; }
        public static ConfigEntry<bool> DontScaleInteractables { get; set; }
        public static ConfigEntry<bool> BotsUseInteractables { get; set; }
        public static ConfigEntry<bool> ContinueAfterDeath { get; set; }
        public static ConfigEntry<bool> RespawnAfterWave { get; set; }

        //
        public static bool allRealPlayersDead;

        public void Awake()
        {
            // Config
            InitialRandomBots = Config.Bind("Starting Bots", "StartingBots.Random", 0, "Starting amount of bots to spawn at the start of a run. (Random)");

            AutoPurchaseItems = Config.Bind("Bot Inventory", "AutoPurchaseItems", true, "Maximum amount of purchases a playerbot can do per stage. Items are purchased directly instead of from chests.");
            MaxBotPurchasesPerStage = Config.Bind("Bot Inventory", "MaxBotPurchasesPerStage", 10, "Maximum amount of putchases a playerbot can do per stage.");
            Tier1ChestBotWeight = Config.Bind("Bot Inventory", "Tier1ChestBotWeight", 0.8f, "Weight of a bot picking an item from a small chest's loot table.");
            Tier2ChestBotWeight = Config.Bind("Bot Inventory", "Tier2ChestBotWeight", 0.2f, "Weight of a bot picking an item from a large chest's loot table.");
            Tier3ChestBotWeight = Config.Bind("Bot Inventory", "Tier3ChestBotWeight", 0f, "Weight of a bot picking an item from a legendary chest's loot table.");
            Tier1ChestBotCost = Config.Bind("Bot Inventory", "Tier1ChestBotCost", 25, "Base price of a small chest for the bot.");
            Tier2ChestBotCost = Config.Bind("Bot Inventory", "Tier2ChestBotCost", 50, "Base price of a large chest for the bot.");
            Tier3ChestBotCost = Config.Bind("Bot Inventory", "Tier3ChestBotCost", 400, "Base price of a legendary chest for the bot.");
            EquipmentBuyChance = Config.Bind("Bot Inventory", "EquipmentBuyChance", 15, "Chance between 0 and 100 for a bot to buy from an equipment barrel instead of a tier 1 chest. Only active while the bot does not have a equipment item. (Default: 15)");
            MinBuyingDelay = Config.Bind("Bot Inventory", "MinBuyingDelay", 0f, "Minimum delay in seconds between the time it takes for a bot checks to buy an item.");
            MaxBuyingDelay = Config.Bind("Bot Inventory", "MaxBuyingDelay", 5f, "Maximum delay in seconds between the time it takes for a bot checks to buy an item.");
            ShowBuyMessages = Config.Bind("Bot Inventory", "ShowBuyMessages", true, "Displays whenever a bot buys an item in chat.");

            HostOnlySpawnBots = Config.Bind("Misc", "HostOnlySpawnBots", true, "Set true so that only the host may spawn bots");
            ShowNameplates = Config.Bind("Misc", "ShowNameplates", true, "Show player nameplates on playerbots if PlayerMode is false. (Host only)");

            PlayerMode = Config.Bind("Player Mode", "PlayerMode", false, "Makes the game treat playerbots like how regular players are treated. The bots now show up on the scoreboard, can pick up items, influence the map scaling, etc.");
            DontScaleInteractables = Config.Bind("Player Mode", "DontScaleInteractables", true, "Prevents interactables spawn count from scaling with bots. Only active is PlayerMode is true.");
            BotsUseInteractables = Config.Bind("Player Mode", "BotsUseInteractables", false, "[Experimental] Allow bots to use interactables, such as buying from a chest and picking up items on the ground. Only active is PlayerMode is true.");
            ContinueAfterDeath = Config.Bind("Player Mode", "ContinueAfterDeath", false, "Bots will activate and use teleporters when all real players die. Only active is PlayerMode is true.");

            RespawnAfterWave = Config.Bind("Simulacrum", "RespawnAfterWave", false, "Respawns bots after each wave in simulacrum");

            // Sanity check
            MaxBuyingDelay.Value = Math.Max(MaxBuyingDelay.Value, MinBuyingDelay.Value);

            // Add console commands
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            // Content manager load hook - Will find a better place for this later
            RoR2Application.onLoad += OnContentLoad;

            // Apply hooks
            PlayerBotHooks.AddHooks();
        }

        public void OnContentLoad()
        {
            // Base game survivors
            SurvivorDict.Add("mult", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("mul-t", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("toolbot", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("hunt", SurvivorCatalog.FindSurvivorIndex("Huntress"));
            SurvivorDict.Add("huntress", SurvivorCatalog.FindSurvivorIndex("Huntress"));
            SurvivorDict.Add("engi", SurvivorCatalog.FindSurvivorIndex("Engi"));
            SurvivorDict.Add("engineer", SurvivorCatalog.FindSurvivorIndex("Engi"));
            SurvivorDict.Add("mage", SurvivorCatalog.FindSurvivorIndex("Mage"));
            SurvivorDict.Add("arti", SurvivorCatalog.FindSurvivorIndex("Mage"));
            SurvivorDict.Add("artificer", SurvivorCatalog.FindSurvivorIndex("Mage"));
            SurvivorDict.Add("merc", SurvivorCatalog.FindSurvivorIndex("Merc"));
            SurvivorDict.Add("mercenary", SurvivorCatalog.FindSurvivorIndex("Merc"));
            SurvivorDict.Add("rex", SurvivorCatalog.FindSurvivorIndex("Treebot"));
            SurvivorDict.Add("treebot", SurvivorCatalog.FindSurvivorIndex("Treebot"));
            SurvivorDict.Add("croco", SurvivorCatalog.FindSurvivorIndex("Croco"));
            SurvivorDict.Add("capt", SurvivorCatalog.FindSurvivorIndex("Captain"));
            SurvivorDict.Add("captain", SurvivorCatalog.FindSurvivorIndex("Captain"));

            // SoTV survivors
            SurvivorDict.Add("railgunner", SurvivorCatalog.FindSurvivorIndex("Railgunner"));
            SurvivorDict.Add("rail", SurvivorCatalog.FindSurvivorIndex("Railgunner"));
            SurvivorDict.Add("void", SurvivorCatalog.FindSurvivorIndex("VoidSurvivor"));
            SurvivorDict.Add("voidfiend", SurvivorCatalog.FindSurvivorIndex("VoidSurvivor"));
            SurvivorDict.Add("voidsurvivor", SurvivorCatalog.FindSurvivorIndex("VoidSurvivor"));

            // SotS survivors
            SurvivorDict.Add("seeker", SurvivorCatalog.FindSurvivorIndex("Seeker"));
            SurvivorDict.Add("chef", SurvivorCatalog.FindSurvivorIndex("Chef"));
            SurvivorDict.Add("son", SurvivorCatalog.FindSurvivorIndex("FalseSon"));
            SurvivorDict.Add("falseson", SurvivorCatalog.FindSurvivorIndex("FalseSon"));

            // Init skill helpers
            AiSkillHelperCatalog.Populate();

            // Config
            InitialBots = new ConfigEntry<int>[RandomSurvivorsList.Count];
            for (int i = 0; i < RandomSurvivorsList.Count; i++)
            {
                string name = BodyCatalog.GetBodyName(SurvivorCatalog.GetBodyIndexFromSurvivorIndex(RandomSurvivorsList[i])).Replace("\'", "");
                InitialBots[i] = Config.Bind("Starting Bots", "StartingBots." + name, 0, "Starting amount of bots to spawn at the start of a run. (" + name + ")");
            }
        }

        public static int GetInitialBotCount()
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
            else if (!def.CheckRequiredExpansionEnabled())
            {
                Debug.Log("You do not have the proper expansion enabled.");
                return;
            }

            GameObject bodyPrefab = def.bodyPrefab;
            if (bodyPrefab == null)
            {
                return;
            }

            // Create spawn card
            PlayerBotSpawnCard card = ScriptableObject.CreateInstance<PlayerBotSpawnCard>();
            card.hullSize = HullClassification.Human;
            card.nodeGraphType = MapNodeGroup.GraphType.Ground;
            card.occupyPosition = false;
            card.sendOverNetwork = true;
            card.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            card.prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMaster");

            // Get spawn position
            Transform spawnPosition = GetRandomSpawnPosition(owner);

            if (spawnPosition == null)
            {
                Debug.LogError("No spawn positions found for playerbot");
                return;
            }

            // Spawn
            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(card, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = spawnPosition
            }, RoR2Application.rng);
            spawnRequest.ignoreTeamMemberLimit = true;
            spawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);

            spawnRequest.onSpawnedServer = result =>
            {
                GameObject gameObject = result.spawnedInstance;

                if (gameObject)
                {
                    // Add components
                    EntityStateMachine stateMachine = gameObject.AddComponent<PlayerBotStateMachine>() as EntityStateMachine;
                    BaseAI ai = gameObject.AddComponent<PlayerBotBaseAI>() as BaseAI;
                    AIOwnership aiOwnership = gameObject.AddComponent<AIOwnership>() as AIOwnership;
                    aiOwnership.ownerMaster = owner;

                    CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                    PlayerCharacterMasterController playerMaster = gameObject.GetComponent<PlayerCharacterMasterController>();
                    playerMaster.name = "PlayerBot";

                    // Required to bypass entitlements
                    master.bodyPrefab = bodyPrefab;
                    master.Respawn(master.transform.position, master.transform.rotation);

                    // Random skin
                    SetRandomSkin(master, bodyPrefab);

                    // Set commponent values
                    master.SetFieldValue("aiComponents", gameObject.GetComponents<BaseAI>());
                    master.destroyOnBodyDeath = false; // Allow the bots to spawn in the next stage

                    // Starting items
                    GiveStartingItems(owner, master);

                    // Add custom skills
                    InjectSkillDrivers(gameObject, ai, survivorIndex);

                    if (AutoPurchaseItems.Value)
                    {
                        // Add item manager
                        ItemManager itemManager = gameObject.AddComponent<ItemManager>() as ItemManager;
                    }

                    // Add to playerbot list
                    playerbots.Add(gameObject);
                }
            };

            DirectorCore.instance.TrySpawnObject(spawnRequest);

            // Cleanup
            Destroy(card);
        }

        private static void SpawnPlayerbotAsSummon(CharacterMaster owner, SurvivorIndex survivorIndex)
        {
            SurvivorDef def = SurvivorCatalog.GetSurvivorDef(survivorIndex);
            if (def == null)
            {
                return;
            }
            else if (!def.CheckRequiredExpansionEnabled())
            {
                Debug.Log("You do not have the proper expansion enabled.");
                return;
            }

            GameObject bodyPrefab = def.bodyPrefab;
            if (bodyPrefab == null)
            {
                return;
            }

            // Create spawn card
            PlayerBotSpawnCard card = ScriptableObject.CreateInstance<PlayerBotSpawnCard>();
            card.hullSize = HullClassification.Human;
            card.nodeGraphType = MapNodeGroup.GraphType.Ground;
            card.occupyPosition = false;
            card.sendOverNetwork = true;
            card.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            card.prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster");
            card.bodyPrefab = bodyPrefab;

            // Get spawn position
            Transform spawnPosition = GetRandomSpawnPosition(owner);

            if (spawnPosition == null)
            {
                Debug.LogError("No spawn positions found for playerbot");
                return;
            }

            // Spawn request
            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(card, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = spawnPosition
            }, RoR2Application.rng);
            spawnRequest.ignoreTeamMemberLimit = true;
            spawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);

            // Spawn
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(spawnRequest);

            if (gameObject)
            {
                CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                BaseAI ai = gameObject.GetComponent<BaseAI>();
                AIOwnership aiOwnership = gameObject.AddComponent<AIOwnership>() as AIOwnership;
                aiOwnership.ownerMaster = owner;

                if (master)
                {
                    master.name = "PlayerBot";
                    master.teamIndex = TeamIndex.Player;

                    SetRandomSkin(master, bodyPrefab);

                    GiveStartingItems(owner, master);

                    // Allow the bots to spawn in the next stage
                    master.destroyOnBodyDeath = false;
                    master.gameObject.AddComponent<SetDontDestroyOnLoad>();
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

        private static Transform GetRandomSpawnPosition(CharacterMaster owner)
        {
            if (owner.GetBody() != null)
            {
                return owner.GetBody().transform;
            }
            else
            {
                SpawnPoint spawnPoint = SpawnPoint.ConsumeSpawnPoint();
                if (spawnPoint != null)
                {
                    spawnPoint.consumed = false;
                    return spawnPoint.transform;
                }
            }

            return null;
        }

        private static void GiveStartingItems(CharacterMaster owner, CharacterMaster master)
        {
            master.GiveMoney(owner.money);
            master.inventory.CopyItemsFrom(owner.inventory);
            master.inventory.RemoveItem(ItemCatalog.FindItemIndex("CaptainDefenseMatrix"), owner.inventory.GetItemCount(ItemCatalog.FindItemIndex("CaptainDefenseMatrix")));
            master.inventory.GiveItem(ItemCatalog.FindItemIndex("DrizzlePlayerHelper"), 1);
        }

        private static void SetRandomSkin(CharacterMaster master, GameObject bodyPrefab)
        {
            BodyIndex bodyIndex = bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            SkinDef[] skins = BodyCatalog.GetBodySkins(bodyIndex);
            master.loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, (uint)UnityEngine.Random.Range(0, skins.Length));
        }

        private static void InjectSkillDrivers(GameObject gameObject, BaseAI ai, SurvivorIndex survivorIndex)
        {
            // Get skill helper
            AiSkillHelper skillHelper = AiSkillHelperCatalog.CreateSkillHelper(survivorIndex);

            // Remove old skill drivers if custom skill drivers exist
            if (skillHelper.GetType() != typeof(DefaultSkillHelper))
            {
                // Get old skill drivers
                AISkillDriver[] skillDrivers = gameObject.GetComponents<AISkillDriver>();
                if (skillDrivers != null)
                {
                    // Remove skill drivers
                    StripSkills(skillDrivers);
                }

                // Add skill drivers based on class
                skillHelper.InjectSkills(gameObject, ai);

                // Set new skill drivers
                PropertyInfo property = typeof(BaseAI).GetProperty("skillDrivers");
                property.DeclaringType.GetProperty("skillDrivers");
                property.SetValue(ai, gameObject.GetComponents<AISkillDriver>(), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            }
            else
            {
                // Add leash skills
                skillHelper.AddDefaultSkills(gameObject, ai, 0);
            }

            // Set BaseAI properties
            if (ai)
            {
                ai.name = "PlayerBot";
                ai.neverRetaliateFriendlies = true;
                ai.fullVision = true;
                ai.aimVectorDampTime = .01f;
                ai.aimVectorMaxSpeed = 180f;
            }

            // Add playerbot controller for extra behaviors and fixes
            PlayerBotController controller = gameObject.AddComponent<PlayerBotController>();
            controller.SetSkillHelper(skillHelper);
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
                    randomSurvivorIndex = random.Next(0, RandomSurvivorsList.Count);
                }
                while ((randomSurvivorIndex == lastCharacterType || !SurvivorCatalog.GetSurvivorDef((SurvivorIndex) RandomSurvivorsList[randomSurvivorIndex]).CheckRequiredExpansionEnabled()) && RandomSurvivorsList.Count > 1);

                SpawnPlayerbot(owner, RandomSurvivorsList[randomSurvivorIndex]);

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

        public void FixedUpdate()
        {
            allRealPlayersDead = !PlayerCharacterMasterController.instances.Any(p => p.preventGameOver && p.isConnected);
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
                        characterType = (int)index;
                    }
                    else
                    {
                        characterType = 0;
                        Debug.LogError("No survivor with that name exists.");
                        return;
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

            if (!user || user.master.IsDeadAndOutOfLivesServer())
            {
                return;
            }

            SpawnPlayerbots(user.master, (SurvivorIndex)characterType, amount);

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

            if (!user || user.master.IsDeadAndOutOfLivesServer())
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
                if (gameObject)
                {
                    CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
                    master.TrueKill();
                }
            }
        }

        [ConCommand(commandName = "tpbots", flags = ConVarFlags.SenderMustBeServer, helpText = "Teleports all bots to you")]
        private static void CCTpBots(ConCommandArgs args)
        {
            NetworkUser user = args.sender;

            if (Stage.instance == null || user.master == null || user.master.IsDeadAndOutOfLivesServer())
            {
                return;
            }

            foreach (GameObject gameObject in playerbots)
            {
                if (gameObject)
                {
                    CharacterMaster master = gameObject.GetComponent<CharacterMaster>();

                    if (!master.IsDeadAndOutOfLivesServer())
                    {
                        TeleportHelper.TeleportGameObject(master.GetBody().gameObject, new Vector3(
                            user.master.GetBody().transform.position.x,
                            user.master.GetBody().transform.position.y,
                            user.master.GetBody().transform.position.z
                        ));
                    }
                }
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
                characterType = Math.Max(Math.Min(characterType, RandomSurvivorsList.Count - 1), 0);
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
            Debug.Log("Set StartingBots." + RandomSurvivorsList[characterType].ToString() + " to " + amount);
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
                Debug.Log(def.bodyPrefab.GetComponent<CharacterBody>().GetDisplayName() + " (" + def.bodyPrefab.name  + ") : " + (int)def.survivorIndex);
            }
        }
    }
}
