using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.DB;
using System.IO.Streams;
using static MonoMod.InlineRT.MonoModRule;
using Chest_Sort;
using TShockAPI.Hooks;
using System.Diagnostics;

public class ChestCloseEventArgs
{
    public ChestCloseEventArgs(TSPlayer player, short chestID)
    {
        Player = player;
        ChestID = chestID;
    }

    public TSPlayer Player { get; }
    public short ChestID { get; }
}

namespace ChestSort
{
    [ApiVersion(2, 1)]
    public class ChestSortPlugin : TerrariaPlugin
    {
        public override string Author => "ToxicFrazzles";
        public override string Description => "A plugin to sort items in chests";
        public override string Name => "Chest Sort";
        public override Version Version => new Version(1,0,5,1);

        private List<Sorter>? Sorters = null;

        public delegate void ChestCloseEventHandler(object sender, ChestCloseEventArgs args);
        public event ChestCloseEventHandler ChestClose;

        public ChestSortPlugin(Main game) : base(game)
        {
            
        }
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            TShockAPI.Hooks.RegionHooks.RegionCreated += OnRegionCreated;
            TShockAPI.Hooks.RegionHooks.RegionDeleted += OnRegionDeleted;
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += ReloadEventHandler;
        }


        private void TestHook(EventArgs args)
        {
            Log.Debug("Test Hook Executed! Regions: {0}", TShock.Regions.Regions.Count);
        }

        private void OnGameInitialize(EventArgs args)
        {
            // Add the "sort" command to the chat commands
            Commands.ChatCommands.Add(new Command(SortCMD, "sort"));
            Commands.ChatCommands.Add(new Command(PauseSortCMD, "pausesort"));

        }

        private void OnServerJoin(EventArgs args)
        {
            // Couldn't find a nice way to initialise the sorters after the game has loaded and the regions have initialised
            // So we just initialise the sorters when the first player joins :)

            if (Sorters == null)
            {
                // If the sorters haven't been initialised yet
                Config.Reload();    // Reload the categories config file
                Sorters = new List<Sorter>();   // Create a new list to store the sorters
                foreach (Region region in TShock.Regions.Regions)
                {
                    // Add a sorter for each region
                    Sorters.Add(new Sorter(this, region));
                }
            }
        }

        private void ReloadEventHandler(ReloadEventArgs args)
        {
            Log.Debug("Reload event");
            Config.Reload();

            foreach (Sorter sorter in Sorters)
            {
                sorter.Delete();
            }
            Sorters = new List<Sorter>();
            foreach (Region region in TShock.Regions.Regions)
            {
                Sorters.Add(new Sorter(this, region));
            }
        }


