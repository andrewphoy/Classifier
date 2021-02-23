using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dragon.Models;

namespace Dragon.Chess {
    public class ParsingGame {

        public ParsingGame(string body) {
            this.Position = new sbyte[128];
            this.ColorToMove = 1;
            this.StartPly = 1;
            this.StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            this.Body = body;
        }

        public void Parse() {
            this.ParseFen(this.StartFen);
            this.ParsePgn(this.Body);
            this.SetupProperties();
            this.ExtractStartState();
            this.GenerateAllTransitions();
        }

        public Dictionary<int, ParsingMove> Moves { get; set; }
        public int ParseCurrentMoveId { get; set; }
        public string Body { get; set; }
        public string StartFen { get; set; }
        public int StartPly { get; set; }
        public string Result { get; set; }
        public string GameComment { get; set; }

        public int CurrentMoveId { get; set; }
        public int ColorToMove { get; set; }
        public int? EnPassant { get; set; }
        public int CurrentPly { get; set; }
        public int HalfMoveCount { get; set; }
        public sbyte[] Position { get; set; }
        public int WhiteKingPosition { get; set; }
        public int BlackKingPosition { get; set; }
        public int WhiteCastling { get; set; }
        public int BlackCastling { get; set; }
        public BoardState StartState { get; set; }

        private struct ChessRegex {
            public static Regex Suffix = new Regex("((?:\\s*\\$\\d+)*)(\\s*\\{[^\\}]*\\})*", RegexOptions.Compiled);
            public static Regex LeadingComment = new Regex("^\\s*\\{([^\\}]*)\\}", RegexOptions.Compiled);
            public static Regex MoveBodyExtra = new Regex("(.*?[a-h][1-8][+#]?)(.*)", RegexOptions.Compiled); // only check and checkmate are allowed as literals, everything else must be a nag or stripped
            public static Regex GameTermMarker = new Regex(@"(1-0|0-1|1\/2-1\/2|\*)\s*$", RegexOptions.Compiled);
            public static Regex CommentLeftParen = new Regex(@"\{([^}]*?)(\()(.*?)\}", RegexOptions.Compiled);
            public static Regex CommentRightParen = new Regex(@"\{([^}]*?)(\))(.*?)\}", RegexOptions.Compiled);

            public static Regex CastleQueenside = new Regex(@"^O-O-O", RegexOptions.Compiled);
            public static Regex CastleKingside = new Regex(@"^O-O", RegexOptions.Compiled);

            public static Regex PieceMove = new Regex(@"^([KQBNR])", RegexOptions.Compiled);
            public static Regex PieceDest = new Regex(@"(.*)([a-h])([1-8])", RegexOptions.Compiled);
            public static Regex PieceMoveFull = new Regex(@"^[KQBNR]([a-h])?([1-8])?x?[a-h][1-8]", RegexOptions.Compiled);

            public static Regex PawnMove = new Regex(@"^([a-h])([1-8])", RegexOptions.Compiled);
            public static Regex PawnCapture = new Regex(@"^([a-h])x([a-h])([1-8])", RegexOptions.Compiled);
            public static Regex PawnPromoSuffix = new Regex(@"=([QBNR])", RegexOptions.Compiled);
        }

        private void ExtractStartState() {
            this.StartState = new BoardState() {
                CurrentPly = this.CurrentPly,
                ColorToMove = this.ColorToMove,
                EnPassant = this.EnPassant,
                HalfMoveCount = this.HalfMoveCount,
                WhiteKingPosition = this.WhiteKingPosition,
                BlackKingPosition = this.BlackKingPosition,
                WhiteCastling = this.WhiteCastling,
                BlackCastling = this.BlackCastling
            };
        }

        private void SetupProperties() {
            this.CurrentMoveId = 0;
            this.CurrentPly = this.StartPly;
        }

        public int MoveCount() {
            return this.ParseCurrentMoveId;
        }

        #region Parsing Input
        private bool ValidateFen(string fen) {
            var fenRegex = new Regex(@"\s*([rnbqkpRNBQKP1-8]+\/){7}([rnbqkpRNBQKP1-8]+)\s[bw-]\s(([a-hkqA-HKQ]{1,4})|(-))\s(([a-h][36])|(-))\s\d+\s\d+\s*");
            return fenRegex.IsMatch(fen);
        }

