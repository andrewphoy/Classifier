using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dragon.Chess;
using Classifier.Helpers;
using Classifier.Models;
using System.Data.SqlClient;
using Classifier.Chess;

namespace Classifier {
    class Program {
        static void Main(string[] args) {
            var options = Classifier.Helpers.OptionAttribute.Parse<ClassifierOptions>(args);

            CalculateTactics();
            //string path = @"C:\SsdCode\inprocsave.txt";
            //string json = File.ReadAllText(path);

            //List<Tactic> tactics = Jil.JSON.Deserialize<List<Tactic>>(json);
            //var a = 0;

            //var random = new Random();
            //var randomOrder = tactics.OrderBy(x => random.Next());

            //StringBuilder sbPgn = new StringBuilder();
            //foreach (var t in randomOrder) {
            //    AddHeader(sbPgn, t.PgnGame, "Event");
            //    AddHeader(sbPgn, t.PgnGame, "Site");
            //    AddHeader(sbPgn, t.PgnGame, "Date");
            //    AddHeader(sbPgn, t.PgnGame, "Round");
            //    AddHeader(sbPgn, t.PgnGame, "White");
            //    AddHeader(sbPgn, t.PgnGame, "Black");
            //    AddHeader(sbPgn, t.PgnGame, "Result");
            //    AddHeader(sbPgn, t.PgnGame, "ECO");
            //    sbPgn.AppendLine("[SetUp \"1\"]");
            //    sbPgn.AppendLine($"[FEN \"{t.LastPositionFen}\"]");
            //    sbPgn.AppendLine();
            //    sbPgn.AppendLine(t.LastMove);
            //    sbPgn.AppendLine();
            //}

            //string pgn = sbPgn.ToString();
            //var b = 0;
        }

        private static void AddHeader(StringBuilder sb, PgnGame game, string header) {
            if (game != null && game.Headers != null && game.Headers.ContainsKey(header)) {
                var val = game.Headers[header];
                sb.AppendLine($"[{header} \"{val}\"]");
            }
        }

        private static void PainfullyFindGoodOnes() {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var path = @"D:\OtherCode\Classifier\tactics.json";
            var sb = new StringBuilder();

            List<Tactic> tactics = new List<Tactic>();
            foreach (string line in File.ReadLines(path)) {
                sb.AppendLine(line);
                if (line == "}]") {
                    string json = sb.ToString();
                    sb.Length = 0;

                    List<Tactic> t = Jil.JSON.Deserialize<List<Tactic>>(json);
                    tactics.AddRange(t);
                }
            }

            List<Tactic> missed = new List<Tactic>();
            foreach (var t in tactics) {
                var position = FenEncoder.GetArrayForFen(t.Fen);
                if (!MoveComparer.IsEngineEqual(position, t.GameMove, t.BestMove)) {
                    missed.Add(t);
                }
            }


            // first pass at checking for ambiguity
            string ucipath = @"D:\OtherCode\Classifier\Resources\stockfish-11-win\Windows\stockfish_20011801_x64_modern.exe";
            var engine = new UciEngine(ucipath);
            engine.Start().Wait();
            engine.SetOption("Threads", "8").Wait();
            engine.SetOption("MultiPV", "2").Wait();

            var yay = new List<Tactic>();
            foreach (var t in missed) {
                engine.UciNewGame().Wait();
                engine.SetFenPosition(t.Fen).Wait();
                var result = engine.Analyze(7000000).Result;

                int best = Math.Abs(result.Variations[1].ScoreCp.Value);
                int second = Math.Abs(result.Variations[2].ScoreCp.Value);

                if (second < 100) {
                    yay.Add(t);
                } else if ((best - second) > 200) {
                    yay.Add(t);
                } else if (best < 300 && (best - second) > 100) {
                    yay.Add(t);
                } else {
                    Console.WriteLine(best + "\t" + second);
                }
            }


            Console.WriteLine("Missed " + missed.Count);

            //var random = new Random();
            //var randomOrder = tactics.OrderBy(x => random.Next());

            //StringBuilder sbPgn = new StringBuilder();
            //foreach (var t in randomOrder) {
            //    AddHeader(sbPgn, t.PgnGame, "Event");
            //    AddHeader(sbPgn, t.PgnGame, "Site");
            //    AddHeader(sbPgn, t.PgnGame, "Date");
            //    AddHeader(sbPgn, t.PgnGame, "Round");
            //    AddHeader(sbPgn, t.PgnGame, "White");
            //    AddHeader(sbPgn, t.PgnGame, "Black");
            //    AddHeader(sbPgn, t.PgnGame, "Result");
            //    AddHeader(sbPgn, t.PgnGame, "ECO");
            //    sbPgn.AppendLine("[SetUp \"1\"]");
            //    sbPgn.AppendLine($"[FEN \"{t.LastPositionFen}\"]");
            //    sbPgn.AppendLine();
            //    sbPgn.AppendLine(t.LastMove);
            //    sbPgn.AppendLine();
            //}

            //string pgn = sbPgn.ToString();

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            var b = 0;


            //var random = new Random();
            //var randomOrder = tactics.OrderBy(x => random.Next());

            //StringBuilder sbPgn = new StringBuilder();
            //foreach (var t in randomOrder) {
            //    AddHeader(sbPgn, t.PgnGame, "Event");
            //    AddHeader(sbPgn, t.PgnGame, "Site");
            //    AddHeader(sbPgn, t.PgnGame, "Date");
            //    AddHeader(sbPgn, t.PgnGame, "Round");
            //    AddHeader(sbPgn, t.PgnGame, "White");
            //    AddHeader(sbPgn, t.PgnGame, "Black");
            //    AddHeader(sbPgn, t.PgnGame, "Result");
            //    AddHeader(sbPgn, t.PgnGame, "ECO");
            //    sbPgn.AppendLine("[SetUp \"1\"]");
            //    sbPgn.AppendLine($"[FEN \"{t.LastPositionFen}\"]");
            //    sbPgn.AppendLine();
            //    sbPgn.AppendLine(t.LastMove);
            //    sbPgn.AppendLine();
            //}

            //string pgn = sbPgn.ToString();
            //var l = 0;
        }

