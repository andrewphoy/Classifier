using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dragon.Chess {
    public static class MoveGenerator {

        public static IEnumerable<Move> LegalMoves(Position p, bool whiteToMove, string filter = null) {
            var pseudoLegal = PseudoLegalMoves(p, whiteToMove, filter);

            //TODO should we order these moves?
            foreach (var move in pseudoLegal) {
                if (whiteToMove && p.WhiteCastling != Castling.None) {
                    if (move.Start == 4) {
                        move.Transitions.Add(Transition.Castling(p.WhiteCastling, Castling.None));
                    } else if (move.Start == 0) {
                        move.Transitions.Add(Transition.Castling(p.WhiteCastling, p.WhiteCastling & Castling.Kingside));
                    } else if (move.Start == 7) {
                        move.Transitions.Add(Transition.Castling(p.WhiteCastling, p.WhiteCastling & Castling.Queenside));
                    }
                }

                if (!whiteToMove && p.BlackCastling != Castling.None) {
                    if (move.Start == 116) {
                        move.Transitions.Add(Transition.Castling(p.BlackCastling, Castling.None));
                    } else if (move.Start == 112) {
                        move.Transitions.Add(Transition.Castling(p.BlackCastling, p.BlackCastling & Castling.Kingside));
                    } else if (move.Start == 119) {
                        move.Transitions.Add(Transition.Castling(p.BlackCastling, p.BlackCastling & Castling.Queenside));
                    }
                }

                var next = CreateNextPosition(p, move);
                move.ResultingPosition = next;

                if (next.EnPassant.HasValue && !HasLegalEnPassant(next)) {
                    var enPassantTrans = move.Transitions.First(t => t.Type == TransitionType.EnPassant);
                    move.Transitions.Remove(enPassantTrans);
                }

                if (move.San.StartsWith("O-")) {
                    // is a castling move
                    // all squares from start to end inclusive must not be attacked (we can skip end because it will be checked immediately below)
                    // because we only love regular chess, this can be simplified to the king's square (not in check) and the adjacent square
                    int kingDelta = (move.End > move.Start) ? 1 : -1;
                    if (IsSquareAttacked(p, whiteToMove, move.Start) || IsSquareAttacked(p, whiteToMove, move.Start + kingDelta)) {
                        continue;
                    }
                }

                if (IsKingAttacked(next, whiteToMove)) {
                    // is check, skip move
                } else {
                    yield return move;
                }
            }
        }

        public static void DecorateMove(Move move) {
            if (IsKingAttacked(move.ResultingPosition, move.ResultingPosition.WhiteToMove)) {
                move.IsCheck = true;

                // also see if it's mate
                var nextLegals = LegalMoves(move.ResultingPosition, move.ResultingPosition.WhiteToMove);
                if (nextLegals.Count() == 0) {
                    move.IsMate = true;
                }
            }
        }

        private static bool HasLegalEnPassant(Position p) {
            var legals = LegalMoves(p, p.WhiteToMove);
            return legals.Any(m => m.IsEnPassantCapture);
        }

        internal static Position CreateNextPosition(Position p, Move m) {
            var next = new Position();
            next.Squares = new Piece[128];
            p.Squares.CopyTo(next.Squares, 0);
            next.ColorToMove = p.ColorToMove * -1;
            next.WhiteCastling = p.WhiteCastling;
            next.BlackCastling = p.BlackCastling;
            next.EnPassant = null;

            // first do any remove transitions
            foreach (var transition in m.Transitions.Where(t => t.Type == TransitionType.Remove)) {
                next.Squares[transition.FirstSquare] = 0;
            }

            foreach (var transition in m.Transitions.Where(t => t.Type != TransitionType.Remove)) {
                switch (transition.Type) {
                    case TransitionType.Add:
                        next.Squares[transition.FirstSquare] = transition.Piece;
                        break;

                    case TransitionType.Castling:
                        if (p.WhiteToMove) {
                            next.WhiteCastling = transition.NewCastling;
                        } else {
                            next.BlackCastling = transition.NewCastling;
                        }
                        break;

                    case TransitionType.EnPassant:
                        next.EnPassant = transition.File;
                        break;

                    case TransitionType.PawnPromo:
                        throw new ArgumentException("Cannot promote since Andrew is lazy");
                        break;

                    case TransitionType.Move:
                        next.Squares[transition.SecondSquare] = next.Squares[transition.FirstSquare];
                        next.Squares[transition.FirstSquare] = 0;
                        break;

                        //case TransitionType.ResetCount:
                        //    next.HalfMoveCount = 0;
                        //    break;
                }
            }

            return next;
        }


        public static IEnumerable<Move> PseudoLegalMoves(Position p, bool whiteToMove, string filter = null) {
            var moves = new List<Move>();

            Piece? filterPiece = null;

            if (!string.IsNullOrEmpty(filter)) {
                char c = filter[0];
                if (char.IsLower(c)) {
                    // pawn move
                    filterPiece = whiteToMove ? Piece.WhitePawn : Piece.BlackPawn;
                } else if (c == 'O') {
                    // castling
                    filterPiece = whiteToMove ? Piece.WhiteKing : Piece.BlackKing;
                } else {
                    var colorless = c.GetPiece();
                    if (whiteToMove) {
                        filterPiece = (Piece)((int)colorless * 1);
                    } else {
                        filterPiece = (Piece)((int)colorless * -1);
                    }
                }
            }

            Piece piece;
            for (int i = 0; i < 120; i++) {
                if (p.Squares[i] == 0) {
                    continue;
                }

                piece = p.Squares[i];
                if (filterPiece.HasValue && piece != filterPiece) {
                    continue;
                }

                if (whiteToMove && piece > 0) {
                    // white piece and white to move
                    if (piece == Piece.WhitePawn) {
                        moves.AddRange(GetMovesForPawn(p, whiteToMove, i, piece));
                    } else {
                        moves.AddRange(GetMovesForPiece(p, whiteToMove, i, piece));
                    }
                } else if (!whiteToMove && piece < 0) {
                    if (piece == Piece.BlackPawn) {
                        moves.AddRange(GetMovesForPawn(p, whiteToMove, i, piece));
                    } else {
                        moves.AddRange(GetMovesForPiece(p, whiteToMove, i, piece));
                    }
                }
            }

            // disambiguate here because it's evil to rely on pins
            foreach (var m in moves) {
                bool hasAmbig = false;
                bool hasFileAmbig = false;
                bool hasRankAmbig = false;

                foreach (var other in moves) {
                    if (other.Equals(m)) {
                        continue;
                    }
                    if (m.San == other.San) {
                        hasAmbig = true;
                    }
                    if (m.RankSan == other.RankSan) {
                        hasRankAmbig = true;
                    }
                    if (m.FileSan == other.FileSan) {
                        hasFileAmbig = true;
                    }
                }

                if (hasAmbig) {
                    if (!hasFileAmbig) {
                        m.CorrectSan = m.FileSan;
                        m.IsFileDisambig = true;
                        continue;
                    }
                    if (!hasRankAmbig) {
                        m.CorrectSan = m.RankSan;
                        m.IsRankDisambig = true;
                        continue;
                    }
                    m.CorrectSan = m.FullSan;
                    m.IsFileDisambig = true;
                    m.IsRankDisambig = true;
                } else {
                    m.CorrectSan = m.San;
                }
            }

            return moves;
        }

        public static bool IsKingAttacked(Position p, bool whiteKing) {
            Piece king = whiteKing ? Piece.WhiteKing : Piece.BlackKing;

            int kingSquare = -1;
            for (int i = 0; i < 120; i++) {
                if (p.Squares[i] == king) {
                    kingSquare = i;
                    break;
                }
            }
            if (kingSquare < 0) {
                // if we no longer have a king, it has been captured
                return true;
                //throw new ArgumentOutOfRangeException("Could not find friendly king");
            }

            return IsSquareAttacked(p, whiteKing, kingSquare);
        }

        public static IEnumerable<int> GetKingAttackers(Position p, bool whiteKing) {
            Piece king = whiteKing ? Piece.WhiteKing : Piece.BlackKing;

            int kingSquare = -1;
            for (int i = 0; i < 120; i++) {
                if (p.Squares[i] == king) {
                    kingSquare = i;
                    break;
                }
            }
            if (kingSquare < 0) {
                // if we no longer have a king, it has been captured
                throw new ArgumentOutOfRangeException("Could not find king");
            }

            return GetAttackingSquares(p, whiteKing, kingSquare);
        }

        public static IEnumerable<int> GetAttackingSquares(Position p, bool whiteAttacked, int square) {
            // knights and pawns are easy because they're contact and special cases
            int[] knightDeltas = new int[] { -33, -31, -18, -14, 14, 18, 31, 33 };
            Piece enemyKnight = whiteAttacked ? Piece.BlackKnight : Piece.WhiteKnight;
            foreach (int delta in knightDeltas) {
                if (((square + delta) & 0x88) == 0) {
                    if (p.Squares[square + delta] == enemyKnight) {
                        yield return square + delta;
                    }
                }
            }

            int[] pawnDeltas = whiteAttacked ? new int[] { 15, 17 } : new int[] { -15, -17 };
            Piece enemyPawn = whiteAttacked ? Piece.BlackPawn : Piece.WhitePawn;
            foreach (int delta in pawnDeltas) {
                if (((square + delta) & 0x88) == 0) {
                    if (p.Squares[square + delta] == enemyPawn) {
                        yield return square + delta;
                    }
                }
            }

            int ixChecker;
            Piece piece;

            int[] horizontalDeltas = new int[] { 1, -1, 16, -16 };
            foreach (int delta in horizontalDeltas) {
                ixChecker = square + delta;
                bool isFirstSquare = true;

                // loop while the square is on the board
                while ((ixChecker & 0x88) == 0) {
                    piece = p.Squares[ixChecker];
                    if (piece != 0) {
                        // occupied

                        if (whiteAttacked) {
                            if (piece == Piece.BlackRook || piece == Piece.BlackQueen) {
                                yield return ixChecker;
                            }
                        } else {
                            if (piece == Piece.WhiteRook || piece == Piece.WhiteQueen) {
                                yield return ixChecker;
                            }
                        }

                        if (isFirstSquare) {
                            if (whiteAttacked && piece == Piece.BlackKing) {
                                yield return ixChecker;
                            }
                            if (!whiteAttacked && piece == Piece.WhiteKing) {
                                // adjacent kings
                                yield return ixChecker;
                            }
                        }

                        break;
                    } else {
                        ixChecker += delta;
                        isFirstSquare = false;
                    }
                }
            }

            int[] diagonalDeltas = new int[] { 15, 17, -15, -17 };
            foreach (int delta in diagonalDeltas) {
                ixChecker = square + delta;
                bool isFirstSquare = true;

                // loop while the square is on the board
                while ((ixChecker & 0x88) == 0) {
                    piece = p.Squares[ixChecker];
                    if (piece != 0) {
                        // occupied

                        if (whiteAttacked) {
                            if (piece == Piece.BlackBishop || piece == Piece.BlackQueen) {
                                yield return ixChecker;
                            }
                        } else {
                            if (piece == Piece.WhiteBishop || piece == Piece.WhiteQueen) {
                                yield return ixChecker;
                            }
                        }

                        if (isFirstSquare) {
                            if (piece == Piece.WhiteKing || piece == Piece.BlackKing) {
                                // adjacent kings
                                yield return ixChecker;
                            }
                        }

                        break;
                    } else {
                        ixChecker += delta;
                        isFirstSquare = false;
                    }
                }
            }
        }

        public static bool IsSquareAttacked(Position p, bool whiteAttacked, int square) {
            // knights and pawns are easy because they're contact and special cases
            int[] knightDeltas = new int[] { -33, -31, -18, -14, 14, 18, 31, 33 };
            Piece enemyKnight = whiteAttacked ? Piece.BlackKnight : Piece.WhiteKnight;
            foreach (int delta in knightDeltas) {
                if (((square + delta) & 0x88) == 0) {
                    if (p.Squares[square + delta] == enemyKnight) {
                        return true;
                    }
                }
            }

            int[] pawnDeltas = whiteAttacked ? new int[] { 15, 17 } : new int[] { -15, -17 };
            Piece enemyPawn = whiteAttacked ? Piece.BlackPawn : Piece.WhitePawn;
            foreach (int delta in pawnDeltas) {
                if (((square + delta) & 0x88) == 0) {
                    if (p.Squares[square + delta] == enemyPawn) {
                        return true;
                    }
                }
            }

            int ixChecker;
            Piece piece;

            int[] horizontalDeltas = new int[] { 1, -1, 16, -16 };
            foreach (int delta in horizontalDeltas) {
                ixChecker = square + delta;
                bool isFirstSquare = true;

                // loop while the square is on the board
                while ((ixChecker & 0x88) == 0) {
                    piece = p.Squares[ixChecker];
                    if (piece != 0) {
                        // occupied

                        if (whiteAttacked) {
                            if (piece == Piece.BlackRook || piece == Piece.BlackQueen) {
                                return true;
                            }
                        } else {
                            if (piece == Piece.WhiteRook || piece == Piece.WhiteQueen) {
                                return true;
                            }
                        }

                        if (isFirstSquare) {
                            if (whiteAttacked && piece == Piece.BlackKing) {
                                return true;
                            }
                            if (!whiteAttacked && piece == Piece.WhiteKing) {
                                // adjacent kings
                                return true;
                            }
                        }

                        break;
                    } else {
                        ixChecker += delta;
                        isFirstSquare = false;
                    }
                }
            }

            int[] diagonalDeltas = new int[] { 15, 17, -15, -17 };
            foreach (int delta in diagonalDeltas) {
                ixChecker = square + delta;
                bool isFirstSquare = true;

                // loop while the square is on the board
                while ((ixChecker & 0x88) == 0) {
                    piece = p.Squares[ixChecker];
                    if (piece != 0) {
                        // occupied

                        if (whiteAttacked) {
                            if (piece == Piece.BlackBishop || piece == Piece.BlackQueen) {
                                return true;
                            }
                        } else {
                            if (piece == Piece.WhiteBishop || piece == Piece.WhiteQueen) {
                                return true;
                            }
                        }

                        if (isFirstSquare) {
                            if (piece == Piece.WhiteKing || piece == Piece.BlackKing) {
                                // adjacent kings
                                return true;
                            }
                        }

                        break;
                    } else {
                        ixChecker += delta;
                        isFirstSquare = false;
                    }
                }
            }


            return false;
        }

        private static Move GenerateMove(Position p, int start, int end) {
            Move m = new Move() { Start = start, End = end };
            var piece = p.Squares[start];
            m.Piece = (int)piece;
            m.Transitions = new List<Transition>();
            m.Transitions.Add(new Transition(start, piece, TransitionType.Remove));
            m.Transitions.Add(new Transition(end, piece, TransitionType.Add));

            if (p.Squares[end] != 0) {
                // capture
                m.Transitions.Add(new Transition(end, p.Squares[end], TransitionType.Remove));
            }

            return m;
        }

        private static IEnumerable<Move> GetPawnPromotions(Position p, bool whiteToMove, int square, Piece piece) {
            var plusOneSquare = square + (16 * (whiteToMove ? 1 : -1));
            var rank = square / 16;

            // pawn promotion
            int promoCaptureLeft, promoCaptureRight;
            if (whiteToMove && rank == 6) {
                if ((plusOneSquare & 0x88) == 0 && p.Squares[plusOneSquare] == 0) {
                    string baseSan = plusOneSquare.GetSanSquare() + "=";
                    Piece[] promos = new Piece[] { Piece.WhiteQueen, Piece.WhiteRook, Piece.WhiteBishop, Piece.WhiteKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = plusOneSquare };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(plusOneSquare, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + plusOneSquare.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }
                promoCaptureLeft = plusOneSquare - 1;
                if ((promoCaptureLeft & 0x88) == 0 && p.Squares[promoCaptureLeft] < 0) {
                    string baseSan = square.GetSanSquare()[0] + "x" + promoCaptureLeft.GetSanSquare() + "=";

                    Piece[] promos = new Piece[] { Piece.WhiteQueen, Piece.WhiteRook, Piece.WhiteBishop, Piece.WhiteKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = promoCaptureLeft };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureLeft, p.Squares[promoCaptureLeft], TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureLeft, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + promoCaptureLeft.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsCapture = true;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }
                promoCaptureRight = plusOneSquare + 1;
                if ((promoCaptureRight & 0x88) == 0 && p.Squares[promoCaptureRight] < 0) {
                    string baseSan = square.GetSanSquare()[0] + "x" + promoCaptureRight.GetSanSquare() + "=";

                    Piece[] promos = new Piece[] { Piece.WhiteQueen, Piece.WhiteRook, Piece.WhiteBishop, Piece.WhiteKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = promoCaptureLeft };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureRight, p.Squares[promoCaptureRight], TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureRight, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + promoCaptureRight.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsCapture = true;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }

                yield break;
            }

            if (!whiteToMove && rank == 1) {
                if ((plusOneSquare & 0x88) == 0 && p.Squares[plusOneSquare] == 0) {
                    string baseSan = plusOneSquare.GetSanSquare() + "=";
                    Piece[] promos = new Piece[] { Piece.BlackQueen, Piece.BlackRook, Piece.BlackBishop, Piece.BlackKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = plusOneSquare };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(plusOneSquare, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + plusOneSquare.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }
                promoCaptureLeft = plusOneSquare - 1;
                if ((promoCaptureLeft & 0x88) == 0 && p.Squares[promoCaptureLeft] > 0) {
                    string baseSan = square.GetSanSquare()[0] + "x" + promoCaptureLeft.GetSanSquare() + "=";

                    Piece[] promos = new Piece[] { Piece.BlackQueen, Piece.BlackRook, Piece.BlackBishop, Piece.BlackKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = promoCaptureLeft };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureLeft, p.Squares[promoCaptureLeft], TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureLeft, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + promoCaptureLeft.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsCapture = true;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }
                promoCaptureRight = plusOneSquare + 1;
                if ((promoCaptureRight & 0x88) == 0 && p.Squares[promoCaptureRight] > 0) {
                    string baseSan = square.GetSanSquare()[0] + "x" + promoCaptureRight.GetSanSquare() + "=";

                    Piece[] promos = new Piece[] { Piece.BlackQueen, Piece.BlackRook, Piece.BlackBishop, Piece.BlackKnight };
                    foreach (Piece promo in promos) {
                        Move m = new Move() { Start = square, End = promoCaptureLeft };
                        m.Piece = (int)promo;
                        m.Transitions = new List<Transition>();
                        m.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureRight, p.Squares[promoCaptureRight], TransitionType.Remove));
                        m.Transitions.Add(new Transition(promoCaptureRight, promo, TransitionType.Add));
                        m.San = baseSan + promo.GetSanPiece();
                        m.Uci = square.GetSanSquare() + promoCaptureRight.GetSanSquare() + promo.GetSanPiece().ToLower();
                        m.FullSan = m.San;
                        m.IsCapture = true;
                        m.IsPawnMove = true;
                        m.IsPromotion = true;
                        yield return m;
                    }
                }

                yield break;
            }
        }

        private static IEnumerable<Move> GetMovesForPawn(Position p, bool whiteToMove, int square, Piece piece) {
            var plusOneSquare = square + (16 * (whiteToMove ? 1 : -1));
            var plusTwoSquare = square + (32 * (whiteToMove ? 1 : -1));
            var rank = square / 16;

            if ((whiteToMove && rank == 6) || (!whiteToMove && rank == 1)) {
                foreach (var promoMove in GetPawnPromotions(p, whiteToMove, square, piece)) {
                    yield return promoMove;
                }
                yield break;
            }

            // move forward one
            if ((plusOneSquare & 0x88) == 0 && p.Squares[plusOneSquare] == 0) {
                Move m = GenerateMove(p, square, plusOneSquare);
                m.San = plusOneSquare.GetSanSquare();
                m.Uci = square.GetSanSquare() + plusOneSquare.GetSanSquare();
                m.FullSan = m.San;
                m.IsPawnMove = true;
                m.IsCapture = false;
                yield return m;

                // move forward two
                if ((plusTwoSquare & 0x88) == 0 && p.Squares[plusTwoSquare] == 0) {
                    if (whiteToMove && rank == 1) {
                        Move m2 = GenerateMove(p, square, plusTwoSquare);
                        if (p.Squares[plusTwoSquare + 1] == Piece.BlackPawn || p.Squares[plusTwoSquare - 1] == Piece.BlackPawn) {
                            m2.Transitions.Add(Transition.EnPassant(square % 16));
                        }
                        m2.San = plusTwoSquare.GetSanSquare();
                        m2.Uci = square.GetSanSquare() + plusTwoSquare.GetSanSquare();
                        m2.FullSan = m2.San;
                        m2.IsPawnMove = true;
                        m2.IsCapture = false;
                        yield return m2;
                    }
                    if (!whiteToMove && rank == 6) {
                        Move m2 = GenerateMove(p, square, plusTwoSquare);
                        if (p.Squares[plusTwoSquare + 1] == Piece.WhitePawn || p.Squares[plusTwoSquare - 1] == Piece.WhitePawn) {
                            m2.Transitions.Add(Transition.EnPassant(square % 16));
                        }
                        m2.San = plusTwoSquare.GetSanSquare();
                        m2.Uci = square.GetSanSquare() + plusTwoSquare.GetSanSquare();
                        m2.FullSan = m2.San;
                        m2.IsPawnMove = true;
                        m2.IsCapture = false;
                        yield return m2;
                    }
                }
            }

            int file = square % 16;
            // white en passant
            if (p.EnPassant.HasValue && whiteToMove && rank == 4) {
                if ((p.EnPassant.Value + 1) == file && ((plusOneSquare - 1) & 0x88) == 0 && p.Squares[plusOneSquare - 1] == 0 && p.Squares[square - 1] == Piece.BlackPawn) {
                    Move m = GenerateMove(p, square, plusOneSquare - 1);
                    m.Transitions.Add(new Transition(square - 1, Piece.BlackPawn, TransitionType.Remove));
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare - 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare - 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    m.IsEnPassantCapture = true;
                    yield return m;
                }
                if ((p.EnPassant.Value - 1) == file && ((plusOneSquare + 1) & 0x88) == 0 && p.Squares[plusOneSquare + 1] == 0 && p.Squares[square + 1] == Piece.BlackPawn) {
                    Move m = GenerateMove(p, square, plusOneSquare + 1);
                    m.Transitions.Add(new Transition(square + 1, Piece.BlackPawn, TransitionType.Remove));
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare + 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare + 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    m.IsEnPassantCapture = true;
                    yield return m;
                }
            }
            // black en passant
            if (p.EnPassant.HasValue && !whiteToMove && rank == 3) {
                if ((p.EnPassant.Value + 1) == file && ((plusOneSquare - 1) & 0x88) == 0 && p.Squares[plusOneSquare - 1] == 0 && p.Squares[square - 1] == Piece.WhitePawn) {
                    Move m = GenerateMove(p, square, plusOneSquare - 1);
                    m.Transitions.Add(new Transition(square - 1, Piece.WhitePawn, TransitionType.Remove));
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare - 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare - 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    m.IsEnPassantCapture = true;
                    yield return m;
                }
                if ((p.EnPassant.Value - 1) == file && ((plusOneSquare + 1) & 0x88) == 0 && p.Squares[plusOneSquare + 1] == 0 && p.Squares[square + 1] == Piece.WhitePawn) {
                    Move m = GenerateMove(p, square, plusOneSquare + 1);
                    m.Transitions.Add(new Transition(square + 1, Piece.WhitePawn, TransitionType.Remove));
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare + 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare + 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    m.IsEnPassantCapture = true;
                    yield return m;
                }
            }


            // capture left
            if (((plusOneSquare - 1) & 0x88) == 0 && p.Squares[plusOneSquare - 1] != 0) {
                if (whiteToMove && p.Squares[plusOneSquare - 1] < 0) {
                    Move m = GenerateMove(p, square, plusOneSquare - 1);
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare - 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare - 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    yield return m;
                }
                if (!whiteToMove && p.Squares[plusOneSquare - 1] > 0) {
                    Move m = GenerateMove(p, square, plusOneSquare - 1);
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare - 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare - 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    yield return m;
                }
            }

            // capture right
            if (((plusOneSquare + 1) & 0x88) == 0 && p.Squares[plusOneSquare + 1] != 0) {
                if (whiteToMove && p.Squares[plusOneSquare + 1] < 0) {
                    Move m = GenerateMove(p, square, plusOneSquare + 1);
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare + 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare + 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    yield return m;
                }
                if (!whiteToMove && p.Squares[plusOneSquare + 1] > 0) {
                    Move m = GenerateMove(p, square, plusOneSquare + 1);
                    m.San = square.GetSanSquare()[0] + "x" + (plusOneSquare + 1).GetSanSquare();
                    m.Uci = square.GetSanSquare() + (plusOneSquare + 1).GetSanSquare();
                    m.FullSan = m.San;
                    m.IsPawnMove = true;
                    m.IsCapture = true;
                    yield return m;
                }
            }
        }

        private static IEnumerable<Move> GetMovesForPiece(Position p, bool whiteToMove, int square, Piece piece) {
            List<Move> moves = new List<Move>();

            bool isSliding = piece.IsSliding();
            var deltas = piece.GetDeltas();
            var dests = new List<int>();

            int ixDest, delta;

            // handle non-pawns
            for (int i = 0; i < deltas.Length; i++) {
                delta = deltas[i];
                ixDest = square + delta;

                // loop while the square is on the board
                while ((ixDest & 0x88) == 0) {
                    // check if the square is occupied
                    if (p.Squares[ixDest] != 0) {
                        if (whiteToMove) {
                            if (p.Squares[ixDest] > 0) {
                                // white moving, white piece
                                break;
                            } else {
                                Move m = GenerateMove(p, square, ixDest);
                                m.San = piece.GetSanPiece() + "x" + ixDest.GetSanSquare();
                                m.Uci = square.GetSanSquare() + ixDest.GetSanSquare();
                                m.FileSan = piece.GetSanPiece() + square.GetSanSquare()[0] + "x" + ixDest.GetSanSquare();
                                m.RankSan = piece.GetSanPiece() + square.GetSanSquare()[1] + "x" + ixDest.GetSanSquare();
                                m.FullSan = piece.GetSanPiece() + square.GetSanSquare() + "x" + ixDest.GetSanSquare();
                                m.IsPawnMove = false;
                                m.IsCapture = true;
                                moves.Add(m);
                                break;
                            }
                        } else {
                            if (p.Squares[ixDest] < 0) {
                                // black moving, black piece
                                break;
                            } else {
                                Move m = GenerateMove(p, square, ixDest);
                                m.San = piece.GetSanPiece() + "x" + ixDest.GetSanSquare();
                                m.Uci = square.GetSanSquare() + ixDest.GetSanSquare();
                                m.FileSan = piece.GetSanPiece() + square.GetSanSquare()[0] + "x" + ixDest.GetSanSquare();
                                m.RankSan = piece.GetSanPiece() + square.GetSanSquare()[1] + "x" + ixDest.GetSanSquare();
                                m.FullSan = piece.GetSanPiece() + square.GetSanSquare() + "x" + ixDest.GetSanSquare();
                                m.IsPawnMove = false;
                                m.IsCapture = true;
                                moves.Add(m);
                                break;
                            }
                        }
                    } else {
                        // add the square as a destination
                        Move m = GenerateMove(p, square, ixDest);
                        m.San = piece.GetSanPiece() + ixDest.GetSanSquare();
                        m.Uci = square.GetSanSquare() + ixDest.GetSanSquare();
                        m.FileSan = piece.GetSanPiece() + square.GetSanSquare()[0] + ixDest.GetSanSquare();
                        m.RankSan = piece.GetSanPiece() + square.GetSanSquare()[1] + ixDest.GetSanSquare();
                        m.FullSan = piece.GetSanPiece() + square.GetSanSquare() + ixDest.GetSanSquare();
                        m.IsPawnMove = false;
                        m.IsCapture = false;
                        moves.Add(m);
                    }

                    ixDest += delta;

                    if (!isSliding) {
                        break;
                    }
                }
            }

            // extra case for castling
            if (piece.IsKing()) {
                AddCastlingMoves(p, whiteToMove, square, piece, moves);
            }

            return moves;
        }


        private static void AddCastlingMoves(Position p, bool whiteToMove, int square, Piece piece, List<Move> moves) {
            // if the king is on its own start square, we might be able to castle
            // we oversimplify here because we don't really care about legality
            // we can castle through check (with this code) and we can castle with a previously moved rook
            Piece rook = 0;
            if (whiteToMove) {
                rook = Piece.WhiteRook;
                if (square == 4) {
                    if (p.Squares[5] == 0 && p.Squares[6] == 0 && p.Squares[7] == rook) {
                        // kside castling
                        Move kside = new Move() { Start = square, End = 6, IsCastling = true };
                        kside.Piece = (int)Piece.WhiteKing;
                        kside.Transitions = new List<Transition>();
                        kside.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        kside.Transitions.Add(new Transition(7, rook, TransitionType.Remove));
                        kside.Transitions.Add(new Transition(6, piece, TransitionType.Add));
                        kside.Transitions.Add(new Transition(5, rook, TransitionType.Add));
                        kside.San = "O-O";
                        kside.Uci = square.GetSanSquare() + kside.End.GetSanSquare();
                        kside.FullSan = "O-O";
                        kside.IsPawnMove = false;
                        kside.IsCapture = false;
                        moves.Add(kside);
                    }
                    if (p.Squares[3] == 0 && p.Squares[2] == 0 && p.Squares[1] == 0 && p.Squares[0] == rook) {
                        // qside castling
                        Move qside = new Move() { Start = square, End = 2, IsCastling = true };
                        qside.Piece = (int)Piece.WhiteKing;
                        qside.Transitions = new List<Transition>();
                        qside.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        qside.Transitions.Add(new Transition(0, rook, TransitionType.Remove));
                        qside.Transitions.Add(new Transition(2, piece, TransitionType.Add));
                        qside.Transitions.Add(new Transition(3, rook, TransitionType.Add));
                        qside.San = "O-O-O";
                        qside.Uci = square.GetSanSquare() + qside.End.GetSanSquare();
                        qside.FullSan = "O-O-O";
                        qside.IsPawnMove = false;
                        qside.IsCapture = false;
                        moves.Add(qside);
                    }
                }
            } else {
                rook = Piece.BlackRook;
                if (square == 116) {
                    if (p.Squares[117] == 0 && p.Squares[118] == 0 && p.Squares[119] == rook) {
                        // kside castling
                        Move kside = new Move() { Start = square, End = 118, IsCastling = true };
                        kside.Piece = (int)Piece.BlackKing;
                        kside.Transitions = new List<Transition>();
                        kside.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        kside.Transitions.Add(new Transition(119, rook, TransitionType.Remove));
                        kside.Transitions.Add(new Transition(118, piece, TransitionType.Add));
                        kside.Transitions.Add(new Transition(117, rook, TransitionType.Add));
                        kside.San = "O-O";
                        kside.Uci = square.GetSanSquare() + kside.End.GetSanSquare();
                        kside.FullSan = "O-O";
                        kside.IsPawnMove = false;
                        kside.IsCapture = false;
                        moves.Add(kside);
                    }
                    if (p.Squares[115] == 0 && p.Squares[114] == 0 && p.Squares[113] == 0 && p.Squares[112] == rook) {
                        // qside castling
                        Move qside = new Move() { Start = square, End = 114, IsCastling = true };
                        qside.Piece = (int)Piece.BlackKing;
                        qside.Transitions = new List<Transition>();
                        qside.Transitions.Add(new Transition(square, piece, TransitionType.Remove));
                        qside.Transitions.Add(new Transition(112, rook, TransitionType.Remove));
                        qside.Transitions.Add(new Transition(114, piece, TransitionType.Add));
                        qside.Transitions.Add(new Transition(115, rook, TransitionType.Add));
                        qside.San = "O-O-O";
                        qside.Uci = square.GetSanSquare() + qside.End.GetSanSquare();
                        qside.FullSan = "O-O-O";
                        qside.IsPawnMove = false;
                        qside.IsCapture = false;
                        moves.Add(qside);
                    }
                }
            }
        }
    }
}