        private void ParseFen(string fen) {
            if (!this.ValidateFen(fen)) {
                throw new ArgumentException("Invalid FEN, cannot continue", nameof(fen));
            }

            // ["5rk1/p4p2/1p2r2p/2p2Rp1/3b2P1/1P1P3P/P1PR3B/7K", "b", "-", "-", "0", "1"] 
            // 0) position
            // 1) colorToMove
            // 2) castling avail
            // 3) en passant file
            // 4) halfmove clock
            // 5) fullmove number

            var dataGroups = fen.Split(new char[] { ' ' });
            var rows = dataGroups[0].Split(new char[] { '/' });

            var offset = 112;
            var ixChar = 0;

            for (var col = 0; col < 8; col++) {
                ixChar = 0;
                for (var i = 0; i < 8; i++) {
                    if (rows[col].Length > ixChar) {
                        char charNext = rows[col][ixChar];
                        if (char.IsLetter(charNext)) {
                            // is a piece

                            // -1 is black, 1 is white
                            var color = (charNext.ToString().ToLower() == charNext.ToString()) ? -1 : 1;
                            var piece = charNext.ToString().GetPiece();
                            this.Position[offset + i] = (sbyte)(color * (int)piece);

                            // is it a king?
                            if (piece == ColorlessPiece.King) {
                                if (color == 1) {
                                    this.WhiteKingPosition = offset + i;
                                } else {
                                    this.BlackKingPosition = offset + i;
                                }
                            }
                        } else {
                            // is an empty square
                            i += (int.Parse(charNext.ToString()) - 1);
                        }
                    }
                    ixChar++;
                }
                offset -= 16;
            }

            if (dataGroups[1] == "b") {
                this.ColorToMove = -1;
            }

            // castling (-, KQ, Kk, kq, etc)
            if (dataGroups[2] == "-") {
                this.WhiteCastling = (int)Castling.None;
                this.BlackCastling = (int)Castling.None;
            } else {
                // one or both sides can castle
                // Both: 3,
                // Queenside: 2,
                // Kingside: 1,
                // None: 0
                int whiteCastling = 0;
                int blackCastling = 0;
                foreach (char c in dataGroups[2]) {
                    switch (c) {
                        case 'K':
                            whiteCastling = whiteCastling | 1;
                            break;
                        case 'Q':
                            whiteCastling = whiteCastling | 2;
                            break;
                        case 'k':
                            blackCastling = blackCastling | 1;
                            break;
                        case 'q':
                            blackCastling = blackCastling | 2;
                            break;
                        default:
                            throw new PgnException("Unknown castling availability - FRC not yet supported, blame the lazy developer");
                    }
                }
                this.WhiteCastling = whiteCastling;
                this.BlackCastling = blackCastling;
            }

            // en passant square (a3, a6, -) 
            if (dataGroups[3] != "-") {
                this.EnPassant = dataGroups[3][0] - 'a';
                // also add this as a transition to the root move
                //TODO add as a transition (or some way to set the en passant back to a value when we get to the start of the game)
            } else {
                this.EnPassant = null;
            }

            // halfmove clock
            this.HalfMoveCount = int.Parse(dataGroups[4]);

            // fullmove number
            int fullMoveNumber = int.Parse(dataGroups[5]);
            this.StartPly = fullMoveNumber * 2;
            if (this.ColorToMove == 1) {
                this.StartPly--;
            }
        }

        private void ParsePgn(string pgn) {
            if (string.IsNullOrEmpty(pgn)) {
                throw new ArgumentNullException("body");
            }

            pgn = pgn.Trim();

            // we have already removed headers in the preprocess step
            this.Moves = new Dictionary<int, ParsingMove>();
            this.Moves.Add(0, new ParsingMove(0, 0, "", -1, ""));
            this.Moves[0].Transitions = new List<ChessTransition>();
            this.ParseCurrentMoveId = 0;

            // the pgn spec (http://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm)
            // allows for end of line comments starting with ";" and escaping with "%"
            // we just ignore that as no major software emits pgn with this type of comment
            //TODO update parser to allow for end of line comments
            // deal with special comments (leading comment and EOL comments)

            var lcMatch = ChessRegex.LeadingComment.Match(pgn);
            if (lcMatch.Success) {
                string leadingComment = lcMatch.Groups[1].Value;
                if (!string.IsNullOrEmpty(leadingComment)) {
                    this.GameComment = leadingComment;
                    pgn = ChessRegex.LeadingComment.Replace(pgn, "");
                }
            }

            // replace any line breaks with a space
            pgn = pgn.Replace(Environment.NewLine, " ");

            // protect any parens in comments
            pgn = ChessRegex.CommentLeftParen.Replace(pgn, "{$1&lpar;$3}");
            pgn = ChessRegex.CommentRightParen.Replace(pgn, "{$1&rpar;$3}");

            // label the variations, hybrid between state machine and regex parsing
            pgn = this.LabelVariations(pgn);

            // replace parens
            pgn = pgn.Replace("&lpar;", "(");
            pgn = pgn.Replace("&rpar;", ")");

            // try to get the result of the game
            var gameTermMarker = ChessRegex.GameTermMarker.Match(pgn);
            if (gameTermMarker.Success) {
                this.Result = gameTermMarker.Value;
                pgn = ChessRegex.GameTermMarker.Replace(pgn, "");
            }

            // if the pgn is now empty, we have a placeholder game
            if (string.IsNullOrEmpty(pgn)) {
                return;
            }

            // now extract all of the moves from the pgn
            // figure out what the first move ply is
            var firstPly = -1;

            // get the first number match in the pgn
            var firstMoveMatch = Regex.Match(pgn, @"(\d+)(\.+)");
            if (firstMoveMatch.Success) {
                string strPly = firstMoveMatch.Groups[1].Value;
                int moveNum = Int32.Parse(strPly);
                int ply = moveNum * 2;
                if (firstMoveMatch.Groups[2].Value == ".") {
                    ply = ply - 1;
                }
                firstPly = ply;
            }

            if (firstPly != this.StartPly) {
                throw new PgnException("The start ply from the header does not match the first move in the game");
            }

            if (firstPly > 0) {
                this.ExtractMoves(0, firstPly, pgn);
            }

            // this.Moves now holds all of the moves from the pgn - which we assume has no errors
        }
        
