using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dragon.Models;

namespace Dragon.Chess {
    public class PgnGame : Game {
        public PgnGame() {
            this.RawHeaders = new List<string>();
            this.AdditionalHeaders = new Dictionary<string, string>();
            this.Event = new Event();
            this.Errors = new List<string>();
            this.HasErrors = false;
        }

        public List<string> RawHeaders { get; set; }
        public Dictionary<string, string> AdditionalHeaders { get; set; }
        public string RawBody { get; set; }

        // pgn headers
        public string StrEvent { get; set; }
        public string EcoRaw { get; set; }
        public string Site { get; set; }

        public List<string> Errors { get; set; }
        public bool HasErrors { get; set; }
    }
}
