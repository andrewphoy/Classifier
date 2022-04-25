using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class PgnOptions {
        public bool RemoveComments { get; set; } = false;
        public bool RemoveNags { get; set; } = false;
        public bool RemoveAnnotations { get; set; } = false;
        public bool RemoveEngineEvaluations { get; set; } = false;
        public bool RemoveClockTimes { get; set; } = false;

        public static PgnOptions Default {
            get => new PgnOptions();
        }

    }
}