        private string LabelVariations(string pgn) {
            int i = 0;
            bool hasVariations = true;
            Regex pattern = new Regex(@"\(([^(]*?)\)", RegexOptions.Compiled);

            while (hasVariations) {
                if (pattern.IsMatch(pgn)) {
                    pgn = pattern.Replace(pgn, "%%" + i + "% $1 %" + i + "%%");
                }

                hasVariations = false;
                if (pattern.IsMatch(pgn)) {
                    hasVariations = true;
                    i = i + 1;
                }
            }

            return pgn;
        }
        
        private void CleanMove(ref string moveBody, ref string nag) {
            var match = ChessRegex.MoveBodyExtra.Match(moveBody);
            if (match.Captures.Count == 3) {
                // might have something to clean
                moveBody = match.Groups[1].Value.Trim();

                // can we convert a literal to a nag?
                string literal = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(literal)) {
                    string nagForLiteral = NagConverter.GetNagsForLiteral(literal);
                    if (!string.IsNullOrEmpty(nagForLiteral)) {
                        nag = nagForLiteral + nag;
                    }
                }
            }
        }

        private void ExtractMoves(int parentId, int ply, string pgn) {
            // get the number (ply 1 = 1., ply 2 = 1..., etc.)
            var numberPattern = this.PatternForPly(ply);

            // process all the variations for this particular ply
            var variationPattern = "%%(\\d+)%\\s*" + numberPattern + "\\s*(\\w\\S+)" + ChessRegex.Suffix + "(?:\\s+(.*?))?%\\1%%";
            var variationRegex = new Regex(variationPattern);

            string moveBody;
            string line;
            string comment;
            string nag;

            foreach (Match m in variationRegex.Matches(pgn)) {
                moveBody = m.Groups[2].Value.Trim();
                nag = m.Groups[3].Value.Trim();
                this.CleanMove(ref moveBody, ref nag);
                comment = m.Groups[4].Value.TrimStart(new char[] { ' ', '{' }).TrimEnd(new char[] { ' ', '}' });

                // store in the moves dictionary
                this.ParseCurrentMoveId++;
                ParsingMove move = new ParsingMove(this.ParseCurrentMoveId, ply, moveBody, parentId, numberPattern.Replace("\\", ""), comment, nag);
                this.Moves.Add(this.ParseCurrentMoveId, move);
                this.Moves[parentId].Children.Add(move);

                line = m.Groups[5].Value;
                if (!string.IsNullOrEmpty(line)) {
                    line = line.Trim();
                    if (line.Length > 0 && char.IsLetter(line[0])) {
                        // if there are remaining mvoes and the first char is not a move number
                        line = this.PatternForPly(ply + 1).Replace("\\", "") + " " + line;
                    }
                    // move a level deeper within the variation
                    this.ExtractMoves(this.ParseCurrentMoveId, ply + 1, line);
                }
            }

            // remove the variations from the pgn
            pgn = variationRegex.Replace(pgn, "");

            // now process the main line
            var mainPattern = "(?:^|\\s+)" + numberPattern + "\\s*(\\w\\S+)" + ChessRegex.Suffix + "(?:\\s+(.*)|($))";
            var mainRegex = new Regex(mainPattern);

            Match main = mainRegex.Match(pgn);
            if (main.Success) {
                moveBody = main.Groups[1].Value.Trim();
                nag = main.Groups[2].Value.Trim();
                this.CleanMove(ref moveBody, ref nag);
                comment = main.Groups[3].Value.TrimStart(new char[] { ' ', '{' }).TrimEnd(new char[] { ' ', '}' });

                // store in the moves dictionary
                this.ParseCurrentMoveId++;
                ParsingMove move = new ParsingMove(this.ParseCurrentMoveId, ply, moveBody, parentId, numberPattern.Replace("\\", ""), comment, nag);
                this.Moves.Add(this.ParseCurrentMoveId, move);
                this.Moves[parentId].Children.Insert(0, move);

                line = main.Groups[4].Value;
                if (!string.IsNullOrEmpty(line)) {
                    line = line.Trim();
                    if (line.Length > 0 && char.IsLetter(line[0])) {
                        // if there are remaining moves and the first char is not a move number
                        line = this.PatternForPly(ply + 1).Replace("\\", "") + " " + line;
                    }
                    // move a level deeper
                    this.ExtractMoves(this.ParseCurrentMoveId, ply + 1, line);
                }
            }
        }

