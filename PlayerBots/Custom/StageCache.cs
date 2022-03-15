using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlayerBots.Custom
{
    class StageCache
    {
        public List<BotInteractable<PurchaseInteraction>> interactablesItem;
        //public List<BarrelInteraction> interactablesBarrel;

        public StageCache()
        {
            this.interactablesItem = new List<BotInteractable<PurchaseInteraction>>();
        }

        public void Update()
        {
            // Clear and update
            this.interactablesItem.Clear();
            //this.interactablesBarrel = InstanceTracker.GetInstancesList<BarrelInteraction>();

            // Process multishops
            MultiShopController[] multishops = GameObject.FindObjectsOfType<MultiShopController>();

            foreach (MultiShopController multishop in multishops)
            {
                GameObject[] terminalGameObjects = multishop.GetFieldValue<GameObject[]>("_terminalGameObjects");
                if (terminalGameObjects != null)
                {
                    foreach (GameObject terminal in terminalGameObjects)
                    {
                        // TODO: Check if pickup is higher priority
                        /*
                        ShopTerminalBehavior shop = terminal.GetComponent<ShopTerminalBehavior>();
                        PickupIndex pickup = shop.GetFieldValue<PickupIndex>("pickupIndex");
                        */

                        // FILLER
                        this.interactablesItem.Add(new BotInteractable<PurchaseInteraction>(terminal.gameObject));
                    }
                }
            }

            // Process chests
            ChestBehavior[] chests = GameObject.FindObjectsOfType<ChestBehavior>();

            foreach (ChestBehavior chest in chests)
            {
                interactablesItem.Add(new BotInteractable<PurchaseInteraction>(chest.gameObject));
            }
        }
    }

    public class BotInteractable<T> where T : IInteractable
    {
        public GameObject gameObject;
        public T Value;

        public BotInteractable(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.Value = gameObject.GetComponent<T>();
        }

        public BotInteractable(GameObject gameObject, T interactable)
        {
            this.gameObject = gameObject;
            this.Value = interactable;
        }
    }
}
