using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dragon.Chess {
    public enum TransitionType : byte {
        Move = 3, // 11   <-- note that this is the only transition that ends with two set bits
        Add = 1, // 01
        Remove = 2, // 10
        EnPassant = 4, // 100
        Castling = 6, // 110
        PawnPromo = 0 // 00
    }

    public class ChessTransition {
        public ChessTransition() { }

        public ChessTransition(short encodedValue) {
            /*
            var t = new Transition();
            var minorType = +(encoded & 3);
            if (minorType === 3) {
                // move type
                t.type = ttype.move;
                t.secondVal = +((encoded >> 2) & 127);
                t.firstVal = +((encoded >> 9) & 127);
            } else {
                // not a move
                t.type = +(encoded & 7);
                // two values, normal case
                t.secondVal = +((encoded >> 3) & 63);
                t.firstVal = +((encoded >> 9) & 127);
            }

            // for add, remove, promo, cast to negative pieces
            if (t.type == ttype.add || t.type == ttype.remove || t.type == ttype.pawnPromo) {
                if (t.secondVal > 8) {
                    t.secondVal = t.secondVal - 16;
                }
            }
            return t;
            */

            byte minorType = (byte)(encodedValue & 3);
            if (minorType == 3) {
                // move type
                this.Type = TransitionType.Move;
                this.FirstSquare = (byte)((encodedValue >> 2) & 127);
                this.SecondSquare = (byte)((encodedValue >> 9) & 127);
                return;
            } else {
                // not a move
                this.Type = (TransitionType)(encodedValue & 7);

                byte secondVal = (byte)((encodedValue >> 3) & 63);
                byte firstVal = (byte)((encodedValue >> 9) & 127);
                this.SetValuesForType(this.Type, firstVal, secondVal);
                return;
            }
        }

        public ChessTransition(TransitionType type, byte firstVal, byte secondVal) {
            this.SetValuesForType(type, firstVal, secondVal);
        }

        public ChessTransition(TransitionType type, Castling oldCastling, Castling newCastling) {
            this.SetValuesForType(type, (byte)oldCastling, (byte)newCastling);
        }

        public ChessTransition(TransitionType type, int firstVal, int secondVal) {
            this.SetValuesForType(type, (byte)firstVal, (byte)secondVal);
        }

        public TransitionType Type { get; set; }
        public byte FirstSquare { get; set; }
        public byte SecondSquare { get; set; }
        
        public sbyte Piece { get; set; }
        public byte File { get; set; }

        public Castling OldCastling { get; set; }
        public Castling NewCastling { get; set; }

        private void SetValuesForType(TransitionType type, byte firstVal, byte secondVal) {
            this.Type = type;
            switch(type) {
                case TransitionType.Move:
                    this.FirstSquare = firstVal;
                    this.SecondSquare = secondVal;
                    break;
                case TransitionType.Add:
                case TransitionType.Remove:
                    this.FirstSquare = (byte)firstVal;
                    this.Piece = (sbyte)secondVal;
                    break;
                case TransitionType.EnPassant:
                    this.File = (byte)firstVal;
                    break;
                case TransitionType.Castling:
                    this.OldCastling = (Castling)firstVal;
                    this.NewCastling = (Castling)secondVal;
                    break;
                case TransitionType.PawnPromo:
                    this.FirstSquare = (byte)firstVal;
                    this.Piece = (sbyte)secondVal;
                    break;
            }
        }

        public ushort ToStorageFormat() {
            byte type = (byte)this.Type;
            ushort firstVal = 0;
            ushort secondVal = 0;

            switch(this.Type) {
                case TransitionType.Move:
                    firstVal = this.FirstSquare;
                    secondVal = this.SecondSquare;
                    break;
                case TransitionType.Add:
                    firstVal = this.FirstSquare;
                    if (this.Piece < 0) {
                        secondVal = (byte)(this.Piece + 16);
                    } else {
                        secondVal = (byte)this.Piece;
                    }
                    break;
                case TransitionType.Remove:
                    firstVal = this.FirstSquare;
                    if (this.Piece < 0) {
                        secondVal = (byte)(this.Piece + 16);
                    } else {
                        secondVal = (byte)this.Piece;
                    }
                    break;
                case TransitionType.EnPassant:
                    firstVal = this.File;
                    secondVal = 0;
                    break;
                case TransitionType.Castling:
                    firstVal = (byte)this.OldCastling;
                    secondVal = (byte)this.NewCastling;
                    break;
                case TransitionType.PawnPromo:
                    firstVal = this.FirstSquare;
                    if (this.Piece < 0) {
                        secondVal = (byte)(this.Piece + 16);
                    } else {
                        secondVal = (byte)this.Piece;
                    }
                    break;
            }

            if (this.Type == TransitionType.Move) {
                // result is 7-7-2
                ushort result = 3;
                result = (ushort)(result | (firstVal << 9));
                result = (ushort)(result | (secondVal << 2));
                return result;
            } else {
                // not a move
                ushort result = type;
                result = (ushort)(result | (firstVal << 9));
                result = (ushort)(result | (secondVal << 3));
                return result;
            }
        }
    }
}