        private string PatternForPly(int ply) {
            var moveNumber = Math.Ceiling((decimal)ply / 2);
            var movePattern = moveNumber.ToString();
            var period = "\\.";
            if (ply % 2 == 0) {
                period = "\\.\\.\\.";
            }
            movePattern = movePattern + period;
            return movePattern;
        }
        #endregion

        #region Move Forward/Backward
        public bool EndOfGame() {
            return !(this.Moves[this.CurrentMoveId].Children.Count > 0);
        }

        public void ExecuteMoveForward(ParsingMove move) {
            // if the move isn't one of the current children, error
            if (move.ParentMoveId != this.CurrentMoveId) {
                throw new ArgumentException("Move is not available at this time", nameof(move));
            }

            this.ExecuteForwardTransitions(move);

            // and finally, update the game state (other than position)
            this.CurrentPly++;
            this.HalfMoveCount = move.HalfMoveCount;
            this.CurrentMoveId = move.MoveId;
            this.ColorToMove *= -1;
        }

        public void ExecuteMoveBackward() {
            if (this.CurrentPly > 0) {
                var lastMove = this.Moves[this.CurrentMoveId];

                this.ExecuteBackwardTransitions(lastMove);

                // update the game state (other than position)
                this.CurrentPly--;
                this.HalfMoveCount = lastMove.HalfMoveCount;
                this.CurrentMoveId = lastMove.ParentMoveId;
                this.ColorToMove *= -1;
            } else {
                throw new PgnException("Tried to go backward from the root move");
            }
        }

        private void ExecuteForwardTransitions(ParsingMove move) {
            if (move.Transitions == null || move.Transitions.Count == 0) {
                throw new PgnException("Transitions have not been defined yet");
            }

            this.EnPassant = null;
            foreach(ChessTransition t in move.Transitions) {
                switch (t.Type) {
                    case TransitionType.Move:
                        Position[t.SecondSquare] = Position[t.FirstSquare];
                        if (Position[t.SecondSquare] == (sbyte) Piece.WhiteKing) {
                            this.WhiteKingPosition = t.SecondSquare;
                        }
                        if (Position[t.SecondSquare] == (sbyte) Piece.BlackKing) {
                            this.BlackKingPosition = t.SecondSquare;
                        }

                        Position[t.FirstSquare] = (sbyte)Piece.Empty;
                        break;

                    case TransitionType.Add:
                        Position[t.FirstSquare] = t.Piece;
                        break;

                    case TransitionType.Remove:
                        Position[t.FirstSquare] = (sbyte)Piece.Empty;
                        break;

                    case TransitionType.EnPassant:
                        this.EnPassant = t.File;
                        break;

                    case TransitionType.PawnPromo:
                        // two parts: 1) remove pawn, 2) add promo piece
                        Position[t.FirstSquare] = t.Piece;
                        break;

                    case TransitionType.Castling:
                        // only update castling state, the move already happened
                        if (this.ColorToMove == 1) {
                            this.WhiteCastling = (int)t.NewCastling;
                        } else {
                            this.BlackCastling = (int)t.NewCastling;
                        }
                        break;

                    default:
                        throw new PgnException("Unknown transition type: " + t.Type.ToString());
                }
            }
        }