        private static void CalculateTactics() {
            string ucipath = @"D:\OtherCode\Classifier\Resources\stockfish-11-win\Windows\stockfish_20011801_x64_modern.exe";
            string path = @"D:\OtherCode\Classifier\Resources\Andrew.pgn";
            var games = PgnImporter.ImportFromPath(path);

            var remaining = games.Skip(460).Take(1);
            var finder = new TacticFinder(ucipath);
            var tactics = finder.FindTactics(remaining).Result;
        }

        private static void ExtractNames() {
            string path = @"D:\OtherCode\Classifier\Resources\Andrew.pgn";
            var games = PgnImporter.ImportFromPath(path);

            Dictionary<string, int> names = new Dictionary<string, int>();
            foreach (var g in games) {
                if (g.Headers.ContainsKey("White")) {
                    string white = g.Headers["White"].ToLower();
                    if (names.ContainsKey(white)) {
                        names[white]++;
                    } else {
                        names[white] = 1;
                    }
                }
                if (g.Headers.ContainsKey("Black")) {
                    string black = g.Headers["Black"].ToLower();
                    if (names.ContainsKey(black)) {
                        names[black]++;
                    } else {
                        names[black] = 1;
                    }
                }
            }
        }

        private static void TestEcoStuff() {
            string path = @"D:\OtherCode\Classifier\Resources\eco.txt";

            StringBuilder sb = new StringBuilder();
            List<EcoVariation> ecos = new List<EcoVariation>();

            foreach (string line in File.ReadLines(path)) {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";")) {
                    continue;
                }
                sb.AppendLine(line);
            }

