using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models;
using FashionSense.Framework.Utilities;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class FavoritesManager
    {
        private IMonitor _monitor;

        public FavoritesManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public void AddFavorite(Farmer who, IApi.Type type, string id)
        {
            if (IsFavorite(who, type, id))
            {
                return;
            }

            var favorites = GetAllFavorites(who);
            favorites.Add(new Favorite(type, id));

            SerializeFavorites(who, favorites);
        }

        public void RemoveFavorite(Farmer who, IApi.Type type, string id)
        {
            var favorites = GetAllFavorites(who);
            if (favorites.RemoveAll(f => f.Type == type && f.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) == 0)
            {
                return;
            }

            SerializeFavorites(who, favorites);
        }

        public void ToggleFavorite(Farmer who, IApi.Type type, string id)
        {
            if (IsFavorite(who, type, id))
            {
                RemoveFavorite(who, type, id);
            }
            else
            {
                AddFavorite(who, type, id);
            }
        }

        public bool IsFavorite(Farmer who, IApi.Type type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return GetAllFavorites(who).Any(f => f.Type == type && f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetFavorites(Farmer who, IApi.Type type)
        {
            return GetAllFavorites(who).Where(f => f.Type == type).Select(f => f.Id).ToList();
        }

        public List<Favorite> GetAllFavorites(Farmer who)
        {
            if (who.modData.ContainsKey(ModDataKeys.FAVORITES) is false || string.IsNullOrEmpty(who.modData[ModDataKeys.FAVORITES]))
            {
                return new List<Favorite>();
            }

            return JsonConvert.DeserializeObject<List<Favorite>>(who.modData[ModDataKeys.FAVORITES]) ?? new List<Favorite>();
        }

        private void SerializeFavorites(Farmer who, List<Favorite> favorites)
        {
            who.modData[ModDataKeys.FAVORITES] = JsonConvert.SerializeObject(favorites);
        }
    }
}
