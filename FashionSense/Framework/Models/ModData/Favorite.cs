using FashionSense.Framework.Interfaces.API;

namespace FashionSense.Framework.Models
{
    public class Favorite
    {
        public IApi.Type Type { get; set; }
        public string Id { get; set; }

        public Favorite()
        {

        }

        public Favorite(IApi.Type type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}
