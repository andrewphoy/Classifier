using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class EngineEval {

        public EngineEval() {
            Variations = new Dictionary<int, EngineInfo>();
        }

        public int ColorToMove { get; set; }

        /// <summary>
        /// The best move from the engine, reported as squares (ex. e2e4)
        /// </summary>
        public string BestMove { get; set; }
        public string EvalString {
            get {
                if (Variations.ContainsKey(1)) {
                    int normalizingFactor = ColorToMove;
                    if (normalizingFactor == 0) {
                        normalizingFactor = 1;
                    }

                    var var = Variations[1];
                    if (var.ScoreCp.HasValue) {
                        decimal score = ((decimal)var.ScoreCp.Value * normalizingFactor) / 100;
                        return score.ToString();
                    } else if (var.ScoreMate.HasValue) {
                        int distToMate = var.ScoreMate.Value * normalizingFactor;
                        return "#" + distToMate.ToString();
                    }

                    return "";

                } else {
                    return "";
                }
            }
        }

        public int? Cp => Variations.ContainsKey(1) ? Variations[1].ScoreCp : null;
        public int? Mate => Variations.ContainsKey(1) ? Variations[1].ScoreMate : null;

        public Dictionary<int, EngineInfo> Variations { get; set; }
    }
}
