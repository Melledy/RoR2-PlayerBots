using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerBots
{
    class ItemManager : MonoBehaviour
    {
        public CharacterMaster master;
        private WeightedSelection<ChestTier> chestPicker;

        private ChestTier nextChestTier = ChestTier.WHITE;
        private int nextChestPrice = 25;
        private int purchases = 0;
        private int maxPurchases = 8;

        private Run.FixedTimeStamp lastBuyCheck;
        private float buyingDelay;

        public static EquipmentIndex[] usableEquipment;
        public static List<PickupIndex> equipmentPickups;

        public void Awake()
        {
            this.master = base.gameObject.GetComponent<CharacterMaster>();
            this.maxPurchases = PlayerBotManager.MaxBotPurchasesPerStage.Value;

            this.chestPicker = new WeightedSelection<ChestTier>();
            this.chestPicker.AddChoice(ChestTier.WHITE, PlayerBotManager.Tier1ChestBotWeight.Value);
            this.chestPicker.AddChoice(ChestTier.GREEN, PlayerBotManager.Tier2ChestBotWeight.Value);
            this.chestPicker.AddChoice(ChestTier.RED, PlayerBotManager.Tier3ChestBotWeight.Value);

            ResetPurchases();
            ResetBuyingDelay();
        }

        public void FixedUpdate()
        {
            if (this.lastBuyCheck.timeSince >= this.buyingDelay)
            {
                CheckBuy();
                ResetBuyingDelay();
            }
        }

        public void ResetPurchases()
        {
            this.ResetChest();
            this.maxPurchases = GetMaxPurchases();
        }

        private void ResetBuyingDelay()
        {
            this.lastBuyCheck = Run.FixedTimeStamp.now;
            this.buyingDelay = Random.Range(PlayerBotManager.MinBuyingDelay.Value, PlayerBotManager.MaxBuyingDelay.Value);
        }

        public void ResetChest()
        {
            this.nextChestTier = this.chestPicker.Evaluate(UnityEngine.Random.value);
            switch (this.nextChestTier)
            {
                case ChestTier.WHITE:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(PlayerBotManager.Tier1ChestBotCost.Value);
                    break;
                case ChestTier.GREEN:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(PlayerBotManager.Tier2ChestBotCost.Value);
                    break;
                case ChestTier.RED:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(PlayerBotManager.Tier3ChestBotCost.Value);
                    break;
            }
        }

        private int GetMaxPurchases()
        {
            return PlayerBotManager.MaxBotPurchasesPerStage.Value * (RoR2.Run.instance.stageClearCount + 1);
        }

        private void CheckBuy()
        {
            if (master.IsDeadAndOutOfLivesServer())
            {
                return;
            }

            // Max purchases for this map reached
            if (this.purchases >= this.maxPurchases)
            {
                return;
            }

            uint price = (uint)this.nextChestPrice;
            if (this.master.money >= price)
            {
                Buy(this.nextChestTier);
                this.master.money -= price;
                this.purchases++;
                ResetChest();
            }
        }

        private void Buy(ChestTier chestTier)
        {
            // Get drop list
            List<PickupIndex> dropList = null;
            switch (chestTier)
            {
                case ChestTier.WHITE:
                    if (this.master.inventory.currentEquipmentIndex == EquipmentIndex.None && PlayerBotManager.EquipmentBuyChance.Value > UnityEngine.Random.Range(0, 100))
                    {
                        dropList = Run.instance.availableEquipmentDropList;
                        break;
                    }
                    dropList = Run.instance.smallChestDropTierSelector.Evaluate(UnityEngine.Random.value);
                    break;
                case ChestTier.GREEN:
                    dropList = Run.instance.mediumChestDropTierSelector.Evaluate(UnityEngine.Random.value);
                    break;
                case ChestTier.RED:
                    dropList = Run.instance.largeChestDropTierSelector.Evaluate(UnityEngine.Random.value);
                    break;
            }

            // Pickup
            if (dropList != null && dropList.Count > 0)
            {
                PickupIndex dropPickup = Run.instance.treasureRng.NextElementUniform<PickupIndex>(dropList);
                PickupDef pickup = PickupCatalog.GetPickupDef(dropPickup);
                if (pickup.itemIndex != ItemIndex.None)
                {
                    this.master.inventory.GiveItem(pickup.itemIndex, 1);
                }
                else if (pickup.equipmentIndex != EquipmentIndex.None)
                {
                    this.master.inventory.SetEquipmentIndex(pickup.equipmentIndex);
                }
                else
                {
                    // Neither item nor valid equipment
                    return;
                }
                // Chat
                if (PlayerBotManager.ShowBuyMessages.Value)
                {
                    PickupDef pickupDef = PickupCatalog.GetPickupDef(dropPickup);
                    Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
                    {
                        subjectAsCharacterBody = this.master.GetBody(),
                        baseToken = "PLAYER_PICKUP",
                        pickupToken = ((pickupDef != null) ? pickupDef.nameToken : null) ?? PickupCatalog.invalidPickupToken,
                        pickupColor = (pickupDef != null) ? pickupDef.baseColor : Color.black,
                        pickupQuantity = pickup.itemIndex != ItemIndex.None ? (uint)this.master.inventory.GetItemCount(pickup.itemIndex) : 1
                    });
                }
            }
        }

        enum ChestTier
        {
            WHITE = 0,
            GREEN = 1,
            RED = 2
        }
    }
}