        private void ExecuteBackwardTransitions(ParsingMove move) {
            if (move.Transitions == null || move.Transitions.Count == 0) {
                throw new PgnException("Transitions have not been defined and we are trying to move backward");
            }

            // check to see if we have an en passant square
            this.EnPassant = null;
            foreach (ChessTransition tran in Moves[move.ParentMoveId].Transitions.Where(t => t.Type == TransitionType.EnPassant)) {
                this.EnPassant = tran.File;
            }

            // make the transitions in reverse order
            for (int i = move.Transitions.Count - 1; i >= 0; i--) {
                var t = move.Transitions[i];
                switch (t.Type) {
                    case TransitionType.Move:
                        Position[t.FirstSquare] = Position[t.SecondSquare];
                        if (Position[t.FirstSquare] == (sbyte)Piece.WhiteKing) {
                            this.WhiteKingPosition = t.FirstSquare;
                        }
                        if (Position[t.FirstSquare] == (sbyte)Piece.BlackKing) {
                            this.BlackKingPosition = t.FirstSquare;
                        }
                        Position[t.SecondSquare] = (sbyte)Piece.Empty;
                        break;

                    case TransitionType.Add:
                        // remove the piece
                        Position[t.FirstSquare] = (sbyte)Piece.Empty;
                        break;

                    case TransitionType.Remove:
                        // add the piece back
                        Position[t.FirstSquare] = t.Piece;
                        break;

                    case TransitionType.EnPassant:
                        // do nothing, en passant is set from the parent move
                        break;

                    case TransitionType.PawnPromo:
                        // undo a promotion
                        // two parts: 1) remove piece, 2) add back the pawn
                        Position[t.FirstSquare] = (sbyte)((int)ColorlessPiece.Pawn * this.ColorToMove * -1);
                        break;

                    case TransitionType.Castling:
                        if (this.ColorToMove == 1) {
                            this.BlackCastling = (int)t.OldCastling;
                        } else {
                            this.WhiteCastling = (int)t.OldCastling;
                        }
                        break;

                    default:
                        throw new PgnException("Unknown transition type: " + t.Type.ToString());
                }
            }
        }
        #endregion

        #region Generating Transitions
        /// <summary>
        /// Generate all of the transitions for the moves
        /// This is the lengthiest part of the parsing stage
        /// </summary>
        private void GenerateAllTransitions() {
            if (this.Moves[0].Children.Count == 0) {
                throw new PgnException("Empty pgn game provided");
            }

            foreach (ParsingMove rootChild in this.Moves[0].Children) {
                this.GenerateAndExecuteTransitions(rootChild);
            }
        }

        private void GenerateAndExecuteTransitions(ParsingMove move) {
            // start by incrementing the half move count for the move
            move.HalfMoveCount = this.HalfMoveCount + 1;
            move.Transitions = this.GenerateTransitions(move);

            // store the position
            move.Position = new Position();
            move.Position.Squares = new sbyte[128];
            this.Position.CopyTo(move.Position.Squares, 0);

            // move forward
            this.ExecuteMoveForward(move);

            foreach(ParsingMove childMove in move.Children) {
                GenerateAndExecuteTransitions(childMove);
            }

            // move backward
            this.ExecuteMoveBackward();
        }

        private List<ChessTransition> GenerateTransitions(ParsingMove move) {
            var transitions = new List<ChessTransition>();

            // first check castling
            // queenside first because of regex considerations
            if (ChessRegex.CastleQueenside.IsMatch(move.MoveBody)) {
                var rank = (this.ColorToMove == 1) ? 0 : 7;
                var kstart = rank * 16 + 4;
                var rstart = rank * 16;
                var oldCastling = (this.ColorToMove == 1) ? WhiteCastling : BlackCastling;

                transitions.Add(new ChessTransition(TransitionType.Move, kstart, kstart - 2));
                transitions.Add(new ChessTransition(TransitionType.Move, rstart, rstart + 3));
                transitions.Add(new ChessTransition(TransitionType.Castling, (byte)oldCastling, (byte)Castling.None));
                move.HalfMoveCount = 0;
                return transitions;
            }

            // now kingside castling
            if (ChessRegex.CastleKingside.IsMatch(move.MoveBody)) {
                var rank = (this.ColorToMove == 1) ? 0 : 7;
                var kstart = rank * 16 + 4;
                var rstart = rank * 16 + 7;
                var oldCastling = (this.ColorToMove == 1) ? WhiteCastling : BlackCastling;

                transitions.Add(new ChessTransition(TransitionType.Move, kstart, kstart + 2));
                transitions.Add(new ChessTransition(TransitionType.Move, rstart, rstart - 2));
                transitions.Add(new ChessTransition(TransitionType.Castling, (byte)oldCastling, (byte)Castling.None));
                move.HalfMoveCount = 0;
                return transitions;
            }

            // was the move a piece?
            if (ChessRegex.PieceMove.IsMatch(move.MoveBody)) {
                return GeneratePieceMoveTransitions(move);
            } else {
                // since the move wasn't a piece move, it was a pawn move
                return GeneratePawnMoveTransitions(move);
            }
        }
        
