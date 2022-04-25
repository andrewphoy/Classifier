//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Dragon.Models;

//namespace Dragon.Chess {
//    public static class PositionExtensions {
//        /// <summary>
//        /// Return the encoded position without modifying the argument
//        /// </summary>
//        /// <param name="position"></param>
//        /// <returns></returns>
//        public static byte[] GetHuffmanPosition(this Position position) {
//            return ChessPositionEncoder.GetByteArrayForPosition(position);
//        }

//        public static void LoadFromHuffman(this Position position, byte[] bytes) {
//            ChessPositionEncoder.LoadPositionForByteArray(position, bytes);
//        }

//        public static string GetFen(this Position position) {
//            string fen = "";
//            int idx;
//            sbyte piece;
//            string pieceRep;
//            int empties = 0;
//            for (int row = 7; row >= 0; row--) {
//                for (int col = 0; col < 8; col++) {
//                    idx = row * 16 + col;
//                    piece = position.Squares[idx];
//                    switch (piece) {
//                        case (sbyte)Piece.Empty:
//                            pieceRep = "";
//                            empties++;
//                            break;
//                        case (sbyte)Piece.WhitePawn:
//                            pieceRep = "P";
//                            break;
//                        case (sbyte)Piece.WhiteRook:
//                            pieceRep = "R";
//                            break;
//                        case (sbyte)Piece.WhiteKnight:
//                            pieceRep = "N";
//                            break;
//                        case (sbyte)Piece.WhiteBishop:
//                            pieceRep = "B";
//                            break;
//                        case (sbyte)Piece.WhiteQueen:
//                            pieceRep = "Q";
//                            break;
//                        case (sbyte)Piece.WhiteKing:
//                            pieceRep = "K";
//                            break;
//                        case (sbyte)Piece.BlackPawn:
//                            pieceRep = "p";
//                            break;
//                        case (sbyte)Piece.BlackRook:
//                            pieceRep = "r";
//                            break;
//                        case (sbyte)Piece.BlackKnight:
//                            pieceRep = "n";
//                            break;
//                        case (sbyte)Piece.BlackBishop:
//                            pieceRep = "b";
//                            break;
//                        case (sbyte)Piece.BlackQueen:
//                            pieceRep = "q";
//                            break;
//                        case (sbyte)Piece.BlackKing:
//                            pieceRep = "k";
//                            break;
//                        default:
//                            throw new ArgumentException("Unknown piece");
//                    }

//                    // emit the piece
//                    if (pieceRep != "") {
//                        if (empties > 0) {
//                            fen += empties.ToString();
//                            empties = 0;
//                        }
//                        fen += pieceRep;
//                    }
//                }
//                // end of a row
//                if (empties > 0) {
//                    fen += empties.ToString();
//                    empties = 0;
//                }
//                if (row != 0) {
//                    fen += "/";
//                }
//            }

//            return fen;
//        }

//        /// <summary>
//        /// Store the encoded position in position.Encoded
//        /// Also returns the encoded position
//        /// </summary>
//        /// <param name="position"></param>
//        /// <returns></returns>
//        public static byte[] CalculateEncoded(this Position position) {
//            position.Encoded = position.Encoded ?? ChessPositionEncoder.GetByteArrayForPosition(position);
//            return position.Encoded;
//        }
//    }

//    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]> {

//        public bool Equals(byte[] x, byte[] y) {
//            return x.SequenceEqual(y);
//        }

//        public int GetHashCode(byte[] obj) {
//            if (obj.Length < 4) {
//                byte[] longer = new byte[4];
//                Array.Copy(obj, longer, obj.Length);
//                return BitConverter.ToInt32(longer, 0);
//            }
//            return BitConverter.ToInt32(obj, 0);
//        }
//    }

//}