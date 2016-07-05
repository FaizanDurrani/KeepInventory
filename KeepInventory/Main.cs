using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.API;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepInventory
{
    public class Main : RocketPlugin
    {

        public const string LOGGER_PREFIX = "[KEEPINVENTORY]: ";

        private List<UnturnedPlayer> deadAdmins = new List<UnturnedPlayer> ();
        private Dictionary<UnturnedPlayer, List<Item>> adminItems = new Dictionary<UnturnedPlayer, List<Item>> ();
        protected override void Load()
        {
            UnturnedPlayerEvents.OnPlayerRevive += Give;
            UnturnedPlayerEvents.OnPlayerDeath += ClearInventory;

            Logger.Log (LOGGER_PREFIX + "LOADED!");
        }
        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerRevive -= Give;
            UnturnedPlayerEvents.OnPlayerDeath -= ClearInventory;

            Logger.Log (LOGGER_PREFIX + "UNLOADED!");
        }

        private void Give(UnturnedPlayer player, UnityEngine.Vector3 position, byte angle)
        {
            for (int i = 0; i < deadAdmins.Count; i++)
            {
                if (deadAdmins[i].CSteamID.ToString () == player.CSteamID.ToString () && deadAdmins[i].CharacterName == player.CharacterName)
                {
                    for (int j = 0; j < adminItems[deadAdmins[i]].Count; j++)
                    {
                        if (adminItems[deadAdmins[i]][j] == null)
                            continue;

                        player.Inventory.forceAddItem (adminItems[deadAdmins[i]][j], true);
                    }

                    adminItems.Remove (player);
                    deadAdmins.RemoveAt (i);
                    break;
                }
            }
        }


        private void ClearInventory(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (!player.HasPermission ("keepinventory.keep"))
            {
                return;
            }

            var playerInventory = player.Inventory;
            List<Item> ids = new List<Item> ();
            List<Item> clothes = new List<Item> ();

            // "Remove "models" of items from player "body""
            player.Player.channel.send ("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, ( byte ) 0, ( byte ) 0, new byte[0]);
            player.Player.channel.send ("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, ( byte ) 1, ( byte ) 0, new byte[0]);

            // Remove items
            for (byte page = 0; page < 8; page++)
            {
                var count = playerInventory.getItemCount (page);

                for (byte index = 0; index < count; index++)
                {
                    ids.Add (playerInventory.getItem (page, 0).item);
                    playerInventory.removeItem (page, 0);
                }
            }

            // Unequip & remove from inventory
            player.Player.clothing.askWearBackpack (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearGlasses (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearHat (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearPants (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearMask (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearShirt (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));

            player.Player.clothing.askWearVest (0, 0, new byte[0], true);
            clothes.Add (removeUnequipped (playerInventory));
            clothes.AddRange (ids);
            deadAdmins.Add (player);
            adminItems.Add (player, clothes);
        }

        private Item removeUnequipped(PlayerInventory playerInventory)
        {
            for (byte i = 0; i < playerInventory.getItemCount (2); i++)
            {
                Item item = playerInventory.getItem (2, 0).item;
                playerInventory.removeItem (2, 0);
                return item;
            }

            return null;
        }
    }
}
