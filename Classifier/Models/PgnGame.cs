using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class PgnGame {

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string PgnBody { get; set; }
        public List<string> RawHeaders { get; set; } = new List<string>();

        public string WhitePlayerName { get; set; }
        public string BlackPlayerName { get; set; }
        public string EcoRaw { get; set; }

        public int? PlyCount { get; set; }
        public string Annotator { get; set; }
        public string Site { get; set; }


        public List<string> Errors { get; set; }
        public bool HasErrors { get; set; }
    }
}
