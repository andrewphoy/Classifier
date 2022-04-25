using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NagAttribute : Attribute {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NagNumber { get; set; }
        public int[] AdditionalNumbers { get; set; }

    }
}
