using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Models.Generic
{
    public class Condition
    {
        public enum Type
        {
            Unknown,
            MovementSpeed,
            MovementDuration
        }

        public Type Name { get; set; }
        public object Value { get; set; }
        public bool Independent { get; set; }

        internal dynamic GetParsedValue<T>()
        {
            switch (Value)
            {
                case Type.MovementSpeed:
                    return (float)Value;
                case Type.MovementDuration:
                    return (float)Value;
                default:
                    return Value;
            }
        }
    }
}
