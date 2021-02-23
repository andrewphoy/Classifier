using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Dragon.Models;

namespace Dragon.Chess {
    public static class ChessPositionEncoder {

        // Huffman codes
        // c = placeholder for color (1 = white, 0 = black)
        // empty    0
        // pawn     1c0
        // rook     1c100
        // knight   1c101
        // bishop   1c110
        // queen    1c1110
        // king     1c1111

        #region Encoding
        /// <summary>
        /// Encodes a 64 bit mailbox array into a single byte string
        /// </summary>
        /// <param name="squares"></param>
        /// <returns></returns>
        public static byte[] GetByteArrayForPosition(sbyte[] squares) {
            // the most bits we can have is 176 (1 * 32 + 3 * 16 + 5 * 12 + 6 * 6)
            BitArray bits = new BitArray(176);

            // we fill from a8 -> h8, a8 -> a1
            int location = 175;
            int idx;
            sbyte piece;
            for (int row = 7; row >= 0; row--) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 8 + col;
                    piece = squares[idx];
                    ChessPositionEncoder.SetPieceBits(piece, bits, ref location);
                }
            }

            byte[] bytes = new byte[22];
            bits.CopyTo(bytes, 0);

            // we might be able to trim our byte array
            if (location > 7) {
                // for every 8 bits, we can trim one byte
                int toTrim = location / 8;
                byte[] trimmedBytes = new byte[22 - toTrim];
                for (int i = toTrim; i < 22; i++) {
                    trimmedBytes[i - toTrim] = bytes[i];
                }
                return trimmedBytes;
            }

