using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class Transition {
        public TransitionType Type { get; set; }
        public int FirstSquare { get; set; }
        public int SecondSquare { get; set; }
        public Piece Piece { get; set; }
        public int File { get; set; }

        public Castling OldCastling { get; set; }
        public Castling NewCastling { get; set; }

        public Transition() {
        }

        public Transition(int square, Piece piece, TransitionType type) {
            this.FirstSquare = square;
            this.Piece = piece;
            this.Type = type;
        }

        /// <summary>
        /// Generates a move transition
        /// </summary>
        /// <param name="type"></param>
        /// <param name="firstSquare"></param>
        /// <param name="secondSquare"></param>
        public Transition(TransitionType type, int firstSquare, int secondSquare) {
            if (type == TransitionType.EnPassant) {
                throw new Exception("do not use this");
            }
            this.Type = type;
            this.FirstSquare = firstSquare;
            this.SecondSquare = secondSquare;
        }

        [Obsolete]
        public Transition(TransitionType type, Castling oldCastling, Castling newCastling) {
            this.Type = TransitionType.Castling;
            this.OldCastling = oldCastling;
            this.NewCastling = newCastling;
        }

        public override int GetHashCode() {
            return this.FirstSquare << 4 | this.SecondSquare;
        }

        public override bool Equals(object obj) {
            var other = obj as Transition;
            if (other == null) {
                return false;
            }
            return (
                other.Type == this.Type
                && other.FirstSquare == this.FirstSquare
                && other.SecondSquare == this.SecondSquare
            );
        }

        internal static Transition Castling(Castling oldCastling, Castling newCastling) {
            return new Transition {
                Type = TransitionType.Castling,
                OldCastling = oldCastling,
                NewCastling = newCastling,
            };
        }

        internal static Transition PawnPromotion(int square, Piece promoPiece) {
            return new Transition {
                Type = TransitionType.PawnPromo,
                FirstSquare = square,
                Piece = (Piece)promoPiece
            };
        }

        internal static Transition Remove(int square, Piece piece) {
            return new Transition {
                Type = TransitionType.Remove,
                FirstSquare = square,
                Piece = piece
            };
        }

        internal static Transition EnPassant(int file) {
            return new Transition {
                Type = TransitionType.EnPassant,
                File = file
            };
        }

    }
}