        private List<ChessTransition> GeneratePawnMoveTransitions(ParsingMove move) {
            var transitions = new List<ChessTransition>();
            int trank, tfile, ixStartSquare;

            // was it just a simple pawn move?
            if (ChessRegex.PawnMove.IsMatch(move.MoveBody)) {
                var match = ChessRegex.PawnMove.Match(move.MoveBody);
                tfile = match.Groups[1].Value.ToLower()[0] - 'a';
                trank = Int32.Parse(match.Groups[2].Value) - 1; // minus one for zero based index

                // find the start square for the pawn
                // did we move forward one square?
                var minusOneRank = trank - (1 * this.ColorToMove);
                var minusOneStartSquare = minusOneRank * 16 + tfile;

                if (this.Position[minusOneStartSquare] == ((sbyte)ColorlessPiece.Pawn * ColorToMove)) {
                    // yes, we moved forward one square!
                    ixStartSquare = minusOneStartSquare;

                    transitions.Add(new ChessTransition(TransitionType.Move, ixStartSquare, (trank * 16 + tfile)));
                    move.HalfMoveCount = 0;
                    
                } else {
                    // in order to move forward 2, we must start on the 2nd (7th) and move the the 4th (5th)
                    // additionally, 2nd (7th) must be a pawn and 3rd (6th) must be empty
                    if (this.ColorToMove == 1) {
                        // white is moving
                        if (trank == 3 && Position[16 + tfile] == (sbyte)Piece.WhitePawn && Position[32 + tfile] == (sbyte)Piece.Empty) {
                            transitions.Add(new ChessTransition(TransitionType.Move, (16 + tfile), (48 + tfile)));
                            transitions.Add(new ChessTransition(TransitionType.EnPassant, tfile, 0));
                            move.HalfMoveCount = 0;

                            // retrun early because we can't queen on the first move
                            return transitions;
                        }
                    } else {
                        // black is moving
                        if (trank == 4 && Position[96 + tfile] == (sbyte)Piece.BlackPawn && Position[80 + tfile] == (sbyte)Piece.Empty) {
                            transitions.Add(new ChessTransition(TransitionType.Move, (96 + tfile), (64 + tfile)));
                            transitions.Add(new ChessTransition(TransitionType.EnPassant, tfile, 0));
                            move.HalfMoveCount = 0;

                            // retrun early because we can't queen on the first move
                            return transitions;
                        }
                    }

                    // error
                    throw new PgnException("Could not resolve a pawn move, no pawn can execute move: " + move.MoveBody);
                }
            } else if (ChessRegex.PawnCapture.IsMatch(move.MoveBody)) {
                // how about a capture?
                var match = ChessRegex.PawnCapture.Match(move.MoveBody);

                // we know where the pawn was going...
                tfile = match.Groups[2].Value.ToLower()[0] - 'a';
                trank = Int32.Parse(match.Groups[3].Value) - 1;

                // now figure out where the pawn started
                var ffile = match.Groups[1].Value.ToLower()[0] - 'a';
                var minusOneRank = trank - (1 * this.ColorToMove);

                // was it an en passant capture?
                if (this.EnPassant == tfile) {
                    // we are on the correct file, now we need ot check to see if we're moving to the proper square
                    int remSquare = 0;

                    if (this.ColorToMove == 1 && trank == 5) {
                        remSquare = 64 + tfile;
                    }

                    // are we black?
                    if (this.ColorToMove == -1 && trank == 2) {
                        remSquare = 48 + tfile;
                    }

                    if (remSquare != 0) {
                        var remPiece = this.Position[remSquare];
                        // return early because we can't queen on an en passant capture
                        transitions.Add(new ChessTransition(TransitionType.Remove, remSquare, remPiece));
                        transitions.Add(new ChessTransition(TransitionType.Move, minusOneRank * 16 + ffile, trank * 16 + tfile));
                        move.HalfMoveCount = 0;

                        return transitions;
                    }
                }

                // regular capture, remove the piece at the to square and make the move
                ixStartSquare = minusOneRank * 16 + ffile;
                transitions.Add(new ChessTransition(TransitionType.Remove, trank * 16 + tfile, Position[trank * 16 + tfile]));
                transitions.Add(new ChessTransition(TransitionType.Move, ixStartSquare, trank * 16 + tfile));
                move.HalfMoveCount = 0;

            } else {
                // neither a pawn move nor a capture
                throw new PgnException("Unknown move type: " + move.MoveBody);
            }

            // finally, check to see if we queened
            if (ChessRegex.PawnPromoSuffix.IsMatch(move.MoveBody)) {
                var promoPiece = ChessRegex.PawnPromoSuffix.Match(move.MoveBody).Groups[1].Value.GetPiece();
                
                // the ixEndSquare must be on the 8th rank
                if ((ColorToMove == 1 && trank != 7) || (ColorToMove == -1 && trank != 0)) {
                    throw new PgnException("Can only queen on the 8th rank: " + move.MoveBody);
                }

                // we will add a promo transition (internally a remove and then an add)
                transitions.Add(new ChessTransition(TransitionType.PawnPromo, trank * 16 + tfile, (sbyte)promoPiece * ColorToMove));
            }

            return transitions;
        }
        
