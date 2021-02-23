using Classifier.Models;
using Dragon.Chess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Internal;
using System.Text;
using System.Threading.Tasks;

namespace Classifier.Helpers {
    public class TacticFinder {


        private readonly ObjectPool<UciEngine> _pool;

        public TacticFinder(string uciPath) {
            _pool = new ObjectPool<UciEngine>(() => {
                var engine = new UciEngine(uciPath);
                engine.Start().Wait();
                return engine;
            });

            Tactics = new ConcurrentBag<Tactic>();
        }

        private ConcurrentBag<Tactic> Tactics { get; set; }
        public async Task<List<Tactic>> FindTactics(IEnumerable<PgnGame> games) {
            var opts = new Jil.Options(prettyPrint: true);

            foreach (var g in games) {
                var tactics = await ProcessSingleGame(g);
                if (tactics != null && tactics.Count > 0) {
                    string json = Jil.JSON.Serialize(tactics, opts);
                    await System.IO.File.AppendAllTextAsync(@"D:\OtherCode\Classifier\tactics.json", json + "\n");
                }
            }

            return Tactics.ToList();
        }

        private async Task<List<Tactic>> ProcessSingleGame(PgnGame pgn) {
            if (pgn.PgnBody.Length < 10) {
                return null;
            }

            var result = new List<Tactic>();
            var engine = _pool.Allocate();

            try {
                var game = new ParsingGame(pgn.PgnBody);
                game.Parse();

                if (game.Moves == null || game.Moves.Count < 2) {
                    return null;
                }

                await engine.UciNewGame();
                await engine.SetOption("Threads", "8");
                await engine.SetOption("MultiPV", "1");

                var root = game.Moves[0];
                ParsingMove curr = root.Children[0];
                ParsingMove next;
                EngineEval lastEval = null;
                string lastFen = "";

                while (curr.Children.Count > 0) {
                    next = curr.Children[0];

                    string fen = GetFen(curr.Position, game);

                    await engine.SetFenPosition(fen);
                    var currEval = await engine.Analyze(nodes: 3500000);
                    currEval.ColorToMove = game.ColorToMove;


                    Console.WriteLine(currEval.EvalString + "\t" + fen + "\t" + curr.MoveBody);

                    if (lastEval != null) {
                        // current eval must be at least two pawns to the good, and last one must be less than 1.1 pawns to the good
                        if (lastEval.Cp > -110 && lastEval.Cp < 850 && currEval.Cp > 200 && currEval.Cp < 850) {
                            var tactic = new Tactic {
                                Fen = fen,
                                BestMove = currEval.BestMove,
                                ColorToMove = game.ColorToMove,
                                GameMove = curr.ToString(),
                                ScoreCp = currEval.Cp,
                                ScoreMate = currEval.Mate,
                                LastMove = game.Moves[curr.ParentMoveId].ToString(),
                                LastPositionFen = lastFen,
                                PgnGame = pgn
                            };
                            result.Add(tactic);
                        }
                    }

                    game.ExecuteMoveForward(curr);
                    curr = next;
                    lastEval = currEval;
                    lastFen = fen;
                }
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            } finally {
                _pool.Free(engine);
            }
            return result;
        }

        private string GetFen(Dragon.Models.Position position, ParsingGame game) {
            string fen = position.GetFen();
            fen += " " + (game.ColorToMove > 0 ? "w" : "b");

            string castling = "";
            if (game.WhiteCastling == (int)Castling.Both) {
                castling = "KQ";
            } else if (game.WhiteCastling == (int)Castling.Kingside) {
                castling = "K";
            } else if (game.WhiteCastling == (int)Castling.Queenside) {
                castling = "Q";
            }
            if (game.BlackCastling == (int)Castling.Both) {
                castling += "kq";
            } else if (game.BlackCastling == (int)Castling.Kingside) {
                castling += "k";
            } else if (game.BlackCastling == (int)Castling.Queenside) {
                castling += "q";
            }

            if (string.IsNullOrEmpty(castling)) {
                castling = "-";
            }

            fen += " " + castling;

            //    // 4) halfmove clock
            //    // 5) fullmove number

            if (game.EnPassant.HasValue) {
                fen += " " + "abcdefgh"[game.EnPassant.Value] + (game.ColorToMove > 0 ? "6" : "3");
            } else {
                fen += " -";
            }

            fen += " " + game.HalfMoveCount.ToString();
            int moveNumber = (int)Math.Ceiling(game.CurrentPly / 2m);
            fen += " " + moveNumber.ToString();

            return fen;
        }

    }
}
