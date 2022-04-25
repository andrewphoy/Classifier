using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dragon.Chess {
    public class ChessPositionEncoder {


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
        public static byte[] GetByteArrayForPosition(Piece[] squares) {
            // the most bits we can have is 176 (1 * 32 + 3 * 16 + 5 * 12 + 6 * 6)
            BitArray bits = new BitArray(176);

            // we fill from a8 -> h8, a8 -> a1
            int location = 175;
            int idx;
            Piece piece;
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

        public static byte[] GetByteArrayFor0x88Squares(sbyte[] squares) {
            // the most bits we can have is 176 (1 * 32 + 3 * 16 + 5 * 12 + 6 * 6)
            BitArray bits = new BitArray(176);

            // we fill from a8 -> h8, a8 -> a1
            int location = 175;

            int idx;
            for (int row = 7; row >= 0; row--) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 16 + col;
                    ChessPositionEncoder.SetPieceBits((Piece)squares[idx], bits, ref location);
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
            Piece piece;
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

        private static void SetPieceBits(Piece piece, BitArray bits, ref int location) {
            switch (piece) {
                case Piece.Empty:
                    bits[location] = false;
                    location -= 1;
                    break;

                case Piece.WhitePawn:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = false;
                    location -= 3;
                    break;

                case Piece.BlackPawn:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = false;
                    location -= 3;
                    break;

                case Piece.WhiteRook:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case Piece.BlackRook:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case Piece.WhiteKnight:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = true;
                    location -= 5;
                    break;

                case Piece.BlackKnight:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = false;
                    bits[location - 4] = true;
                    location -= 5;
                    break;

                case Piece.WhiteBishop:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case Piece.BlackBishop:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = false;
                    location -= 5;
                    break;

                case Piece.WhiteQueen:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = false;
                    location -= 6;
                    break;

                case Piece.BlackQueen:
                    bits[location] = true;
                    bits[location - 1] = false;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = false;
                    location -= 6;
                    break;

                case Piece.WhiteKing:
                    bits[location] = true;
                    bits[location - 1] = true;
                    bits[location - 2] = true;
                    bits[location - 3] = true;
                    bits[location - 4] = true;
                    bits[location - 5] = true;
                    location -= 6;
                    break;

                case Piece.BlackKing:
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
                position.Squares = new Piece[128];
            }

            BitArray bits = new BitArray(bytes);
            var root = GetTree();

            int idx = 112;
            int cursor = bits.Length - 1;
            while (cursor > 0 && idx >= 0) {
                Piece piece = GetPiece(bits, ref cursor, root);
                position.Squares[idx] = piece;
                idx++;

                // see if we need to move down a row
                if ((idx & 0x88) > 0) {
                    idx -= 24;
                }
            }
        }

        public static sbyte[] GetSquaresForByteArray(byte[] bytes) {
            var squares = new sbyte[128];
            BitArray bits = new BitArray(bytes);
            var root = GetTree();

            int idx = 112;
            int cursor = bits.Length - 1;
            while (cursor > 0 && idx >= 0) {
                Piece piece = GetPiece(bits, ref cursor, root);
                squares[idx] = (sbyte)piece;
                idx++;

                // see if we need to move down a row
                if ((idx & 0x88) > 0) {
                    idx -= 24;
                }
            }
            return squares;
        }

        public static Position GetPositionForByteArray(byte[] bytes) {
            var position = new Position();
            position.Squares = new Piece[128];
            LoadPositionForByteArray(position, bytes);
            return position;
        }

        private class Node {
            public Piece Value { get; set; }
            public Node LeftNode { get; set; }
            public Node RightNode { get; set; }
        }

        private static Node _tree;
        private static Node GetTree() {
            _tree = _tree ?? BuildTree();
            return _tree;
        }

        private static Piece GetPiece(BitArray bits, ref int cursor, Node node) {
            if (node.Value != Piece.Undefined) {
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
            root.Value = Piece.Undefined;

            foreach (KeyValuePair<Piece, bool[]> kvp in pieces) {
                AddPieceToTree(kvp.Key, kvp.Value, root);
            }


            return root;
        }

        private static void AddPieceToTree(Piece piece, bool[] path, Node node) {
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
                next = new Node() { Value = Piece.Undefined };
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


        public static byte[] GetEncodedFor0x88Squares(sbyte[] squares) {
            // the most bits we can have is 176 (1 * 32 + 3 * 16 + 5 * 12 + 6 * 6)
            BitArray bits = new BitArray(176);

            // we fill from a8 -> h8, a8 -> a1
            int location = 175;

            int idx;
            for (int row = 7; row >= 0; row--) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 16 + col;
                    ChessPositionEncoder.SetPieceBits((Piece)squares[idx], bits, ref location);
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

        private static byte PieceToNibble(sbyte enc) {
            Piece piece = (Piece)enc;

            switch (piece) {
                case Piece.Empty:
                    return 0;
                case Piece.WhitePawn:
                    return 1;
                case Piece.WhiteKnight:
                    return 2;
                case Piece.WhiteBishop:
                    return 5;
                case Piece.WhiteRook:
                    return 6;
                case Piece.WhiteQueen:
                    return 7;
                case Piece.WhiteKing:
                    return 3;
                case Piece.BlackPawn:
                    return 9;
                case Piece.BlackKnight:
                    return 10;
                case Piece.BlackBishop:
                    return 13;
                case Piece.BlackRook:
                    return 14;
                case Piece.BlackQueen:
                    return 15;
                case Piece.BlackKing:
                    return 11;
                default:
                    throw new ArgumentOutOfRangeException("Unknown piece");
            }
        }

        public static Piece NibbleToPiece(int nibble) {
            switch (nibble) {
                case 0:
                    return Piece.Empty;
                case 1:
                    return Piece.WhitePawn;
                case 2:
                    return Piece.WhiteKnight;
                case 5:
                    return Piece.WhiteBishop;
                case 6:
                    return Piece.WhiteRook;
                case 7:
                    return Piece.WhiteQueen;
                case 3:
                    return Piece.WhiteKing;
                case 9:
                    return Piece.BlackPawn;
                case 10:
                    return Piece.BlackKnight;
                case 13:
                    return Piece.BlackBishop;
                case 14:
                    return Piece.BlackRook;
                case 15:
                    return Piece.BlackQueen;
                case 11:
                    return Piece.BlackKing;
                default:
                    throw new ArgumentOutOfRangeException("Unknown piece");
            }
        }



        public static EncodedPosition GetEncodedForPosition<T>(IPositionBase position, T[] squares) {
            var encoded = new EncodedPosition {
                Ranks = new int[8],
                WhiteToMove = position.WhiteToMove,
                EnPassant = position.EnPassantByte,
                CastlingWhiteKing = position.CastlingWhiteKing,
                CastlingWhiteQueen = position.CastlingWhiteQueen,
                CastlingBlackKing = position.CastlingBlackKing,
                CastlingBlackQueen = position.CastlingBlackQueen,
                WhitePieceCount = 0,
                BlackPieceCount = 0,
                WhitePawnCount = 0,
                BlackPawnCount = 0
            };

            int idx;
            sbyte p;
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 16 + col;
                    p = Convert.ToSByte(squares[idx]);
                    if (p < 0) {
                        encoded.BlackPieceCount++;
                    }
                    if (p > 0) {
                        encoded.WhitePieceCount++;
                    }
                    if (p == (sbyte)Piece.WhitePawn) {
                        encoded.WhitePawnCount++;
                    }
                    if (p == (sbyte)Piece.BlackPawn) {
                        encoded.BlackPawnCount++;
                    }
                    encoded.Ranks[row] |= PieceToNibble(p);
                    if (col < 7) {
                        encoded.Ranks[row] <<= 4;
                    }
                }
            }

            encoded.RankOne = encoded.Ranks[0];
            encoded.RankTwo = encoded.Ranks[1];
            encoded.RankThree = encoded.Ranks[2];
            encoded.RankFour = encoded.Ranks[3];
            encoded.RankFive = encoded.Ranks[4];
            encoded.RankSix = encoded.Ranks[5];
            encoded.RankSeven = encoded.Ranks[6];
            encoded.RankEight = encoded.Ranks[7];

            return encoded;
        }

        /// <summary>
        /// Takes the position part of a FEN and returns the integer mask
        /// </summary>
        /// <param name="fen"></param>
        /// <param name="whitePieceCount"></param>
        /// <param name="blackPieceCount"></param>
        /// <returns></returns>
        public static int[] GetMaskForFen(string fen, out byte whitePieceCount, out byte blackPieceCount, out byte whitePawnCount, out byte blackPawnCount) {
            whitePieceCount = 0;
            blackPieceCount = 0;
            whitePawnCount = 0;
            blackPawnCount = 0;

            var parts = fen.Split('/');
            if (parts.Length != 8) {
                throw new ArgumentException("Not enough ranks in fen", nameof(fen));
            }

            int[] mask = new int[8];
            for (int i = 7; i >= 0; i--) {
                var row = parts[i];
                sbyte[] pieces = new sbyte[8];
                int ix = 0;
                foreach (char c in row) {
                    if (char.IsDigit(c)) {
                        ix += (c - '0');
                    } else {
                        ColorlessPiece cp = c.GetPiece();
                        if (char.IsLower(c)) {
                            blackPieceCount++;
                            if (cp == ColorlessPiece.Pawn) {
                                blackPawnCount++;
                            }
                            pieces[ix] = (sbyte)((byte)cp * -1);
                        } else {
                            whitePieceCount++;
                            if (cp == ColorlessPiece.Pawn) {
                                whitePawnCount++;
                            }
                            pieces[ix] = (sbyte)((byte)cp * 1);
                        }
                        ix++;
                    }
                }

                for (int j = 0; j < 8; j++) {
                    if (pieces[j] != 0) {
                        mask[7 - i] |= PieceToNibble(pieces[j]);
                    }
                    if (j < 7) {
                        mask[7 - i] <<= 4;
                    }
                }
            }

            return mask;
        }

        /// <summary>
        /// Returns two int arrays, the first is used as a mask to find squares that contain pawns
        /// The second is the exact value of the pawns to determine colors
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static (int[], int[]) GetPawnStructureMask(Position position) {
            int[] ands = new int[8];
            int[] pawns = new int[8];

            int idx;
            Piece p;
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    idx = row * 16 + col;
                    p = position.Squares[idx];
                    if (p == Piece.WhitePawn || p == Piece.BlackPawn) {
                        ands[row] |= 15;
                        pawns[row] |= PieceToNibble(Convert.ToSByte(p));
                    }
                    
                    if (col < 7) {
                        ands[row] <<= 4;
                        pawns[row] <<= 4;
                    }
                }
            }

            return (ands, pawns);
        }

        public static EncodedPosition GetEncodedForFen(string fen) {
            var parts = fen.Split(' ');
            if (parts.Length != 4 && parts.Length != 6) {
                throw new ArgumentException("Invalid FEN", nameof(fen));
            }

            var mask = GetMaskForFen(parts[0], out byte whitePieceCount, out byte blackPieceCount, out byte whitePawnCount, out byte blackPawnCount);

            var encoded = new EncodedPosition {
                Ranks = mask,
                WhiteToMove = parts[1] == "w",
                WhitePieceCount = whitePieceCount,
                BlackPieceCount = blackPieceCount,
                WhitePawnCount = whitePawnCount,
                BlackPawnCount = blackPawnCount
            };

            if (parts[3] == "-") {
                encoded.EnPassant = 0;
            } else {
                encoded.EnPassant = (byte)("abcdefgh".IndexOf(parts[3][0]) + 1);
            }

            encoded.CastlingWhiteKing = parts[2].Contains('K');
            encoded.CastlingWhiteQueen = parts[2].Contains('Q');
            encoded.CastlingBlackKing = parts[2].Contains('k');
            encoded.CastlingBlackQueen = parts[2].Contains('q');

            encoded.RankOne = encoded.Ranks[0];
            encoded.RankTwo = encoded.Ranks[1];
            encoded.RankThree = encoded.Ranks[2];
            encoded.RankFour = encoded.Ranks[3];
            encoded.RankFive = encoded.Ranks[4];
            encoded.RankSix = encoded.Ranks[5];
            encoded.RankSeven = encoded.Ranks[6];
            encoded.RankEight = encoded.Ranks[7];

            return encoded;
        }

        public Move GetUciData(short uciEncoded, bool? whiteToMove = null) {
            var move = new Move();

            // 0-63 mailbox values
            int start = uciEncoded & 63;
            int end = (uciEncoded & 4032) >> 6;

            Piece piece = Piece.Undefined;

            int pieceVal = uciEncoded & 28672 >> 12;
            if (whiteToMove.HasValue && whiteToMove.Value) {
                piece = (Piece)pieceVal;
            }
            if (whiteToMove.HasValue && !whiteToMove.Value) {
                switch (pieceVal) {
                    case 1:
                        piece = Piece.BlackQueen;
                        break;
                    case 2:
                        piece = Piece.BlackRook;
                        break;
                    case 3:
                        piece = Piece.BlackBishop;
                        break;
                    case 5:
                        piece = Piece.BlackKing;
                        break;
                    case 6:
                        piece = Piece.BlackKnight;
                        break;
                    case 7:
                        piece = Piece.BlackPawn;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Ahhhhhhh");
                }
            }

            if (piece == Piece.Undefined) {
                throw new ArgumentOutOfRangeException("Did not set piece");
            }

            if ((uciEncoded & 32768) > 0) {
                // pawn promotion

            }



            //Piece piece = uciEncoded 

            /*
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

                bits[12] = (Piece & 1) > 0;
                bits[13] = (Piece & 2) > 0;
                bits[14] = (Piece & 4) > 0;

                if (IsPromotion) {
                    bits[15] = true;
                }

                byte[] bytes = new byte[2];
                bits.CopyTo(bytes, 0);
                return BitConverter.ToInt16(bytes, 0);
            }

            */
            return move;
        }
    }
}
