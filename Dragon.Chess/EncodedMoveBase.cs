using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dragon.Chess {
    public abstract class EncodedMoveBase {
        [JsonIgnore]
        public short UciEncoded { get; set; }
        [JsonIgnore]
        public bool IsCheck { get; set; }
        [JsonIgnore]
        public bool IsMate { get; set; }
        [JsonIgnore]
        public bool IsVariation { get; set; }
        [JsonIgnore]
        public bool IsCapture { get; set; }
        [JsonIgnore]
        public bool IsPromotion { get; set; }
        [JsonIgnore]
        public bool IsFileDisambig { get; set; }
        [JsonIgnore]
        public bool IsRankDisambig { get; set; }
        [JsonIgnore]
        public bool IsPawnMove { get; set; }
        [JsonIgnore]
        public bool IsCastling { get; set; }

        public string GetUci() {
            short enc = this.UciEncoded;
            int start = enc & 63;
            int end = (enc & 4032) >> 6;

            string uci = "abcdefgh"[start % 8] + (start / 8 + 1).ToString() + "abcdefgh"[end % 8] + (end / 8 + 1).ToString();

            if ((enc & 32768) > 0) {
                int pieceVal = (enc & 28672) >> 12;
                Piece promo;

                if (end < 8) {
                    // black promotion
                    switch (pieceVal) {
                        case 1:
                            promo = Piece.BlackQueen;
                            break;
                        case 2:
                            promo = Piece.BlackRook;
                            break;
                        case 3:
                            promo = Piece.BlackBishop;
                            break;
                        case 5:
                            promo = Piece.BlackKing;
                            break;
                        case 6:
                            promo = Piece.BlackKnight;
                            break;
                        case 7:
                            promo = Piece.BlackPawn;
                            break;
                        default:
                            throw new ArgumentException("Unknown promotion piece");
                    }
                } else {
                    promo = (Piece)pieceVal;
                }

                return uci + promo.GetSanPiece().ToLower();
            }

            return uci;
        }

        public string GetSan() {
            string san = GetSanNoSuffix();

            if (IsMate) {
                return san + "#";
            }
            if (IsCheck) {
                return san + "+";
            }

            return san;
        }

        public string GetSanNoSuffix() {
            if (UciEncoded == 0) {
                return "";
            }

            // 0-63 mailbox values
            short enc = this.UciEncoded;
            int start = enc & 63;
            int end = (enc & 4032) >> 6;

            Piece piece = (Piece)((enc & 28672) >> 12);

            string endSquare = "abcdefgh"[end % 8] + (end / 8 + 1).ToString();

            if ((enc & 32768) > 0) {
                // pawn promotion, never a need to disambiguate
                if (IsCapture) {
                    return "abcdefgh"[start % 8] + "x" + endSquare + "=" + piece.GetSanPiece();
                } else {
                    return endSquare + "=" + piece.GetSanPiece();
                }
            }

            if (IsCastling) {
                if (end > start) {
                    return "O-O";
                } else {
                    return "O-O-O";
                }
            }

            StringBuilder sb = new StringBuilder();

            if (piece != Piece.WhitePawn && piece != Piece.BlackPawn) {
                sb.Append(piece.GetSanPiece());
            }

            if (IsFileDisambig || (IsCapture && (piece == Piece.WhitePawn || piece == Piece.BlackPawn))) {
                sb.Append("abcdefgh"[start % 8]);
            }

            if (IsRankDisambig) {
                sb.Append(start / 8 + 1);
            }

            if (IsCapture) {
                sb.Append("x");
            }

            sb.Append(endSquare);

            return sb.ToString();
        }
    }
}
