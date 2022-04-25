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
    public static class MoveFeatures {

        [Feature]
        public static bool HasCaptureOnH7(LichessPuzzle puzzle) {
            int dest = puzzle.InitialPosition.WhiteToMove ? 103 : 23;
            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].End == dest && puzzle.Moves[i].IsCapture) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static bool HasCaptureOnF7(LichessPuzzle puzzle) {
            int dest = puzzle.InitialPosition.WhiteToMove ? 101 : 21;
            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].End == dest) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static bool HasPawnCheck(LichessPuzzle puzzle) {
            int piece = puzzle.InitialPosition.WhiteToMove ? (int)Piece.WhitePawn : (int)Piece.BlackPawn;

            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].Piece == piece && puzzle.Moves[i].IsCheck) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static bool HasKnightCheck(LichessPuzzle puzzle) {
            int piece = puzzle.InitialPosition.WhiteToMove ? (int)Piece.WhiteKnight : (int)Piece.BlackKnight;

            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].Piece == piece && puzzle.Moves[i].IsCheck) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static bool HasBishopCheck(LichessPuzzle puzzle) {
            int piece = puzzle.InitialPosition.WhiteToMove ? (int)Piece.WhiteBishop : (int)Piece.BlackBishop;

            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].Piece == piece && puzzle.Moves[i].IsCheck) {
                    return true;
                }
            }

            return false;
        }

        //[Feature]
        //public static bool HasContactCheck(LichessPuzzle puzzle) {

        //}

        [Feature]
        public static bool HasRookCheck(LichessPuzzle puzzle) {
            int piece = puzzle.InitialPosition.WhiteToMove ? (int)Piece.WhiteRook : (int)Piece.BlackRook;

            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].Piece == piece && puzzle.Moves[i].IsCheck) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static bool HasQueenCheck(LichessPuzzle puzzle) {
            int piece = puzzle.InitialPosition.WhiteToMove ? (int)Piece.WhiteQueen : (int)Piece.BlackQueen;

            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].Piece == piece && puzzle.Moves[i].IsCheck) {
                    return true;
                }
            }

            return false;
        }

        [Feature]
        public static int NumberOfStrongSideChecks(LichessPuzzle puzzle) {
            int numChecks = 0;
            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (puzzle.Moves[i].IsCheck) {
                    numChecks++;
                }
            }
            return numChecks;
        }

        [Feature]
        public static bool EndsWithMate(LichessPuzzle puzzle) {
            return puzzle.Moves.Last().IsMate;
        }

        [Feature]
        public static bool HasDoubleCheck(LichessPuzzle puzzle) {
            return puzzle.Positions.Any(p => IsDoubleCheck(p));
        }

        private static bool IsDoubleCheck(Position p) {
            return MoveGenerator.GetKingAttackers(p, p.WhiteToMove).Count() > 1;
        }

        [Feature]
        public static bool HasKingCapture(LichessPuzzle puzzle) {
            return puzzle.Moves.Any(m => Math.Abs(m.Piece) == (int)ColorlessPiece.King && m.IsCapture);
        }

        [Feature]
        public static bool HasEnemyKingOnE8(LichessPuzzle puzzle) {
            if (puzzle.InitialPosition.WhiteToMove) {
                return puzzle.Positions.Any(p => p.Squares[116] == Piece.BlackKing);
            } else {
                return puzzle.Positions.Any(p => p.Squares[4] == Piece.WhiteKing);
            }
        }

        [Feature]
        public static bool HasEnemyKingOnG8(LichessPuzzle puzzle) {
            if (puzzle.InitialPosition.WhiteToMove) {
                return puzzle.Positions.Any(p => p.Squares[118] == Piece.BlackKing);
            } else {
                return puzzle.Positions.Any(p => p.Squares[6] == Piece.WhiteKing);
            }
        }

        [Feature]
        public static bool HasEnemyKingOnH8(LichessPuzzle puzzle) {
            if (puzzle.InitialPosition.WhiteToMove) {
                return puzzle.Positions.Any(p => p.Squares[119] == Piece.BlackKing);
            } else {
                return puzzle.Positions.Any(p => p.Squares[7] == Piece.WhiteKing);
            }
        }


        [Feature]
        public static bool HasKnightCheckOnD7OrE7(LichessPuzzle puzzle) {
            int dest = puzzle.InitialPosition.WhiteToMove ? 99 : 19;
            for (int i = 1; i < puzzle.Moves.Length; i += 2) {
                if (Math.Abs(puzzle.Moves[i].Piece) == (int)ColorlessPiece.Knight && puzzle.Moves[i].IsCheck && (puzzle.Moves[i].End == dest || puzzle.Moves[i].End == (dest + 1))) { 
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Requires a bishop on g7, and pawns on f7 and g6
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        [Feature]
        public static bool HasEnemyKingsideFianchetto(LichessPuzzle puzzle) {
            if (puzzle.InitialPosition.WhiteToMove) {
                foreach (Position p in puzzle.Positions) {
                    if (p.Squares[102] == Piece.BlackBishop && p.Squares[101] == Piece.BlackPawn && p.Squares[86] == Piece.BlackPawn) {
                        return true;
                    }
                }
            } else {
                foreach (Position p in puzzle.Positions) {
                    if (p.Squares[14] == Piece.WhiteBishop && p.Squares[13] == Piece.WhitePawn && p.Squares[22] == Piece.WhitePawn) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
