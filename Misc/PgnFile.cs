using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Dragon.Chess {
    public class PgnFile {

        private string _path;

        public PgnFile(string path) {
            _path = path;
        }

        public string OriginalName { get; set; }
        public Guid PgnSource { get; set; }
        public List<PgnGame> Games { get; set; }

        bool _processed = false;
        public void PreProcess() {
            if (_processed) {
                return;
            }
            // read the file and figure out which games are included
            List<PgnGame> games = new List<PgnGame>();
            List<string> headers;
            string body, line;
            bool passedHeaders = false;
            bool unloadedGame = false;
            Regex headerRegex = new Regex(@"\[([\w\d]+)\s+""(.*?)""\]", RegexOptions.Compiled);
            Match headerMatch;

            PgnGame game = new PgnGame();
            bool lastEmpty = true;

            using (var sr = new StreamReader(_path)) {
                headers = new List<string>();
                body = "";
                while(!sr.EndOfStream) {
                    line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) {
                        if (lastEmpty) {
                            continue;
                        }
                        if (passedHeaders) {
                            // on to the next game
                            game.RawBody = body;
                            games.Add(game);

                            game = new PgnGame();
                            body = "";
                            passedHeaders = false;
                            unloadedGame = false;

                        } else {
                            // moving to the body
                            passedHeaders = true;
                        }
                        lastEmpty = true;
                    } else {
                        lastEmpty = false;

                        // there will be some data that needs to be loaded later
                        unloadedGame = true;

                        if (!passedHeaders) {
                            // header
                            game.RawHeaders.Add(line);
                            headerMatch = headerRegex.Match(line);
                            if (headerMatch.Success) {
                                string name = headerMatch.Groups[1].Value;
                                string value = headerMatch.Groups[2].Value;
                                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value)) {
                                    game.AddHeader(name, value);
                                } else {
                                    throw new PgnException(string.Format("Invalid header in game"));
                                }
                            }

                        } else {
                            // part of the game body
                            body += line + Environment.NewLine;
                        }
                    }
                }
            }

            if (unloadedGame) {
                game.RawBody = body;
                games.Add(game);
            }

            this.Games = games;
            _processed = true;
        }

        public void Upload() {
            this.PreProcess();

            List<Tuple<PgnGame, ParsingGame>> parsedGames = new List<Tuple<PgnGame, ParsingGame>>();

            foreach (PgnGame game in Games) {
                try {
                    var parseGame = new ParsingGame(game.RawBody);
                    parseGame.Parse();
                    parsedGames.Add(Tuple.Create(game, parseGame));
                } catch (PgnException pe) {
                    game.Errors.Add(pe.Message);
                    game.HasErrors = true;
                }
            }

            foreach (Tuple<PgnGame, ParsingGame> tup in parsedGames) {
                // insert into the database
                PgnImporter.InsertGame(tup.Item1, tup.Item2, this.PgnSource);
            }
        }
    }
}
