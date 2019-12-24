using RoR2;
using System.Collections.Generic;
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

        private static readonly EquipmentIndex[] usableEquipment = new EquipmentIndex[] {
            EquipmentIndex.CommandMissile, EquipmentIndex.BFG, EquipmentIndex.Lightning, EquipmentIndex.CritOnUse,
            EquipmentIndex.Blackhole, EquipmentIndex.Fruit, EquipmentIndex.GainArmor, EquipmentIndex.Cleanse,
            EquipmentIndex.PassiveHealing
        };

        public void Awake()
        {
            this.master = base.gameObject.GetComponent<CharacterMaster>();
            this.maxPurchases = PlayerBotManager.MaxBotPurchasesPerStage.Value;

            this.chestPicker = new WeightedSelection<ChestTier>();
            this.chestPicker.AddChoice(ChestTier.WHITE, PlayerBotManager.Tier1ChestBotWeight.Value);
            this.chestPicker.AddChoice(ChestTier.GREEN, PlayerBotManager.Tier2ChestBotWeight.Value);
            this.chestPicker.AddChoice(ChestTier.RED, PlayerBotManager.Tier3ChestBotWeight.Value);

            ResetPurchases();
        }

        public void FixedUpdate()
        {
            CheckBuy();
        }

        public void ResetPurchases()
        {
            this.ResetChest();
            this.maxPurchases = GetMaxPurchases();
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
            if (!master.alive)
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
                        this.master.inventory.SetEquipmentIndex(ItemManager.usableEquipment[UnityEngine.Random.Range(0, ItemManager.usableEquipment.Length)]);
                        return;
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
                ItemIndex item = PickupCatalog.GetPickupDef(dropPickup).itemIndex;
                this.master.inventory.GiveItem(item, 1);
                Debug.Log(this.master.GetBody().GetUserName() + " bought a " + item);
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
