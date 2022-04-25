using Dragon.Chess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dragon.Chess {
    [DebuggerDisplay("{San} {Uci}")]
    public class Move {

        public long GameId { get; set; }
        public long MoveId { get; set; }
        public long PositionId { get; set; }

        public string Fen { get; set; }

        public byte[] EncodedPosition { get; set; }

        public short UciEncoded {
            get {
                BitArray bits = new BitArray(16);

                int mbStart = (Start / 16) * 8 + (Start % 16);
                int mbEnd = (End / 16) * 8 + (End % 16);

                for (int i = 0; i < 6; i++) {
                    bits[i] = (mbStart & (1 << i)) > 0;
                }
                for (int i = 0; i < 6; i++) {
                    bits[i + 6] = (mbEnd & (1 << i)) > 0;
                }

                if (Piece == 0) {
                    throw new ArgumentOutOfRangeException("Piece");
                }

                // fix as of 3/26...
                // db values prior to 3/26 are wrong
                int pieceVal = (Piece < 0) ? (Piece * -1) : Piece;

                bits[12] = (pieceVal & 1) > 0;
                bits[13] = (pieceVal & 2) > 0;
                bits[14] = (pieceVal & 4) > 0;

                if (IsPromotion) {
                    bits[15] = true;
                }

                byte[] bytes = new byte[2];
                bits.CopyTo(bytes, 0);
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        public int Clock { get; set; }

        public string MoveBody { get; set; }
        public string Uci { get; set; }
        public string San { get; set; }

        public int ParentMoveId { get; set; }
        public int Ply { get; set; }
        public int HalfMoveCount { get; set; }
        public string NagString { get; set; }
        public string Comment { get; set; }

        public Position Position { get; set; }
        public Position ResultingPosition { get; set; }
        public IEnumerable<Move> Children { get; set; }

        // might be worth killing the stuff below and figuring it out in typescript

        //private long _transitions;
        //public long Transitions {
        //    get => _transitions;
        //    set {
        //        _transitions = value;
        //        ushort[] arr = { (ushort)(_transitions & 0xFFFF), (ushort)((_transitions >> 16) & 0xFFFF), (ushort)((_transitions >> 32) & 0xFFFF), (ushort)((_transitions >> 48) & 0xFFFF) };
        //        TransitionsArray = arr.Where(v => v != 0).ToArray();
        //    }
        //}

        //public ushort[] TransitionsArray { get; set; }

        public int Start { get; internal set; }
        public int End { get; internal set; }
        public int Piece { get; internal set; }

        public List<Transition> Transitions { get; internal set; }

        public bool IsCheck { get; set; }
        public bool IsMate { get; set; }
        public bool IsPawnMove { get; set; }
        public bool IsCastling { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsCapture { get; set; }
        public bool IsEnPassantCapture { get; set; }

        public bool IsFileDisambig { get; set; }
        public bool IsRankDisambig { get; set; }

        public string FileSan { get; set; }
        public string RankSan { get; set; }
        public string FullSan { get; set; }
        public string CorrectSan { get; set; }

    }
}
