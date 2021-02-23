using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class PositionData {
        /// <summary>
        /// The full FEN
        /// </summary>
        public string Fen { get; set; }

        /// <summary>
        /// FEN without move clock and move number
        /// </summary>
        public string FenKey { get; set; }
        public int ColorToMove { get; set; }

        public int? ScoreCp { get; set; }
        public int? ScoreMate { get; set; }
        
        //TODO add more here?
        public string LastMove { get; set; }

        public string GameMove { get; set; }
    }
}
