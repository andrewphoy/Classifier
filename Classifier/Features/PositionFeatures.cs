using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classifier.Helpers;
using Classifier.Models;
using Dragon.Chess;
using MachineLearning;

namespace Classifier.Features {
    public static class PositionFeatures {

        [Feature]
        public static ColorlessPiece KeyPiece(LichessPuzzle puzzle) => (ColorlessPiece)Math.Abs(puzzle.KeyMove.Piece);

        [Feature]
        public static bool IsStrongSideInCheck(LichessPuzzle puzzle) =>
            MoveGenerator.IsKingAttacked(puzzle.InitialPosition, puzzle.InitialPosition.WhiteToMove);

        [Feature]
        public static bool IsKeyMoveACheck(LichessPuzzle puzzle) => puzzle.KeyMove.IsCheck;

        [Feature]
        public static bool IsMateInOne(LichessPuzzle puzzle) => puzzle.KeyMove.IsMate;

        [Feature]
        public static bool IsKeyMoveACapture(LichessPuzzle puzzle) => puzzle.KeyMove.IsCapture;
    }
}
