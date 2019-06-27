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

        public void Awake()
        {
            this.master = base.gameObject.GetComponent<CharacterMaster>();
            this.maxPurchases = PlayerBotManager.MaxBotPurchasesPerStage.Value;

            this.chestPicker = new WeightedSelection<ChestTier>();
            this.chestPicker.AddChoice(ChestTier.WHITE, 0.8f);
            this.chestPicker.AddChoice(ChestTier.GREEN, 0.2f);

            ResetPurchases();
        }

        public void FixedUpdate()
        {
            CheckBuy();
        }

        public void ResetPurchases()
        {
            this.ResetChest();
            this.purchases = 0;
        }

        public void ResetChest()
        {
            this.nextChestTier = this.chestPicker.Evaluate(UnityEngine.Random.value);
            switch (this.nextChestTier)
            {
                case ChestTier.WHITE:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(25);
                    break;
                case ChestTier.GREEN:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(50);
                    break;
                case ChestTier.RED:
                    this.nextChestPrice = Run.instance.GetDifficultyScaledCost(400);
                    break;
            }
        }

        private void CheckBuy()
        {
            if (!master.alive)
            {
                return;
            }

            // Max purchases for this map reached
            if (this.purchases >= PlayerBotManager.MaxBotPurchasesPerStage.Value)
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
                this.master.inventory.GiveItem(dropPickup.itemIndex, 1);
                Debug.Log(this.master.GetBody().GetUserName() + " bought a " + dropPickup.itemIndex);
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