            var a = 0;
        }

        private class EcoVariation {
            public string Eco { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Variation { get; set; }
        }


        private static void LoadAndTestPgn() {

            //Console.WriteLine(Environment.ProcessorCount);

            string path = @"D:\OtherCode\Classifier\Resources\Andrew.pgn";
            var games = PgnImporter.ImportFromPath(path);

            List<ParsingGame> parsedGames = new List<ParsingGame>();

            foreach (var g in games) {
                if (g.PgnBody.Length > 10) {
                    try {
                        var parsed = new ParsingGame(g.PgnBody);
                        parsedGames.Add(parsed);
                    } catch (Exception ex) {
                        Console.WriteLine("Error parsing game: " + ex.Message);
                    }
                }
            }

            var testGame = parsedGames.Last();

            string ucipath = @"D:\OtherCode\Classifier\Resources\stockfish-11-win\Windows\stockfish_20011801_x64_modern.exe";
            var engine = new UciEngine(ucipath);
            engine.Start().Wait();

            //string fen = "6rk/3b3p/1pnp1r2/p1n1ppN1/P6N/2P4P/1PB2PP1/3RR1K1 w - - 4 26";
            //TestEval(engine, fen).Wait();

            List<Tactic> tactics = ProcessSingleGame(engine, testGame).Result;


            var a = 0;
        }



        public async static Task TestEval(UciEngine engine, string fen) {
            await engine.UciNewGame();
            await engine.SetOption("MultiPV", "1");
            await engine.SetFenPosition(fen);
            var data = await engine.Analyze(nodes: 3500000);
            Console.WriteLine(data.EvalString + "\t" + data.Variations[1].Variation);
        }

        public async static Task<List<Tactic>> ProcessSingleGame(UciEngine engine, ParsingGame game) {
            game.Parse();

            if (game.Moves == null || game.Moves.Count < 2) {
                return null;
            }

            var result = new List<Tactic>();

            await engine.UciNewGame();
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
                            LastPositionFen = lastFen
                        };
                        result.Add(tactic);
                    }
                }

                game.ExecuteMoveForward(curr);
                curr = next;
                lastEval = currEval;
                lastFen = fen;
            }

            return result;
            /*
0.88    rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1        e4
-0.04   rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1     c5
0.7     rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2   Nf3
-0.12   rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2  d6
0.69    rnbqkbnr/pp2pppp/3p4/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 0 3 d4
-0.18   rnbqkbnr/pp2pppp/3p4/2p5/3PP3/5N2/PPP2PPP/RNBQKB1R b KQkq d3 0 3        cxd4
0.66    rnbqkbnr/pp2pppp/3p4/8/3pP3/5N2/PPP2PPP/RNBQKB1R w KQkq - 0 4   Nxd4
-0.11   rnbqkbnr/pp2pppp/3p4/8/3NP3/8/PPP2PPP/RNBQKB1R b KQkq - 0 4     Nf6
0.68    rnbqkb1r/pp2pppp/3p1n2/8/3NP3/8/PPP2PPP/RNBQKB1R w KQkq - 1 5   Nc3
0.02    rnbqkb1r/pp2pppp/3p1n2/8/3NP3/2N5/PPP2PPP/R1BQKB1R b KQkq - 2 5 Nc6
0.42    r1bqkb1r/pp2pppp/2np1n2/8/3NP3/2N5/PPP2PPP/R1BQKB1R w KQkq - 3 6        Bc4
-0.29   r1bqkb1r/pp2pppp/2np1n2/8/2BNP3/2N5/PPP2PPP/R1BQK2R b KQkq - 4 6        e6
0.48    r1bqkb1r/pp3ppp/2nppn2/8/2BNP3/2N5/PPP2PPP/R1BQK2R w KQkq - 0 7 O-O
-0.03   r1bqkb1r/pp3ppp/2nppn2/8/2BNP3/2N5/PPP2PPP/R1BQ1RK1 b kq - 0 7  Be7
0.67    r1bqk2r/pp2bppp/2nppn2/8/2BNP3/2N5/PPP2PPP/R1BQ1RK1 w kq - 1 8  Bb3
-0.09   r1bqk2r/pp2bppp/2nppn2/8/3NP3/1BN5/PPP2PPP/R1BQ1RK1 b kq - 2 8  O-O
0.76    r1bq1rk1/pp2bppp/2nppn2/8/3NP3/1BN5/PPP2PPP/R1BQ1RK1 w - - 0 9  f4
-0.45   r1bq1rk1/pp2bppp/2nppn2/8/3NPP2/1BN5/PPP3PP/R1BQ1RK1 b - f3 0 9 a6
1.07    r1bq1rk1/1p2bppp/p1nppn2/8/3NPP2/1BN5/PPP3PP/R1BQ1RK1 w - - 0 10        f5
-3.48   r1bq1rk1/1p2bppp/p1nppn2/5P2/3NP3/1BN5/PPP3PP/R1BQ1RK1 b - - 0 10       e5
mark
0.84    r1bq1rk1/1p2bppp/p1np1n2/4pP2/3NP3/1BN5/PPP3PP/R1BQ1RK1 w - - 0 11      Nxc6
0.12    r1bq1rk1/1p2bppp/p1Np1n2/4pP2/4P3/1BN5/PPP3PP/R1BQ1RK1 b - - 0 11       bxc6
0.72    r1bq1rk1/4bppp/p1pp1n2/4pP2/4P3/1BN5/PPP3PP/R1BQ1RK1 w - - 0 12 g4
-0.22   r1bq1rk1/4bppp/p1pp1n2/4pP2/4P1P1/1BN5/PPP4P/R1BQ1RK1 b - g3 0 12       Qb6+
2.74    r1b2rk1/4bppp/pqpp1n2/4pP2/4P1P1/1BN5/PPP4P/R1BQ1RK1 w - - 1 13 Kh1
1.82    r1b2rk1/4bppp/pqpp1n2/4pP2/4P1P1/1BN5/PPP4P/R1BQ1R1K b - - 2 13 d5
3.15    r1b2rk1/4bppp/pqp2n2/3ppP2/4P1P1/1BN5/PPP4P/R1BQ1R1K w - - 0 14 g5
2.6     r1b2rk1/4bppp/pqp2n2/3ppPP1/4P3/1BN5/PPP4P/R1BQ1R1K b - - 0 14  Nxe4
3.56    r1b2rk1/4bppp/pqp5/3ppPP1/4n3/1BN5/PPP4P/R1BQ1R1K w - - 0 15    Nxe4
3.27    r1b2rk1/4bppp/pqp5/3ppPP1/4N3/1B6/PPP4P/R1BQ1R1K b - - 0 15     dxe4
5.34    r1b2rk1/4bppp/pqp5/4pPP1/4p3/1B6/PPP4P/R1BQ1R1K w - - 0 16      g6
1.57    r1b2rk1/4bppp/pqp3P1/4pP2/4p3/1B6/PPP4P/R1BQ1R1K b - - 0 16     Bxf5
7.67    r4rk1/4bppp/pqp3P1/4pb2/4p3/1B6/PPP4P/R1BQ1R1K w - - 0 17       Bxf7+
7.7     r4rk1/4bBpp/pqp3P1/4pb2/4p3/8/PPP4P/R1BQ1R1K b - - 0 17 Kh8
9.89    r4r1k/4bBpp/pqp3P1/4pb2/4p3/8/PPP4P/R1BQ1R1K w - - 1 18 Rxf5
9.64    r4r1k/4bBpp/pqp3P1/4pR2/4p3/8/PPP4P/R1BQ3K b - - 0 18   h6
10.15   r4r1k/4bBp1/pqp3Pp/4pR2/4p3/8/PPP4P/R1BQ3K w - - 0 19   Qh5
9.77    r4r1k/4bBp1/pqp3Pp/4pR1Q/4p3/8/PPP4P/R1B4K b - - 1 19   e3
10.31   r4r1k/4bBp1/pqp3Pp/4pR1Q/8/4p3/PPP4P/R1B4K w - - 0 20   Rxe5
10.38   r4r1k/4bBp1/pqp3Pp/4R2Q/8/4p3/PPP4P/R1B4K b - - 0 20    Bf6
14.59   r4r1k/5Bp1/pqp2bPp/4R2Q/8/4p3/PPP4P/R1B4K w - - 1 21    Bxe3
15.78   r4r1k/5Bp1/pqp2bPp/4R2Q/8/4B3/PPP4P/R6K b - - 0 21      Qxb2
#4      r4r1k/5Bp1/p1p2bPp/4R2Q/8/4B3/PqP4P/R6K w - - 0 22      Bxh6
#3      r4r1k/5Bp1/p1p2bPB/4R2Q/8/8/PqP4P/R6K b - - 0 22        Rxf7
#3      r6k/5rp1/p1p2bPB/4R2Q/8/8/PqP4P/R6K w - - 0 23  Bg5+
#2      r6k/5rp1/p1p2bP1/4R1BQ/8/8/PqP4P/R6K b - - 1 23 Kg8
#2      r5k1/5rp1/p1p2bP1/4R1BQ/8/8/PqP4P/R6K w - - 2 24        Qh7+
#1      r5k1/5rpQ/p1p2bP1/4R1B1/8/8/PqP4P/R6K b - - 3 24        Kf8
             */

        }


        private static string GetFen(Dragon.Models.Position position, ParsingGame game) {
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

    public static class Extensions {
        public static IEnumerable<PgnGame> Filter(this IEnumerable<PgnGame> games) {
            foreach (var g in games) {
                if (g.PgnBody.Length > 10) {
                    yield return g;
                }
            }
        }
    }
}
