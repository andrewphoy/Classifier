using System;
using System.Collections.Generic;
using System.Text;
using Classifier.Models;
using Dragon.Chess;
using MachineLearning;

namespace Classifier.Features {
    public static class KingFeatures {

        private static int[] CornerSquares = new int[] { 0, 1, 16, 17, 6, 7, 22, 23, 96, 97, 112, 113, 102, 103, 118, 119 };

        [Feature]
        public static bool IsStrongKingInCorner(LichessPuzzle puzzle) {
            Piece king = puzzle.InitialPosition.WhiteToMove ? Piece.WhiteKing : Piece.BlackKing;
            return CornerContainsPiece(puzzle.InitialPosition, king);
        }

        [Feature]
        public static bool IsWeakKingInCorner(LichessPuzzle puzzle) {
            Piece king = puzzle.InitialPosition.WhiteToMove ? Piece.BlackKing : Piece.WhiteKing;
            return CornerContainsPiece(puzzle.InitialPosition, king);
        }

        private static bool CornerContainsPiece(Position p, Piece piece) {
            foreach (int sq in CornerSquares) {
                if (p.Squares[sq] == piece) {
                    return true;
                }
            }
            return false;
        }



    }
}
