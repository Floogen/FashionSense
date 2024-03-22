using FashionSense.Framework.Models.Messages;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class MessageManager
    {
        private IMonitor _monitor;
        private IModHelper _helper;
        private string _modID;

        internal enum MessageType
        {
            Unknown,
            ColorKeyChange,
            VanillaBootColorChange
        }

        public MessageManager(IMonitor monitor, IModHelper helper, string modID)
        {
            _monitor = monitor;
            _helper = helper;
            _modID = modID;
        }

        public void HandleIncomingMessage(ModMessageReceivedEventArgs e)
        {
            if (Enum.TryParse<MessageType>(e.Type, out var type) is false)
            {
                _monitor.LogOnce($"Failed to handle incoming message with type {e.Type}", LogLevel.Trace);
                return;
            }

            switch (type)
            {
                case MessageType.ColorKeyChange:
                    ProcessColorKeyChangeMessage(e.ReadAs<ColorKeyChangeMessage>());
                    return;
                case MessageType.VanillaBootColorChange:
                    ProcessVanillaBootColorChangeMessage(e.ReadAs<VanillaBootColorChangeMessage>());
                    return;
            }
        }

        private void ProcessColorKeyChangeMessage(ColorKeyChangeMessage message)
        {
            var farmer = Game1.getFarmer(message.FarmerID);
            if (farmer is null)
            {
                return;
            }
            FashionSense.colorManager.SetColor(farmer, message.ColorKey, message.ColorValue);
        }
        

        private void ProcessVanillaBootColorChangeMessage(VanillaBootColorChangeMessage message)
        {
            var farmer = Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == message.FarmerID);
            if (farmer is not null && farmer.FarmerRenderer is not null)
            {
                farmer.FarmerRenderer.MarkSpriteDirty();
            }
        }

        public void SendColorKeyChangeMessage(Farmer farmer, string colorKey, Color colorValue)
        {
            var colorChangeMessage = new ColorKeyChangeMessage()
            {
                FarmerID = farmer.UniqueMultiplayerID,
                ColorKey = colorKey,
                ColorValue = colorValue
            };
            _helper.Multiplayer.SendMessage(colorChangeMessage, MessageType.ColorKeyChange.ToString(), modIDs: new[] { FashionSense.modManifest.UniqueID });
        }

        public void SendVanillaBootColorChangeMessage(Farmer farmer)
        {
            if (farmer is null)
            {
                return;
            }

            var message = new VanillaBootColorChangeMessage()
            {
                FarmerID = farmer.UniqueMultiplayerID
            };
            _helper.Multiplayer.SendMessage(message, MessageType.VanillaBootColorChange.ToString(), modIDs: new[] { _modID });
        }
    }
}
