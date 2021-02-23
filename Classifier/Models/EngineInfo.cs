using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class EngineInfo {
        public int MultiPv { get; set; }
        public int Depth { get; set; }
        public int Seldepth { get; set; }
        public int? ScoreCp { get; set; }
        public int? ScoreMate { get; set; }
        public long Nodes { get; set; }
        public long NodesPerSecond { get; set; }
        public int TableBaseHits { get; set; }
        public int HashFull { get; set; }
        public long ElapsedMs { get; set; }
        public string Variation { get; set; }

    }
}