        private List<ChessTransition> GeneratePieceMoveTransitions(ParsingMove move) {

            var transitions = new List<ChessTransition>();
            var piece = ChessRegex.PieceMove.Match(move.MoveBody).Groups[1].Value;

            // find the destination square
            // the destination will always be at the end because a piece cannot promote
            // this will strip off any nags (we have already removed comments, so only literals left are !, +, #, etc.)
            var dest = ChessRegex.PieceDest.Match(move.MoveBody);
            var trank = Int32.Parse(dest.Groups[3].Value) - 1;
            var tfile = dest.Groups[2].Value.ToLower()[0] - 'a';
            var ixEndSquare = 16 * trank + tfile;
            var ixStartSquare = -1;

            // we now know the piece and the to-square
            var possibleOrigins = this.FindOrigins(piece, ixEndSquare);

            // actually figure out where the piece came from - we will factor in legality and disambiguity
            if (possibleOrigins.Count == 1) {
                // simple case, just one move
                ixStartSquare = possibleOrigins[0];
                transitions.Add(new ChessTransition(TransitionType.Move, ixStartSquare, ixEndSquare));
            } else {
                // there are two ways that multiple moves can be pruned:
                //  1) disambiguation (ex. Rad1, N5c3, Na1c2)
                //  2) legality (ex. Ne2 is pinned so Nc3 can only be Nb1-c3)

                // can we use disambiguation?
                var pieceMoveFull = ChessRegex.PieceMoveFull.Match(move.MoveBody);

                if (pieceMoveFull.Success) {
                    if (pieceMoveFull.Groups[1].Success) {
                        // we have a file given
                        var filePossibles = new List<int>();

                        var sfile = pieceMoveFull.Groups[1].Value.ToLower()[0] - 'a';
                        foreach (int po in possibleOrigins) {
                            if (po % 16 == sfile) {
                                filePossibles.Add(po);
                            }
                        }

                        possibleOrigins = filePossibles;
                    }

                    if (pieceMoveFull.Groups[2].Success) {
                        // we have rank given
                        var rankPossibles = new List<int>();

                        var srank = Int32.Parse(pieceMoveFull.Groups[2].Value) - 1;

                        foreach (int po in possibleOrigins) {
                            if (Math.Floor((float)po / 16) == srank) {
                                rankPossibles.Add(po);
                            }
                        }

                        possibleOrigins = rankPossibles;
                    }
                }

                if (possibleOrigins.Count == 1) {
                    // yay, we can make the move
                    ixStartSquare = possibleOrigins[0];
                    transitions.Add(new ChessTransition(TransitionType.Move, ixStartSquare, ixEndSquare));
                } else {
                    // check to see if one (or more) of the possibilities can be removed due to pins
                    var pinnedPossibles = new List<int>();
                    var ixKing = (this.ColorToMove == 1) ? this.WhiteKingPosition : this.BlackKingPosition;

                    foreach (int po in possibleOrigins) {
                        if (!this.IsPinned(po, ixKing)) {
                            pinnedPossibles.Add(po);
                        }
                    }

                    if (pinnedPossibles.Count == 1) {
                        ixStartSquare = pinnedPossibles[0];
                        transitions.Add(new ChessTransition(TransitionType.Move, ixStartSquare, ixEndSquare));
                    } else {
                        throw new PgnException(string.Format("Multiple possible moves for {0}, invalid pgn", move.MoveBody));
                    }
                }
            }

            if (this.ColorToMove == 1) {
                // white

                // a rook
                if (ixStartSquare == 0 && (((byte)WhiteCastling & 2) > 0)) {
                    transitions.Add(new ChessTransition(
                        TransitionType.Castling,
                        (Castling)WhiteCastling,
                        (WhiteCastling == (int)Castling.Both) ? Castling.Kingside : Castling.None
                    ));
                }

                // h rook
                if (ixStartSquare == 7 && (((byte)WhiteCastling & 1) > 0)) {
                    transitions.Add(new ChessTransition(
                        TransitionType.Castling,
                        (Castling)WhiteCastling,
                        (WhiteCastling == (int)Castling.Both) ? Castling.Queenside : Castling.None
                    ));
                }

                // king
                if (ixStartSquare == 4 && WhiteCastling != (int)Castling.None) {
                    transitions.Add(new ChessTransition(TransitionType.Castling, (Castling)WhiteCastling, Castling.None));
                }

            } else {
                // black

                // a rook
                if (ixStartSquare == 112 && (((byte)BlackCastling & 2) > 0)) {
                    transitions.Add(new ChessTransition(
                        TransitionType.Castling,
                        (Castling)BlackCastling,
                        (BlackCastling == (int)Castling.Both) ? Castling.Kingside : Castling.None
                    ));
                }

                // h rook
                if (ixStartSquare == 119 && (((byte)BlackCastling & 1) > 0)) {
                    transitions.Add(new ChessTransition(
                        TransitionType.Castling,
                        (Castling)BlackCastling,
                        (BlackCastling == (int)Castling.Both) ? Castling.Queenside : Castling.None
                    ));
                }

                // king
                if (ixStartSquare == 116 && BlackCastling != (int)Castling.None) {
                    transitions.Add(new ChessTransition(
                        TransitionType.Castling,
                        (Castling)BlackCastling,
                        Castling.None
                    ));
                }
            }

            // and finally, was it a capture? (hint: remove transition must be first!)
            if (this.Position[ixEndSquare] != (sbyte)Piece.Empty) {
                transitions.Insert(0, new ChessTransition(TransitionType.Remove, ixEndSquare, Position[ixEndSquare]));
                move.HalfMoveCount = 0;
            }

            return transitions;
        }

