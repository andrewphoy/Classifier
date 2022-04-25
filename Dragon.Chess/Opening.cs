using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class Opening {
        public int Id { get; set; }
        public string Fen { get; set; }
        public string Eco { get; set; }
        public string ShortName { get; set; }
        public string Variation { get; set; }
        public string SubVariation { get; set; }
    }
}
