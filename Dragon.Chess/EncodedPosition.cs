using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class EncodedPosition {

        public long Id { get; set; }

        public int[] Ranks { get; set; }
        
        public byte WhitePieceCount { get; set; }
        public byte BlackPieceCount { get; set; }
        public byte WhitePawnCount { get; set; }
        public byte BlackPawnCount { get; set; }

        public bool WhiteToMove { get; set; }
        public bool CastlingWhiteKing { get; set; }
        public bool CastlingWhiteQueen { get; set; }
        public bool CastlingBlackKing { get; set; }
        public bool CastlingBlackQueen { get; set; }

        /// <summary>
        /// One-indexed column (a-h) for a valid en passant capture
        /// Zero means that no en passant is possible
        /// </summary>
        public byte EnPassant { get; set; }

        public int RankOne { get; set; }
        public int RankTwo { get; set; }
        public int RankThree { get; set; }
        public int RankFour { get; set; }
        public int RankFive { get; set; }
        public int RankSix { get; set; }
        public int RankSeven { get; set; }
        public int RankEight { get; set; }

        public override int GetHashCode() {
            unchecked {
                int hash = WhiteToMove ? 3 : 7;
                for (int i = 0; i < 8; i++) {
                    hash = (hash * 17) ^ Ranks[i];
                }
                return hash;
            }
        }

        public override bool Equals(object obj) {
            if (!(obj is EncodedPosition other)) {
                return false;
            }

            if (WhiteToMove != other.WhiteToMove) {
                return false;
            }

            for (int i = 0; i < 8; i++) {
                if (Ranks[i] != other.Ranks[i]) {
                    return false;
                }
            }

            if (CastlingWhiteKing != other.CastlingWhiteKing)
                return false;
            if (CastlingWhiteQueen != other.CastlingWhiteQueen)
                return false;
            if (CastlingBlackKing != other.CastlingBlackKing)
                return false;
            if (CastlingBlackQueen != other.CastlingBlackQueen)
                return false;

            if (EnPassant != other.EnPassant)
                return false;


            return true;
        }
    }
}
