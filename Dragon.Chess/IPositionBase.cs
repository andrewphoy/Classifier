using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public interface IPositionBase {
        public long Id { get; set; }

        public bool WhiteToMove { get; }
        public bool CastlingWhiteKing { get; }
        public bool CastlingWhiteQueen { get; }
        public bool CastlingBlackKing { get; }
        public bool CastlingBlackQueen { get; }

        /// <summary>
        /// One-indexed column (a-h) for a valid en passant capture
        /// Zero means that no en passant is possible
        /// </summary>
        public byte EnPassantByte { get; }
    }
}
