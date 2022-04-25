//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Dragon.Chess {
//    public static class PieceExtensions {

//        public static ColorlessPiece GetPiece(this string piece) {
//            switch (piece.ToLower()) {
//                case "p":
//                    return ColorlessPiece.Pawn;
//                case "r":
//                    return ColorlessPiece.Rook;
//                case "n":
//                    return ColorlessPiece.Knight;
//                case "b":
//                    return ColorlessPiece.Bishop;
//                case "q":
//                    return ColorlessPiece.Queen;
//                case "k":
//                    return ColorlessPiece.King;
//                default:
//                    return ColorlessPiece.Undefined;
//            }
//        }

//        public static string GetSanPiece(this sbyte piece) {
//            if (piece < 0) {
//                piece = (sbyte)(piece * -1);
//            }
//            switch ((ColorlessPiece)piece) {
//                case ColorlessPiece.Pawn:
//                    return "P";
//                case ColorlessPiece.Rook:
//                    return "R";
//                case ColorlessPiece.Knight:
//                    return "N";
//                case ColorlessPiece.Bishop:
//                    return "B";
//                case ColorlessPiece.Queen:
//                    return "Q";
//                case ColorlessPiece.King:
//                    return "K";
//                default:
//                    return "";
//            }
//        }

//        public static int[] GetDeltas(this ColorlessPiece piece) {
//            switch (piece) {
//                case ColorlessPiece.Rook:
//                    return new int[] { -16, -1, 1, 16 };
//                case ColorlessPiece.Knight:
//                    return new int[] { -33, -31, -18, -14, 14, 18, 31, 33 };
//                case ColorlessPiece.Bishop:
//                    return new int[] { -17, -15, 15, 17 };
//                case ColorlessPiece.Queen:
//                case ColorlessPiece.King:
//                    return new int[] { -17, -15, 15, 17, -16, -1, 1, 16 };
//                default:
//                    return new int[] { };
//            }
//        }

//        public static bool IsSliding(this ColorlessPiece piece) {
//            return ((int)piece).IsSliding();
//        }

//        public static bool IsSliding(this int piece) {
//            return (Math.Abs(piece) & 4) > 0;
//        }
//    }
//}
