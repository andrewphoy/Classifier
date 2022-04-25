using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dragon.Chess {
    public static class PgnImporter {

        /// <summary>
        /// Processes a full pgn file that has already been read into memory (or uploaded via copy/paste)
        /// </summary>
        /// <param name="pgn"></param>
        /// <param name="options"></param>
        public static IEnumerable<Game> Import(string pgn, PgnOptions options = null) {
            var games = PreprocessGames(pgn);

            int ix = 0;
            foreach (var g in games) {
                ix++;
                if (g.Headers.ContainsKey("setup") || g.Headers.ContainsKey("fen")) {
                    //TODO
                    continue;
                }

                ParsingGame game;
                try {
                    game = new ParsingGame(g.PgnBody);
                    game.Parse();
                } catch (Exception ex) {
                    try {
                        var parts = new List<string>();

                        parts.Add(ex.Message);
                        parts.Add(g.TryGetHeader<string>("white"));
                        parts.Add(g.TryGetHeader<string>("black"));
                        parts.Add(g.TryGetHeader<string>("event"));
                        parts.Add(g.TryGetHeader<string>("date"));
                        parts.Add("");
                        parts.Add("");

                        System.IO.File.AppendAllLines(@"D:\PgnErrors.log", parts);

                    } catch {
                        // error logging the error, bail out
                    }

                    continue;
                }

                if (game.IsEmpty) {
                    continue;
                }

                yield return CreateGameFromPgn(g, game);
                //yield return game;

            }
        }

        //public static async Task<int> CountLinesAsync(string path) {
        //    int numLines = 0;
        //    StringBuilder sbLine = new StringBuilder();

        //    using (var reader = new StreamReader(path)) {
        //        foreach (string line in reader.ReadLinesSafe()) {
        //            if (!string.IsNullOrWhiteSpace(line)) {
        //                numLines++;
        //            }
        //        }
        //    }
        //    return numLines;
        //}

        public static IEnumerable<PgnGame> ExtractGames(string path) {
            int ixGame = 0;
            int ixLine = 0;

            Regex headerRegex = new Regex(@"\[([\w\d]+)\s+""(.*?)""\]", RegexOptions.Compiled);
            Match headerMatch;

            PgnGame game = new PgnGame();
            StringBuilder sbLine = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            bool lastEmpty = true;
            bool passedHeaders = false;
            bool unloadedGame = false;

            using (var reader = new StreamReader(path)) {
                foreach (string line in reader.ReadLinesSafe()) {
                    ixLine++;
                    if (string.IsNullOrWhiteSpace(line)) {
                        if (lastEmpty) {
                            continue;
                        }

                        // transition from headers to game or from game to next game's headers
                        if (passedHeaders) {
                            // on to the next game
                            game.PgnBody = sb.ToString();
                            yield return game;

                            sb.Length = 0;
                            game = new PgnGame();
                            passedHeaders = false;
                            unloadedGame = false;
                            ixLine = 0;
                            ixGame++;
                        } else {
                            // moving to the body
                            passedHeaders = true;
                        }
                        lastEmpty = true;

                    } else {
                        // line has a value
                        lastEmpty = false;

                        // there will be some data that needs to be loaded later
                        unloadedGame = true;

                        if (!passedHeaders) {
                            // header
                            headerMatch = headerRegex.Match(line);
                            if (headerMatch.Success) {
                                string name = headerMatch.Groups[1].Value;
                                string value = headerMatch.Groups[2].Value;
                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value)) {
                                    try {
                                        game.AddHeader(name, value);
                                    } catch (ArgumentNullException e) {
                                        Console.WriteLine("Error: " + line);
                                        throw new Exception(string.Format("Invalid header in game {0} on line {1}", ixGame, ixLine), e);
                                    }
                                }
                            } else {
                                //skip the line?
                                Console.WriteLine("Error: " + line);
                                continue;
                                throw new FormatException(string.Format("Invalid header in game {0} on line {1}", ixGame, ixLine));
                            }
                        } else {
                            // part of the game body
                            sb.AppendLine(line);
                        }
                    }
                }
            }

            // if we have an unloaded game, add that to the games
            if (unloadedGame && sb.Length > 0) {
                game.PgnBody = sb.ToString();
                yield return game;
                ixGame++;
            }

            //return ixGame;
        }

        private static void TryExtractDate(Game game, PgnGame pgn) {
            if (pgn.Headers.ContainsKey("date")) {
                var parts = pgn.Headers["date"].Split('.');
                if (parts.Length == 3) {
                    if (int.TryParse(parts[0], out int year)) {
                        game.Year = year;
                    }
                    if (int.TryParse(parts[1], out int month)) {
                        game.Month = month;
                    }
                    if (int.TryParse(parts[2], out int day)) {
                        game.Day = day;
                    }

                    if (game.Year.HasValue && game.Month.HasValue && game.Day.HasValue) {
                        try {
                            var dt = new DateTime(game.Year.Value, game.Month.Value, game.Day.Value);
                            game.GameDate = dt;
                        } catch { }
                    }
                }
            }
        }

        private static void TryExtractEventDate(Game game, PgnGame pgn) {
            if (pgn.Headers.ContainsKey("eventdate")) {
                var parts = pgn.Headers["eventdate"].Split('.');
                if (parts.Length == 3) {
                    if (int.TryParse(parts[0], out int year)) {
                        game.EventYear = year;
                    }
                    if (int.TryParse(parts[1], out int month)) {
                        game.EventMonth = month;
                    }
                    if (int.TryParse(parts[2], out int day)) {
                        game.EventDay = day;
                    }

                    if (game.EventYear.HasValue && game.EventMonth.HasValue && game.EventDay.HasValue) {
                        try {
                            var dt = new DateTime(game.EventYear.Value, game.EventMonth.Value, game.EventDay.Value);
                            game.EventDate = dt;
                        } catch { }
                    }
                }
            }
        }

        public static Move CreateMove(ParsingMove parsed) {
            var move = new Move();

            move.ParentMoveId = parsed.ParentMoveId;
            move.MoveId = parsed.MoveId;
            move.MoveBody = parsed.MoveBody;
            //move.UciEncoded = parsed.UciEncoded;
            move.Ply = parsed.Ply;
            move.Fen = parsed.Position.Fen;
            move.EncodedPosition = ChessPositionEncoder.GetByteArrayForPosition(parsed.Position);

            move.Position = parsed.Position;
            move.Position.Encoded = move.EncodedPosition;


            return move;
            /*
                public long GameId { get; set; }
                public long MoveId { get; set; }
                public long PositionId { get; set; }

                public string Fen { get; set; }

                public int Clock { get; set; }

                public string MoveBody { get; set; }
                public string Uci { get; set; }
                public string San { get; set; }

                public int ParentMoveId { get; set; }
                public int Ply { get; set; }
                public int HalfMoveCount { get; set; }
                public string NagString { get; set; }
                public string Comment { get; set; }

                public IEnumerable<Move> Children { get; set; }


                public int Start { get; internal set; }
                public int End { get; internal set; }
                public List<Transition> Transitions { get; internal set; }

                public bool IsPawnMove { get; set; }
                public bool IsCapture { get; set; }

                public string FileSAN { get; set; }
                public string RankSAN { get; set; }
                public string FullSAN { get; set; }
                public string CorrectSAN { get; set; }
        */
        }

        public static Game CreateGameFromPgn(PgnGame pgn, ParsingGame parsed) {
            var game = new Game();


            //foreach (var kvp in parsed.Moves) {
            //    if (kvp.Key > 0) {
            //        game.Moves[kvp.Key] = CreateMove(kvp.Value);
            //    }
            //}

            game.Result = pgn.TryGetHeader<string>("Result");

            if (parsed.Result == "1-0" || parsed.Result == "0-1" || parsed.Result == "*") {
                game.Result = parsed.Result;
            } else if (parsed.Result.StartsWith("1/2")) {
                game.Result = "1/2";
            } else {
                if (game.Result != null) {
                    var a = 0;
                }
                game.Result = null;
            }

            game.WhitePlayerName = pgn.TryGetHeader<string>("White");
            if (!string.IsNullOrEmpty(game.WhitePlayerName)) {
                if (game.WhitePlayerName.EndsWith("(wh)")) {
                    game.WhitePlayerName = game.WhitePlayerName.Substring(0, game.WhitePlayerName.Length - 4).Trim();
                }
            }

            game.BlackPlayerName = pgn.TryGetHeader<string>("Black");
            if (!string.IsNullOrEmpty(game.BlackPlayerName)) {
                if (game.BlackPlayerName.EndsWith("(bl)")) {
                    game.BlackPlayerName = game.BlackPlayerName.Substring(0, game.BlackPlayerName.Length - 4).Trim();
                }
            }

            game.WhiteRating = pgn.TryGetHeader<int?>("WhiteElo");
            if (game.WhiteRating < 100 || game.WhiteRating > 4200) {
                game.WhiteRating = null;
            }

            game.BlackRating = pgn.TryGetHeader<int?>("BlackElo");
            if (game.BlackRating < 100 || game.BlackRating > 4200) {
                game.BlackRating = null;
            }

            game.WhiteTitle = pgn.TryGetHeader<string>("WhiteTitle");
            game.BlackTitle = pgn.TryGetHeader<string>("BlackTitle");

            game.WhiteFideId = pgn.TryGetHeader<int?>("WhiteFideId");
            game.BlackFideId = pgn.TryGetHeader<int?>("BlackFideId");


            game.EventName = pgn.TryGetHeader<string>("Event");
            game.SiteName = pgn.TryGetHeader<string>("Site");

            if (pgn.Headers.ContainsKey("round")) {
                game.Round = pgn.TryGetHeader<int?>("Round");

                string roundVal = pgn.TryGetHeader<string>("Round");
                if (!string.IsNullOrWhiteSpace(roundVal)) {
                    if (roundVal.Contains(".")) {
                        var parts = roundVal.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2) {
                            try {
                                game.Round = int.Parse(parts[0]);
                                game.BoardNumber = int.Parse(parts[1]);
                            } catch { }
                        }
                    }
                }
            }

            TryExtractDate(game, pgn);
            TryExtractEventDate(game, pgn);

            return game;
        }

        private static IEnumerable<PgnGame> PreprocessGames(string pgn) {
            if (string.IsNullOrWhiteSpace(pgn)) {
                yield break;
            }

            string[] lines = pgn.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            Regex headerRegex = new Regex(@"\[([\w\d]+)\s+""(.*?)""\]", RegexOptions.Compiled);
            Match headerMatch;

            int ixGame = 1;
            int ixLine = 0;
            PgnGame game = new PgnGame();
            string body = "";
            bool passedHeaders = false;
            bool unloadedGame = false;

            foreach (string line in lines) {
                ixLine++;
                if (string.IsNullOrWhiteSpace(line)) {
                    // transition from headers to game or from game to next game's headers
                    if (passedHeaders) {
                        // on to the next game
                        game.PgnBody = body;
                        yield return game;

                        game = new PgnGame();
                        body = "";
                        passedHeaders = false;
                        unloadedGame = false;
                        ixLine = 0;
                        ixGame++;
                    } else {
                        // moving to the body
                        passedHeaders = true;
                    }
                } else {
                    // line has a value

                    // there will be some data that needs to be loaded later
                    unloadedGame = true;

                    if (!passedHeaders) {
                        // header
                        headerMatch = headerRegex.Match(line);
                        if (headerMatch.Success) {
                            string name = headerMatch.Groups[1].Value;
                            string value = headerMatch.Groups[2].Value;
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value)) {
                                try {
                                    game.AddHeader(name, value);
                                } catch (ArgumentNullException e) {
                                    throw new Exception(string.Format("Invalid header in game {0} on line {1}", ixGame, ixLine), e);
                                }
                            }
                        } else {
                            throw new FormatException(string.Format("Invalid header in game {0} on line {1}", ixGame, ixLine));
                        }
                    } else {
                        // part of the game body
                        body += line + "\n";
                    }
                }
            }

            // if we have an unloaded game, add that to the games
            if (unloadedGame && !string.IsNullOrWhiteSpace(body)) {
                game.PgnBody = body;
                yield return game;
            }
        }
    }
}
