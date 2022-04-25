using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragon.Chess {
    public class Position : IPositionBase {

        public long Id { get; set; }
        public byte[] Encoded { get; set; }
        public int ColorToMove { get; set; }

        public Castling WhiteCastling { get; set; }
        public Castling BlackCastling { get; set; }

        public int? EnPassant { get; set; }

        // values for db access
        public bool WhiteToMove => ColorToMove == 1;
        public bool CastlingWhiteKing => (WhiteCastling == Castling.Kingside || WhiteCastling == Castling.Both);
        public bool CastlingWhiteQueen => (WhiteCastling == Castling.Queenside || WhiteCastling == Castling.Both);
        public bool CastlingBlackKing => (BlackCastling == Castling.Kingside || BlackCastling == Castling.Both);
        public bool CastlingBlackQueen => (BlackCastling == Castling.Queenside || BlackCastling == Castling.Both);

        public byte EnPassantByte {
            get {
                if (!EnPassant.HasValue) {
                    return 0;
                } else {
                    return (byte)(EnPassant.Value + 1);
                }
            }
        }


        // the following is only used in code
        public Piece[] Squares { get; set; }

        private string _fen = null;
        public string Fen { 
            get {
                if (_fen == null) {
                    CreateFen();
                }
                return _fen;
            }
        }

        private string _shortFen = null;
        public string ShortFen {
            get {
                if (_shortFen == null) {
                    CreateFen();
                }
                return _shortFen;
            }
        }


        public static Position StartPosition {
            get {
                return Position.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            }
        }

        private void CreateFen() {
            int ix = 112;
            StringBuilder sb = new StringBuilder();

            while (ix >= 0) {
                int empties = 0;
                for (int col = 0; col < 8; col++) {
                    if (Squares[ix + col] != 0) {
                        // is a piece
                        if (empties > 0) {
                            sb.Append(empties.ToString());
                            empties = 0;
                        }
                        string piece = Squares[ix + col].GetSanPiece();
                        if (Squares[ix + col] < 0) {
                            sb.Append(piece.ToLower());
                        } else {
                            sb.Append(piece);
                        }

                    } else {
                        empties++;
                    }
                }

                if (empties > 0) {
                    sb.Append(empties.ToString());
                }
                sb.Append('/');

                ix -= 16;
            }

            // remove last slash
            sb.Length--;

            sb.Append(' ');
            sb.Append(ColorToMove == 1 ? 'w' : 'b');
            sb.Append(' ');

            if (WhiteCastling == Castling.None && BlackCastling == Castling.None) {
                sb.Append('-');
            }
            if (WhiteCastling == Castling.Kingside || WhiteCastling == Castling.Both) {
                sb.Append('K');
            }
            if (WhiteCastling == Castling.Queenside || WhiteCastling == Castling.Both) {
                sb.Append('Q');
            }
            if (BlackCastling == Castling.Kingside || BlackCastling == Castling.Both) {
                sb.Append('k');
            }
            if (BlackCastling == Castling.Queenside || BlackCastling == Castling.Both) {
                sb.Append('q');
            }

            sb.Append(' ');

            if (this.EnPassant.HasValue) {
                sb.Append("abcdefgh"[this.EnPassant.Value]);
                sb.Append(ColorToMove == 1 ? '6' : '3');
            } else {
                sb.Append('-');
            }

            _shortFen = sb.ToString();

            //TODO ply and half move clock
            //sb.Append(' ');

            _fen = sb.ToString();

            
        }

        public static Position FromFen(string fen) {
            var parts = fen.Split(' ');
            if (parts.Length == 6 || parts.Length == 4) {
                // full fen
                var rows = parts[0].Split('/');
                if (rows.Length != 8) {
                    throw new FormatException("Invalid FEN");
                }

                var p = new Position() {
                    Squares = new Piece[128]
                };

                int offset = 112;
                int ixChar = 0;
                for (var col = 0; col < 8; col++) {
                    ixChar = 0;
                    for (var i = 0; i < 8; i++) {
                        if (rows[col].Length > ixChar) {
                            char charNext = rows[col][ixChar];
                            if (char.IsLetter(charNext)) {
                                // is a piece

                                // -1 is black, 1 is white
                                var color = (charNext.ToString().ToLower() == charNext.ToString()) ? -1 : 1;

                                var piece = GetPiece(charNext.ToString().ToLower());
                                p.Squares[offset + i] = (Piece)(color * piece);

                                //TODO speed up with king positions
                            } else {
                                // is an empty square
                                i += (int.Parse(charNext.ToString()) - 1);
                            }
                        }
                        ixChar++;
                    }
                    offset -= 16;
                }

                if (parts[1] == "w") {
                    p.ColorToMove = 1;
                } else if (parts[1] == "b") {
                    p.ColorToMove = -1;
                } else {
                    throw new ArgumentException("Invalid color to move");
                }

                // parts[2] == castling
                p.WhiteCastling = 0;
                p.BlackCastling = 0;
                foreach (char c in parts[2]) {
                    if (c == 'K') {
                        p.WhiteCastling |= Castling.Kingside;
                    } else if (c == 'Q') {
                        p.WhiteCastling |= Castling.Queenside;
                    } else if (c == 'k') {
                        p.BlackCastling |= Castling.Kingside;
                    } else if (c == 'q') {
                        p.BlackCastling |= Castling.Queenside;
                    }
                }

                // parts[3] == enPassant
                if (parts[3] != "-") {
                    p.EnPassant = "abcdefgh".IndexOf(parts[3][0]);
                }

                return p;
            } else {
                throw new ArgumentOutOfRangeException("Only full FEN's supported at this time");
            }
        }

        private static int GetPiece(string s) {
            switch (s) {
                case "p":
                    return (int)ColorlessPiece.Pawn;
                case "n":
                    return (int)ColorlessPiece.Knight;
                case "b":
                    return (int)ColorlessPiece.Bishop;
                case "r":
                    return (int)ColorlessPiece.Rook;
                case "q":
                    return (int)ColorlessPiece.Queen;
                case "k":
                    return (int)ColorlessPiece.King;
                default:
                    throw new ArgumentException("Invalid FEN piece");
            }
        }

        public Position PlayMove(Move move) {
            return MoveGenerator.CreateNextPosition(this, move);
        }

    }
}
