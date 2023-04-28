using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.DB;
using System.IO.Streams;

namespace ChestSort
{
    [ApiVersion(2, 1)]
    public class ChestSortPlugin : TerrariaPlugin
    {
        public override string Author => "ToxicFrazzles";
        public override string Description => "A plugin to sort items in chests";
        public override string Name => "Chest Sort";
        public override Version Version => new Version(1,0,0,0);

        private List<Sorter>? Sorters = null;

        public ChestSortPlugin(Main game) : base(game)
        {
            
        }
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
            TShockAPI.Hooks.RegionHooks.RegionCreated += OnRegionCreated;
            TShockAPI.Hooks.RegionHooks.RegionDeleted += OnRegionDeleted;
        }
        private void OnGameInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command(SortCMD, "sort"));
        }


        private async void SortCMD(CommandArgs args)
        {
            if (args.Player.ActiveChest < 0)
            {
                args.Player.SendErrorMessage("Execute the command again with a chest open in the region to be sorted.");
                return;
            }
            if(Sorters == null)
            {
                Sorters = new List<Sorter>();
                foreach (Region region in TShock.Regions.Regions)
                {
                    Sorters.Add(new Sorter(region));
                }
            }

            //args.Player.SendWarningMessage("Checking {0} regions to sort", Sorters.Count);
            foreach(Sorter sorter in Sorters)
            {
                if (sorter.handlesChest(args.Player.ActiveChest))
                {
                    //args.Player.SendWarningMessage("Sorting {0}", sorter.Region.Name);
                    sorter.sort();
                    return;
                }
            }
        }


        private void OnRegionCreated(TShockAPI.Hooks.RegionHooks.RegionCreatedEventArgs args)
        {
            Sorters.Add(new Sorter(args.Region));
            //Console.WriteLine("Region Created");
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
            //Console.WriteLine("Region deleted");
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
                    if(Sorters is null) return;
                    foreach (Sorter sorter in Sorters)
                    {
                        if (sorter.handlesChest(ChestID) && sorter.sorting)
                        {
                            player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                            args.Handled = true;
                            return;
                        }
                    }
                }
                else if (args.MsgID == PacketTypes.ChestGetContents)
                {
                    if(Sorters is null) return;
                    foreach(Sorter sorter in Sorters)
                    {
                        if(sorter.handlesChest(data.ReadInt16(), data.ReadInt16()) && sorter.sorting)
                        {
                            player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                            args.Handled = true;
                            return;
                        }
                    }
                } else if(args.MsgID == PacketTypes.ChestItem)
                {
                    short ChestID = data.ReadInt16();
                    if (Sorters is null) return;
                    foreach (Sorter sorter in Sorters)
                    {
                        if (sorter.handlesChest(ChestID) && sorter.sorting)
                        {
                            player.SendWarningMessage("That chest is currently being sorted. Please try again later.");
                            args.Handled = true;
                            return;
                        }
                    }
                }
            }
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