namespace FashionSense.Framework.Models.Appearances
{
    public class SubFrame
    {
        public enum Type
        {
            Unknown,
            Normal,
            Slingshot
        }


        public int Frame { get; set; }
        public Type Handling { get; set; } = Type.Normal;
    }
}
