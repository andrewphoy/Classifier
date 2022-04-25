using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineLearning;
using Dragon.Chess;
using Classifier.Models;

namespace Classifier.Helpers {
    public static class PositionDescriber {

        [InitializeFeatures]
        public static void DescribeLichessPuzzle(LichessPuzzle puzzle) {
            if (puzzle.Initialized) { return; }

            int cntMoves = puzzle.UciMoves.Count;
            puzzle.Positions = new Position[cntMoves + 1];
            puzzle.Moves = new Move[cntMoves];

            Position position = Position.FromFen(puzzle.Fen);
            Move move;

            for (int i = 0; i < cntMoves; i++) {
                puzzle.Positions[i] = position;
                var legals = MoveGenerator.LegalMoves(position, position.WhiteToMove);
                move = legals.Single(l => l.Uci == puzzle.UciMoves[i]);
                MoveGenerator.DecorateMove(move);
                puzzle.Moves[i] = move;

                position = position.PlayMove(move);
            }

            puzzle.Positions[cntMoves] = position;


            //puzzle.PreviousPosition = Dragon.Chess.Position.FromFen(puzzle.Fen);

            //var previousLegals = MoveGenerator.LegalMoves(puzzle.PreviousPosition, puzzle.PreviousPosition.WhiteToMove);
            //puzzle.PreviousMove = previousLegals.Single(l => l.Uci == puzzle.UciMoves[0]);
            //puzzle.InitialPosition = puzzle.PreviousPosition.PlayMove(puzzle.PreviousMove);

            //var legals = MoveGenerator.LegalMoves(puzzle.InitialPosition, puzzle.InitialPosition.WhiteToMove);
            //puzzle.KeyMove = legals.Single(l => l.Uci == puzzle.UciMoves[1]);
            //MoveGenerator.DecorateMove(puzzle.KeyMove);

            
            int cntStrong = 0;
            int cntWeak = 0;
            int cntStrongPawns = 0;
            int cntWeakPawns = 0;

            for (int i = 0; i < 120; i++) {
                if (puzzle.InitialPosition.WhiteToMove) {
                    if (puzzle.InitialPosition.Squares[i] < 0) {
                        if (puzzle.InitialPosition.Squares[i] == Piece.BlackPawn) {
                            cntWeakPawns++;
                        }
                        cntWeak++;
                    } else if (puzzle.InitialPosition.Squares[i] > 0) {
                        if (puzzle.InitialPosition.Squares[i] == Piece.WhitePawn) {
                            cntStrongPawns++;
                        }
                        cntStrong++;
                    }
                } else { 
                    if (puzzle.InitialPosition.Squares[i] > 0) {
                        if (puzzle.InitialPosition.Squares[i] == Piece.WhitePawn) {
                            cntWeakPawns++;
                        }
                        cntWeak++;
                    } else if (puzzle.InitialPosition.Squares[i] < 0) {
                        if (puzzle.InitialPosition.Squares[i] == Piece.BlackPawn) {
                            cntStrongPawns++;
                        }
                        cntStrong++;
                    }
                }
            }

            puzzle.StrongSidePieceCount = cntStrong;
            puzzle.WeakSidePieceCount = cntWeak;
            puzzle.StrongSidePawnCount = cntStrongPawns;
            puzzle.WeakSidePawnCount = cntWeakPawns;


            puzzle.Initialized = true;
        }

    }
}
