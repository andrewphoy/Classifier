using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class PgnGame {

        public string Pgn { get; set; }

        public string PgnBody { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public PgnGame() {
            Headers = new Dictionary<string, string>();
        }

        public void AddHeader(string name, string value) {
            var key = name.ToLowerInvariant();
            if (!Headers.ContainsKey(key)) {
                Headers[key] = value;
            }
        }
    }
}