            return bytes;
        }

        /// <summary>
        /// Encodes a 0x88 array into a single byte string
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static byte[] GetByteArrayForPosition(Position position) {
            // the most bits we can have is 176 (1 * 32 + 3 * 16 + 5 * 12 + 6 * 6)
            BitArray bits = new BitArray(176);

            // we fill from a8 -> h8, a8 -> a1
            int location = 175;

            int idx;
            sbyte piece;
            for (int row = 7; row >= 0; row--) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 16 + col;
                    piece = position.Squares[idx];
                    ChessPositionEncoder.SetPieceBits(piece, bits, ref location);
                }
            }

            byte[] bytes = new byte[22];
            bits.CopyTo(bytes, 0);

            // we might be able to trim our byte array
            if (location > 7) {
                // for every 8 bits, we can trim one byte
                int toTrim = location / 8;
                byte[] trimmedBytes = new byte[22 - toTrim];
                for (int i = toTrim; i < 22; i++) {
                    trimmedBytes[i - toTrim] = bytes[i];
                }
                return trimmedBytes;
            }

            return bytes;
        }

        private static void SetPieceBits (sbyte piece, BitArray bits, ref int location) {
            switch (piece) {
                case (sbyte)Piece.Empty:
                    bits[location] = false;
                    location -= 1;
                    break;

                case (sbyte)Piece.WhitePawn:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = false;
                    location -= 3;
                    break;

                case (sbyte)Piece.BlackPawn:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = false;
                    location -= 3;
                    break;

                case (sbyte)Piece.WhiteRook:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case (sbyte)Piece.BlackRook:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case (sbyte)Piece.WhiteKnight:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = true;
                    location -= 5;
                    break;

                case (sbyte)Piece.BlackKnight:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = true;
                    location -= 5;
                    break;

                case (sbyte)Piece.WhiteBishop:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case (sbyte)Piece.BlackBishop:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case (sbyte)Piece.WhiteQueen:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = false;
                    location -= 6;
                    break;

                case (sbyte)Piece.BlackQueen:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = false;
                    location -= 6;
                    break;

                case (sbyte)Piece.WhiteKing:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = true;
                    location -= 6;
                    break;

                case (sbyte)Piece.BlackKing:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = true;
                    location -= 6;
                    break;

                default:
                    throw new ArgumentException("Unknown piece");
            }
        }
        #endregion

        #region Decoding
        public static void LoadPositionForByteArray(Position position, byte[] bytes) {
            if (position.Squares == null) {
                position.Squares = new sbyte[128];
            }

            BitArray bits = new BitArray(bytes);
            var root = GetTree();

            int idx = 112;
            int cursor = bits.Length - 1;
            while (cursor > 0 && idx >= 0) {
                sbyte piece = GetPiece(bits, ref cursor, root);
                position.Squares[idx] = piece;
                idx++;

                // see if we need to move down a row
                if ((idx & 0x88) > 0) {
                    idx -= 24;
                }
            }
        }

        public static Position GetPositionForByteArray(byte[] bytes) {
            var position = new Position();
            position.Squares = new sbyte[128];
            LoadPositionForByteArray(position, bytes);
            return position;
        }

        private class Node {
            public sbyte Value { get; set; }
            public Node LeftNode { get; set; }
            public Node RightNode { get; set; }
        }

        private static Node _tree;
        private static Node GetTree() {
            _tree = _tree ?? BuildTree();
            return _tree;
        }

        private static sbyte GetPiece(BitArray bits, ref int cursor, Node node) {
            if (node.Value != (sbyte)Piece.Undefined) {
                return node.Value;
            }

            // get value and decrement the cursor
            bool nextBit = bits[cursor];
            cursor--;

            if (nextBit) {
                return GetPiece(bits, ref cursor, node.LeftNode);
            } else {
                return GetPiece(bits, ref cursor, node.RightNode);
            }
        }

        #region Building Tree
        /// <summary>
        /// Build the tree that will be used to decode bytes
        /// </summary>
        /// <remarks>Not terribly efficient, but only runs once so it doesn't have to be</remarks>
        /// <returns></returns>
        private static Node BuildTree() {
            var pieces = new Dictionary<Piece, bool[]>();
            pieces.Add(Piece.Empty, new bool[] { false });

            pieces.Add(Piece.WhitePawn, new bool[] { true, true, false });
            pieces.Add(Piece.WhiteRook, new bool[] { true, true, true, false, false });
            pieces.Add(Piece.WhiteKnight, new bool[] { true, true, true, false, true });
            pieces.Add(Piece.WhiteBishop, new bool[] { true, true, true, true, false });
            pieces.Add(Piece.WhiteQueen, new bool[] { true, true, true, true, true, false });
            pieces.Add(Piece.WhiteKing, new bool[] { true, true, true, true, true, true });

            pieces.Add(Piece.BlackPawn, new bool[] { true, false, false });
            pieces.Add(Piece.BlackRook, new bool[] { true, false, true, false, false });
            pieces.Add(Piece.BlackKnight, new bool[] { true, false, true, false, true });
            pieces.Add(Piece.BlackBishop, new bool[] { true, false, true, true, false });
            pieces.Add(Piece.BlackQueen, new bool[] { true, false, true, true, true, false });
            pieces.Add(Piece.BlackKing, new bool[] { true, false, true, true, true, true });

            var root = new Node();
            root.Value = (sbyte)Piece.Undefined;

            foreach (KeyValuePair<Piece, bool[]> kvp in pieces) {
                AddPieceToTree((sbyte)kvp.Key, kvp.Value, root);
            }


            return root;
        }

        private static void AddPieceToTree(sbyte piece, bool[] path, Node node) {
            if (path.Length == 1) {
                if (path[0]) {
                    // left node
                    node.LeftNode = new Node() { Value = piece };
                } else {
                    // right node
                    node.RightNode = new Node() { Value = piece };
                }
                return;
            }

            // are we going left or right?
            bool direction = path[0];
            Node next;
            if (direction) {
                next = node.LeftNode;
            } else {
                next = node.RightNode;
            }

            if (next == null) {
                next = new Node() { Value = (sbyte)Piece.Undefined };
                if (direction) {
                    node.LeftNode = next;
                } else {
                    node.RightNode = next;
                }
            }

            AddPieceToTree(piece, path.Skip(1).ToArray(), next);
        }
        #endregion
        #endregion
    }
}