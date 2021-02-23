using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class Tactic {

        public string Fen { get; set; }
        public int ColorToMove { get; set; }
        public string BestMove { get; set; }

        public int? ScoreCp { get; set; }
        public int? ScoreMate { get; set; }

        public string LastPositionFen { get; set; }
        public string LastMove { get; set; }
        
        public List<string> SuccessfulMoves { get; set; }
        public string GameMove { get; set; }

        public PgnGame PgnGame { get; set; }
    }
}
