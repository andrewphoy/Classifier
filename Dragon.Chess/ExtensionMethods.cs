using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dragon.Chess {
    public static class ExtensionMethods {

        public static int[] GetDeltas(this ColorlessPiece piece) {
            switch (piece) {
                case ColorlessPiece.Rook:
                    return new int[] { -16, -1, 1, 16 };
                case ColorlessPiece.Knight:
                    return new int[] { -33, -31, -18, -14, 14, 18, 31, 33 };
                case ColorlessPiece.Bishop:
                    return new int[] { -17, -15, 15, 17 };
                case ColorlessPiece.Queen:
                case ColorlessPiece.King:
                    return new int[] { -17, -15, 15, 17, -16, -1, 1, 16 };
                default:
                    return new int[] { };
            }
        }

        public static int[] GetDeltas(this sbyte piece) => GetDeltas((Piece)piece);

        public static int[] GetDeltas(this Piece piece) => GetDeltas((ColorlessPiece)Math.Abs((sbyte)piece));

        public static bool IsPawn(this sbyte piece) {
            return Math.Abs(piece) == (sbyte)ColorlessPiece.Pawn;
        }

        public static bool IsKing(this sbyte piece) {
            return Math.Abs(piece) == (sbyte)ColorlessPiece.King;
        }

        public static bool IsKing(this Piece piece) {
            return Math.Abs((sbyte)piece) == (sbyte)ColorlessPiece.King;
        }

        public static bool IsSliding(this Piece piece) {
            return ((sbyte)piece).IsSliding();
        }

        public static bool IsSliding(this sbyte piece) {
            return (Math.Abs(piece) & 4) > 0;
        }

        public static bool IsSliding(this ColorlessPiece piece) {
            return (((sbyte)piece) & 4) > 0;
        }

        public static string FenPiece(this sbyte piece) {
            string result;

            switch ((ColorlessPiece)Math.Abs((sbyte)piece)) {
                case ColorlessPiece.Pawn:
                    result = "p";
                    break;
                case ColorlessPiece.Rook:
                    result = "r";
                    break;
                case ColorlessPiece.Knight:
                    result = "n";
                    break;
                case ColorlessPiece.Bishop:
                    result = "b";
                    break;
                case ColorlessPiece.Queen:
                    result = "q";
                    break;
                case ColorlessPiece.King:
                    result = "k";
                    break;
                default:
                    return "";
            }

            if (piece > 0) {
                result = result.ToUpper();
            }

            return result;
        }

        public static string Tag(this sbyte piece) {
            string tag = (piece < 0) ? "b" : "w";

            switch ((ColorlessPiece)Math.Abs((sbyte)piece)) {
                case ColorlessPiece.Pawn:
                    return tag + "p";
                case ColorlessPiece.Rook:
                    return tag + "r";
                case ColorlessPiece.Knight:
                    return tag + "n";
                case ColorlessPiece.Bishop:
                    return tag + "b";
                case ColorlessPiece.Queen:
                    return tag + "q";
                case ColorlessPiece.King:
                    return tag + "k";
                default:
                    return "";
            }
        }

        public static string GetSanPiece(this Piece piece) {
            switch ((ColorlessPiece)Math.Abs((sbyte)piece)) {
                case ColorlessPiece.Pawn:
                    return "P";
                case ColorlessPiece.Rook:
                    return "R";
                case ColorlessPiece.Knight:
                    return "N";
                case ColorlessPiece.Bishop:
                    return "B";
                case ColorlessPiece.Queen:
                    return "Q";
                case ColorlessPiece.King:
                    return "K";
                default:
                    return "";
            }
        }

        public static string GetSanSquare(this int square) {
            if ((square & 0x88) == 0) {
                var file = "abcdefgh".ToCharArray()[square % 16];
                var rank = square / 16;
                return file + (rank + 1).ToString();
            } else {
                return square.ToString();
            }
        }

        public static ColorlessPiece GetPiece(this string piece) {
            switch (piece.ToLower()) {
                case "p":
                    return ColorlessPiece.Pawn;
                case "r":
                    return ColorlessPiece.Rook;
                case "n":
                    return ColorlessPiece.Knight;
                case "b":
                    return ColorlessPiece.Bishop;
                case "q":
                    return ColorlessPiece.Queen;
                case "k":
                    return ColorlessPiece.King;
                default:
                    return ColorlessPiece.Undefined;
            }
        }

        public static ColorlessPiece GetPiece(this char piece) {
            switch (piece) {
                case 'P':
                    return ColorlessPiece.Pawn;
                case 'R':
                    return ColorlessPiece.Rook;
                case 'N':
                    return ColorlessPiece.Knight;
                case 'B':
                    return ColorlessPiece.Bishop;
                case 'Q':
                    return ColorlessPiece.Queen;
                case 'K':
                    return ColorlessPiece.King;
                case 'p':
                    return ColorlessPiece.Pawn;
                case 'r':
                    return ColorlessPiece.Rook;
                case 'n':
                    return ColorlessPiece.Knight;
                case 'b':
                    return ColorlessPiece.Bishop;
                case 'q':
                    return ColorlessPiece.Queen;
                case 'k':
                    return ColorlessPiece.King;
                default:
                    return ColorlessPiece.Undefined;
            }
        }
        #region Pgn Header Extensions

        public static T TryGetHeader<T>(this PgnGame game, string header, T defaultVal = default(T)) {
            if (string.IsNullOrWhiteSpace(header)) {
                return defaultVal;
            }

            var key = header.ToLowerInvariant();
            if (!game.Headers.ContainsKey(key)) {
                return defaultVal;
            }

            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                type = Nullable.GetUnderlyingType(type);
            }

            string stringVal = game.Headers[key].Trim();

            if (type == typeof(string)) {
                return (T)Convert.ChangeType(stringVal, type);
            }

            if (type == typeof(Int32) || type == typeof(Int16) || type == typeof(Int64) || type == typeof(byte)) {
                if (int.TryParse(stringVal, out int intVal)) {
                    return (T)Convert.ChangeType(intVal, type);
                }
            }

            if (type == typeof(decimal) || type == typeof(Single) || type == typeof(Double)) {
                if (decimal.TryParse(stringVal, out decimal decVal)) {
                    return (T)Convert.ChangeType(decVal, type);
                }
            }

            if (type == typeof(bool)) {
                if (bool.TryParse(stringVal, out bool boolVal)) {
                    return (T)Convert.ChangeType(boolVal, type);
                }
            }

            return defaultVal;
        }
        #endregion

        public static IEnumerable<string> ReadLinesSafe(this TextReader reader) {
            StringBuilder sb = new StringBuilder();
            int ch;

            while ((ch = reader.Read()) >= 0) {
                if (ch == '\r') {
                    continue;
                }

                if (ch == '\n') {
                    yield return sb.ToString();
                    sb.Length = 0;
                } else {
                    sb.Append((char)ch);
                }
            }

            if (sb.Length > 0) {
                yield return sb.ToString();
            }
        }
        
        public static Position PlayUciMove(this Position p, string uciMove) {
            var legals = MoveGenerator.LegalMoves(p, p.WhiteToMove);
            var move = legals.Single(l => l.Uci == uciMove);
            return p.PlayMove(move);
        }
    }
}
