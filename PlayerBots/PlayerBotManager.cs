using BepInEx;
using BepInEx.Configuration;
using PlayerBots.AI;
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
    [BepInPlugin("com.meledy.PlayerBots", "PlayerBots", "1.5.0")]
    public class PlayerBotManager : BaseUnityPlugin
    {
        public static System.Random random = new System.Random();

        public static List<GameObject> playerbots = new List<GameObject>();

        public static List<SurvivorIndex> RandomSurvivorsList = new List<SurvivorIndex>();
        public static Dictionary<string, SurvivorIndex> SurvivorDict = new Dictionary<string, SurvivorIndex>();

        // Config options
        public static ConfigWrapper<int> InitialRandomBots { get; set; }
        public static ConfigWrapper<int>[] InitialBots;

        public static ConfigWrapper<int> MaxBotPurchasesPerStage { get; set; }
        public static ConfigWrapper<bool> AutoPurchaseItems { get; set; }
        public static ConfigWrapper<float> Tier1ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier1ChestBotCost { get; set; }
        public static ConfigWrapper<float> Tier2ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier2ChestBotCost { get; set; }
        public static ConfigWrapper<float> Tier3ChestBotWeight { get; set; }
        public static ConfigWrapper<int> Tier3ChestBotCost { get; set; }
        public static ConfigWrapper<int> EquipmentBuyChance { get; set; }
        public static ConfigWrapper<float> MinBuyingDelay { get; set; }
        public static ConfigWrapper<float> MaxBuyingDelay { get; set; }
        public static ConfigWrapper<bool> ShowBuyMessages { get; set; }
        public static ConfigWrapper<bool> HostOnlySpawnBots { get; set; }
        public static ConfigWrapper<bool> ShowNameplates { get; set; }
        public static ConfigWrapper<bool> PlayerMode { get; set; }
        public static ConfigWrapper<bool> DontScaleInteractables { get; set; }

        public void Awake()
        {
            // Config
            InitialRandomBots = Config.Wrap("Starting Bots", "StartingBots.Random", "Starting amount of bots to spawn at the start of a run. (Random)", 0);

            AutoPurchaseItems = Config.Wrap("Bot Inventory", "AutoPurchaseItems", "Maximum amount of purchases a playerbot can do per stage. Items are purchased directly instead of from chests.", true);
            MaxBotPurchasesPerStage = Config.Wrap("Bot Inventory", "MaxBotPurchasesPerStage", "Maximum amount of putchases a playerbot can do per stage.", 10);
            Tier1ChestBotWeight = Config.Wrap("Bot Inventory", "Tier1ChestBotWeight", "Weight of a bot picking an item from a small chest's loot table.", 0.8f);
            Tier2ChestBotWeight = Config.Wrap("Bot Inventory", "Tier2ChestBotWeight", "Weight of a bot picking an item from a large chest's loot table.", 0.2f);
            Tier3ChestBotWeight = Config.Wrap("Bot Inventory", "Tier3ChestBotWeight", "Weight of a bot picking an item from a legendary chest's loot table.", 0f);
            Tier1ChestBotCost = Config.Wrap("Bot Inventory", "Tier1ChestBotCost", "Base price of a small chest for the bot.", 25);
            Tier2ChestBotCost = Config.Wrap("Bot Inventory", "Tier2ChestBotCost", "Base price of a large chest for the bot.", 50);
            Tier3ChestBotCost = Config.Wrap("Bot Inventory", "Tier3ChestBotCost", "Base price of a legendary chest for the bot.", 400);
            EquipmentBuyChance = Config.Wrap("Bot Inventory", "EquipmentBuyChance", "Chance between 0 and 100 for a bot to buy from an equipment barrel instead of a tier 1 chest. Only active while the bot does not have a equipment item. (Default: 15)", 15);
            MinBuyingDelay = Config.Wrap("Bot Inventory", "MinBuyingDelay", "Minimum delay in seconds between the time it takes for a bot checks to buy an item.", 0f);
            MaxBuyingDelay = Config.Wrap("Bot Inventory", "MaxBuyingDelay", "Maximum delay in seconds between the time it takes for a bot checks to buy an item.", 5f);
            ShowBuyMessages = Config.Wrap("Bot Inventory", "ShowBuyMessages", "Displays whenever a bot buys an item in chat.", true);

            HostOnlySpawnBots = Config.Wrap("Misc", "HostOnlySpawnBots", "Set true so that only the host may spawn bots", true);
            ShowNameplates = Config.Wrap("Misc", "ShowNameplates", "Show player nameplates on playerbots if PlayerMode == false. (Host only)", true);

            PlayerMode = Config.Wrap("Player Mode", "PlayerMode", "Makes the game treat playerbots like how regular players are treated. The bots now show up on the scoreboard, can pick up items, influence the map scaling, etc.", false);
            DontScaleInteractables = Config.Wrap("Player Mode", "DontScaleInteractables", "Prevents interactables spawn count from scaling with bots. Only active is PlayerMode is true.", true);

            // Sanity check
            MaxBuyingDelay.Value = Math.Max(MaxBuyingDelay.Value, MinBuyingDelay.Value);

            // Add console commands
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            // Apply hooks
            PlayerBotHooks.AddHooks();
        }

        public void Start()
        {
            // Set survivor dict
            SurvivorDict.Add("mult", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("mul-t", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("toolbot", SurvivorCatalog.FindSurvivorIndex("Toolbot"));
            SurvivorDict.Add("hunt", SurvivorCatalog.FindSurvivorIndex("Huntress"));
            SurvivorDict.Add("huntress", SurvivorCatalog.FindSurvivorIndex(".Huntress"));
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

            // Add skill helpers
            AiSkillHelperCatalog.Populate();

            // Config
            InitialBots = new ConfigWrapper<int>[RandomSurvivorsList.Count];
            for (int i = 0; i < RandomSurvivorsList.Count; i++)
            {
                string name = BodyCatalog.GetBodyName(SurvivorCatalog.GetBodyIndexFromSurvivorIndex(RandomSurvivorsList[i]));
                InitialBots[i] = Config.Wrap("Starting Bots", "StartingBots." + name, "Starting amount of bots to spawn at the start of a run. (" + name + ")", 0);
            }

            // Equipments
            IndexManager.Build();

            ItemManager.usableEquipment = new EquipmentIndex[] {
                EquipmentCatalog.FindEquipmentIndex("CommandMissile"), EquipmentCatalog.FindEquipmentIndex("BFG"), EquipmentCatalog.FindEquipmentIndex("Lightning"), EquipmentCatalog.FindEquipmentIndex("CritOnUse"),
                EquipmentCatalog.FindEquipmentIndex("Blackhole"), EquipmentCatalog.FindEquipmentIndex("Fruit"), EquipmentCatalog.FindEquipmentIndex("GainArmor"), EquipmentCatalog.FindEquipmentIndex("Cleanse"),
                EquipmentCatalog.FindEquipmentIndex("PassiveHealing"), EquipmentCatalog.FindEquipmentIndex("TeamWarCry"), EquipmentCatalog.FindEquipmentIndex("DeathProjectile"), EquipmentCatalog.FindEquipmentIndex("LifestealOnHit")
            };
            ItemManager.equipmentPickups = ItemManager.usableEquipment.Select(e => PickupCatalog.FindPickupIndex(e)).Where(e => e != null).ToList();
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

                    // Random skin
                    SetRandomSkin(master, bodyPrefab);

                    // Set commponent values
                    master.SetFieldValue("aiComponents", gameObject.GetComponents<BaseAI>());
                    master.GiveMoney(owner.money);
                    master.inventory.CopyItemsFrom(owner.inventory);
                    master.inventory.RemoveItem(ItemCatalog.FindItemIndex("CaptainDefenseMatrix"), owner.inventory.GetItemCount(ItemCatalog.FindItemIndex("CaptainDefenseMatrix")));
                    master.inventory.GiveItem(ItemCatalog.FindItemIndex("DrizzlePlayerHelper"), 1);
                    master.destroyOnBodyDeath = false; // Allow the bots to spawn in the next stage

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
            spawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);
            //spawnRequest.summonerBodyObject = owner.GetBody().gameObject;

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
                    master.bodyPrefab = bodyPrefab;
                    SetRandomSkin(master, bodyPrefab);

                    master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);
                    master.teamIndex = TeamIndex.Player;

                    master.GiveMoney(owner.money);
                    master.inventory.CopyItemsFrom(owner.inventory);
                    master.inventory.RemoveItem(ItemCatalog.FindItemIndex("CaptainDefenseMatrix"), owner.inventory.GetItemCount(ItemCatalog.FindItemIndex("CaptainDefenseMatrix")));
                    master.inventory.GiveItem(ItemCatalog.FindItemIndex("DrizzlePlayerHelper"), 1);

                    // Allow the bots to spawn in the next stage
                    master.destroyOnBodyDeath = false;
                    master.gameObject.AddComponent<SetDontDestroyOnLoad>();
                }
                if (ai)
                {
                    ai.name = "PlayerBot";
                    ai.leader.gameObject = owner.GetBody().gameObject;

                    ai.neverRetaliateFriendlies = true;
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
            BodyIndex bodyIndex = bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            SkinDef[] skins = BodyCatalog.GetBodySkins(bodyIndex);
            master.loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, (uint)UnityEngine.Random.Range(0, skins.Length));
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
            skillHelper.AddCustomTargetLeash(gameObject, ai);
            skillHelper.InjectSkills(gameObject, ai);

            // Set skill drivers
            PropertyInfo property = typeof(BaseAI).GetProperty("skillDrivers");
            property.DeclaringType.GetProperty("skillDrivers");
            property.SetValue(ai, gameObject.GetComponents<AISkillDriver>(), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

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
                    randomSurvivorIndex = random.Next(0, RandomSurvivorsList.Count);
                }
                while (randomSurvivorIndex == lastCharacterType && RandomSurvivorsList.Count > 1);

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