        private async void SortCMD(CommandArgs args)
        {
            TSPlayer player = args.Player;
            if (player.ActiveChest < 0)
            {
                player.SendErrorMessage("Execute the command again with a chest open in the region to be sorted.");
                return;
            }

            player.SendDebugMessage("Checking {0} regions to sort", Sorters.Count);
            foreach (Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(player.ActiveChest))
                {
                    player.SendDebugMessage("Sorting {0}", sorter.Region.Name);
                    sorter.paused = false;
                    await sorter.sortCmd();
                    return;
                }
            }
        }

        private async void PauseSortCMD(CommandArgs args)
        {
            TSPlayer player = args.Player;
            if (player.ActiveChest < 0)
            {
                player.SendErrorMessage("Execute the command again with a chest open in the region to be sorted.");
                return;
            }
            player.SendDebugMessage("Checking {0} regions to pause sorting", Sorters.Count);
            foreach (Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(player.ActiveChest))
                {
                    player.SendDebugMessage("Pausing sorting in region: {0}", sorter.Region.Name);
                    sorter.paused = true;
                    return;
                }
            }
        }

        private void OnRegionCreated(TShockAPI.Hooks.RegionHooks.RegionCreatedEventArgs args)
        {
            Sorters.Add(new Sorter(this, args.Region));
            Log.Debug("Region Created");
        }
        private void OnRegionDeleted(TShockAPI.Hooks.RegionHooks.RegionDeletedEventArgs args)
        {
            foreach(Sorter sorter in Sorters)
            {
                if(sorter.Region == args.Region)
                {
                    Sorters.Remove(sorter);
                    return;
                }
            }
            Log.Debug("Region deleted");
        }

        private void OnGetData(GetDataEventArgs args)
        {
            var player = TShock.Players[args.Msg.whoAmI];
            if (player == null || !player.ConnectionAlive)
            {
                return;
            }
            using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                if (args.MsgID == PacketTypes.ChestOpen)
                {
                    short ChestID = data.ReadInt16();
                    args.Handled = ChestOpenHandler(player, ChestID);
                }
                else if (args.MsgID == PacketTypes.ChestGetContents)
                {
                    short xpos = data.ReadInt16();
                    short ypos = data.ReadInt16();
                    args.Handled = ChestGetContentsHandler(player, xpos, ypos);
                } else if(args.MsgID == PacketTypes.ChestItem)
                {
                    short ChestID = data.ReadInt16();
                    args.Handled = ChestItemHandler(player, ChestID);
                    
                }else if(args.MsgID == PacketTypes.PlaceChest)
                {
                    int action = data.ReadByte();
                    short xpos = data.ReadInt16();
                    short ypos = data.ReadInt16();
                    short style = data.ReadInt16();
                    short chestIDToDestroy = data.ReadInt16();
                    args.Handled = PlaceChestHandler(player, action, xpos, ypos, style, chestIDToDestroy);
                }
            }
        }


        private bool ChestOpenHandler(TSPlayer player, short ChestID)
        {
            if(ChestID < 0 && player.ActiveChest >= 0)
            {
                ChestClose?.Invoke(this, new ChestCloseEventArgs(player, (short)player.ActiveChest));
                Log.Debug("Chest close: {0}", player.ActiveChest);
                return false;
            }

            Log.Debug("Chest Open: {0}", ChestID);
            foreach (Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(ChestID) && sorter.sorting)
                {
                    player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                    return true;
                }
            }
            return false;
        }

        private bool ChestGetContentsHandler(TSPlayer player, short xpos, short ypos)
        {
            Log.Debug("Get Contents: ({0}, {1})", xpos, ypos);
            foreach (Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(xpos, ypos) && sorter.sorting)
                {
                    player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                    return true;
                }
            }
            return false;
        }

        private bool ChestItemHandler(TSPlayer player, short ChestID)
        {
            Log.Debug("Chest Item: {0}", ChestID);
            foreach (Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(ChestID) && sorter.sorting)
                {
                    player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                    return true;
                }
            }
            return false;
        }

        private bool PlaceChestHandler(TSPlayer player, int action, short xpos, short ypos, short style, short chestIDToDestroy)
        {
            bool placeChest = (action & 0b1) != 0;
            bool destroyChest = (action & 0b10) != 0;
            bool placeDresser = (action & 0b100) != 0;
            bool destroyDresser = (action & 0b1000) != 0;
            bool placeContainers2 = (action & 0b10000) != 0;
            bool destroyContainers2 = (action & 0b100000) != 0;

            if (placeChest)
            {
                Log.Debug("Place chest: ({0}, {1}) {2}", xpos, ypos, chestIDToDestroy);
            }
            if (destroyChest)
            {
                Log.Debug("Destroy chest: {0}", chestIDToDestroy);
            }
            if (placeDresser)
            {
                Log.Debug("Place dresser: ({0}, {1}) {2}", xpos, ypos,chestIDToDestroy);
            }
            if(destroyDresser)
            {
                Log.Debug("Destroy dresser: {0}", chestIDToDestroy);
            }
            if (placeContainers2)
            {
                Log.Debug("Place containers2: ({0}, {1}) {2}", xpos, ypos,chestIDToDestroy);
            }
            if (destroyContainers2)
            {
                Log.Debug("Destroy containers2: {0}", chestIDToDestroy);
            }

            return false;
        }

        /// <summary>
        /// Handles plugin disposal
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnGameInitialize);
                TShockAPI.Hooks.RegionHooks.RegionCreated -= OnRegionCreated;
                TShockAPI.Hooks.RegionHooks.RegionDeleted -= OnRegionDeleted;
            }
            base.Dispose(disposing);
        }
    }
}