﻿using Dungeonator;
using JuneLib.Items;
using System.Collections.Generic;
using System.Linq;

namespace Oddments
{
    public class AbsorptionPlayerItem : PlayerItem
    {
        public static OddItemTemplate template = new OddItemTemplate(typeof(AbsorptionPlayerItem))
        {
            Name = "Siphon Item",
            Description = "Item Absoprtion",
            LongDescription = "Destroys any items and takes their power. Any stats within the items will be permanently applied to the player. Any active effects in the item will be stored in the book",
            SpriteResource = $"{Module.SPRITE_PATH}/siphonitem.png",
            Quality = ItemQuality.D,
            Cooldown = 325,
        };

        public bool GunAbsoprtion = false;

        public override bool CanBeUsed(PlayerController user)
        {
            if (user == null || user.CurrentRoom == null)
            {
                return false;
            }
            if (succedActives.Count > 0)
            {
                foreach (var succed in succedActives)
                {
                    if ((PickupObjectDatabase.GetById(succed) as PlayerItem).CanBeUsed(user))
                    {
                        return base.CanBeUsed(user);
                    }
                }
            }
            IPlayerInteractable iplayer = user.CurrentRoom?.GetNearestInteractable(user.specRigidbody.UnitCenter, 1f, user);
            bool appropriateTypes = (iplayer is PlayerItem
                || iplayer is PassiveItem);
            if (GunAbsoprtion)
            {
                appropriateTypes = iplayer is Gun;
            }
            if (appropriateTypes)
            {

                return base.CanBeUsed(user);
            }
            return false;
        }

        private List<int> succedActives = new List<int>();

        public override void MidGameSerialize(List<object> data)
        {
            base.MidGameSerialize(data);
            data.Add(succedActives);
        }

        public override void MidGameDeserialize(List<object> data)
        {
            base.MidGameDeserialize(data);
            succedActives = (List<int>)data[0];
        }

        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);

            if (succedActives.Count > 0)
            {
                foreach (var succed in succedActives)
                {
                    PlayerItem item = PickupObjectDatabase.GetById(succed) as PlayerItem;
                    if (RealFakeItemHelper.UseFakeItem(user, item))
                    {
                        if (item.consumable)
                        {
                            succedActives.Remove(succed);
                        }
                    }
                }
            }
            PickupObject items = user.CurrentRoom.GetNearestInteractable(user.specRigidbody.UnitCenter, 1f, user) as PickupObject;
            if (items)
            {
                if ((items is PlayerItem || items is PassiveItem))
                {
                    List<StatModifier> stats;
                    if (items is PlayerItem playerItem)
                    {
                        stats = playerItem.passiveStatModifiers.ToList();
                        succedActives.Add(items.PickupObjectId);
                    }
                    else
                    {
                        PassiveItem passiveItem = (PassiveItem)items;
                        stats = passiveItem.passiveStatModifiers.ToList();
                    }
                    user.ownerlessStatModifiers.AddRange(stats);
                    user.stats.RecalculateStats(user);
                } else if (GunAbsoprtion)
                {

                }

                LootEngine.DoDefaultPurplePoof(items.sprite.WorldCenter);
                Destroy(items.gameObject);
                RoomHandler.unassignedInteractableObjects.Remove((IPlayerInteractable)items);
            }
        }
    }
}
