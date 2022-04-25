using Dragon.Chess;
using MachineLearning;
using System;
using System.Collections.Generic;
using System.Text;

namespace Classifier.Models {
    public class LichessPuzzle {
        public string PuzzleId { get; set; }
        public string Fen { get; set; }
        
        public string MoveList { get; set; }
        public List<string> UciMoves { get; set; }

        public int Rating { get; set; }
        public int RatingDeviation { get; set; }
        public int Popularity { get; set; }
        public int NumPlays { get; set; }
        public string TagList { get; set; }
        public List<string> Tags { get; set; }
        public string SourceUrl { get; set; }

        public bool Initialized { get; set; } = false;

        public Position[] Positions { get; set; }
        public Move[] Moves { get; set; }
        

        public Position InitialPosition => Positions[1];
        public Move KeyMove => Moves[1];

        [Feature]
        public int StrongSidePieceCount { get; set; }
        [Feature]
        public int WeakSidePieceCount { get; set; }
        [Feature]
        public int StrongSidePawnCount { get; set; }
        [Feature]
        public int WeakSidePawnCount { get; set; }

    }
}
