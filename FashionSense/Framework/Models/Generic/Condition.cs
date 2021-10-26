using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Generic
{
    public class Condition
    {
        public enum Type
        {
            Unknown,
            MovementSpeed,
            MovementDuration,
            RidingHorse
        }

        public Type Name { get; set; }
        public object Value { get; set; }
        public bool Independent { get; set; }

        internal T GetParsedValue<T>()
        {
            return (T)Value;
        }
    }
}