        private List<int> FindOrigins(string strPiece, int start) {
            ColorlessPiece piece = strPiece.GetPiece();
            if (piece == ColorlessPiece.Undefined) {
                throw new PgnException("Unknown piece: " + strPiece);
            }

            var isSliding = piece.IsSliding();
            var deltas = piece.GetDeltas();

            var startPiece = (sbyte)piece * (sbyte)ColorToMove;
            var possibles = new List<int>();

            int ixDest;
            int delta;

            for (int i = 0; i < deltas.Length; i++) {
                delta = deltas[i];
                ixDest = start + delta;

                // loop while the square is on the baord
                while ((ixDest & 0x88) == 0) {
                    // check if the square is occupied
                    if (Position[ixDest] != (sbyte)ColorlessPiece.Empty) {
                        if (Position[ixDest] == startPiece) {
                            possibles.Add(ixDest);
                        }
                        break;
                    }

                    ixDest += delta;

                    if (!isSliding) {
                        break;
                    }
                }
            }

            return possibles;
        }

        private bool IsPinned(int ixPiece, int ixKing) {
            var toKingDelta = ixPiece - ixKing;
            var absKingDelta = Math.Abs(toKingDelta);
            int? delta = null;

            if (absKingDelta > 0 && absKingDelta < 7) {
                // moving left or right
                delta = 1;
            } else if (absKingDelta % 15 == 0) {
                // moving north west
                delta = 15;
            } else if (absKingDelta % 16 == 0) {
                // moving north
                delta = 16;
            } else if (absKingDelta % 17 == 0) {
                // moving north east
                delta = 17;
            }

            if (delta.HasValue) {
                // we have a potential pinning situation
                var absDelta = delta.Value;

                if (toKingDelta < 0) {
                    delta = delta * -1;
                }

                // start from the king's position + delta
                var ixSquare = ixKing + delta.Value;
                var foundSelf = false;

                // loop while we are still on the board
                while ((ixSquare & 0x88) == 0) {
                    // check if the square is occupied
                    if (this.Position[ixSquare] != 0) {
                        // if we have not found the initial piece, then we don't have a pin
                        if (ixSquare == ixPiece) {
                            foundSelf = true;
                        } else if (!foundSelf) {
                            return false;
                        } else {
                            // we have already found self
                            // is it a pinning piece?
                            // must be of opposite color of colorToMove and a sliding piece in the correct orientation
                            var pinner = this.Position[ixSquare];
                            var pinPiece = (ColorlessPiece)Math.Abs(pinner);
                            var pinColor = (pinner < 0) ? -1 : 1;

                            if ((pinColor * -1) != ColorToMove) {
                                return false;
                            }

                            // now check the delat with the pinPiece
                            return pinPiece.IsSliding() && pinPiece.GetDeltas().Contains(absDelta);
                        }
                    }

                    ixSquare += delta.Value;
                }
            }

            return false;
        }
        #endregion
    }
}
