//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Text.RegularExpressions;
//using Classifier.Models;

//namespace Classifier.Helpers {
//    public static class PgnImporter {

//        public static IEnumerable<PgnGame> Import(string pgn) => 
//            ImportImpl(pgn.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

//        public static IEnumerable<PgnGame> ImportFromPath(string path) => 
//            ImportImpl(File.ReadLines(path));

//        private static IEnumerable<PgnGame> ImportImpl(IEnumerable<string> lines) {
//            PgnGame game = new PgnGame();
//            string body = "";
//            bool passedHeaders = false;
//            bool unloadedGame = false;
//            bool lastEmpty = true;

//            Regex headerRegex = new Regex(@"\[([\w\d]+)\s+""(.*?)""\]", RegexOptions.Compiled);
//            Match headerMatch;

//            foreach (string line in lines) {
//                if (string.IsNullOrWhiteSpace(line)) {
//                    if (lastEmpty) {
//                        continue;
//                    }
//                    if (passedHeaders) {
//                        // on to the next game
//                        game.PgnBody = body;
//                        yield return game;

//                        game = new PgnGame();
//                        body = "";
//                        passedHeaders = false;
//                        unloadedGame = false;
//                    } else {
//                        // moving to the body
//                        passedHeaders = true;
//                    }
//                    lastEmpty = true;

//                } else {
//                    // line has a value
//                    lastEmpty = false;

//                    if (!passedHeaders) {
//                        // header
//                        game.RawHeaders.Add(line);
//                        headerMatch = headerRegex.Match(line);

//                        if (headerMatch.Success) {
//                            string name = headerMatch.Groups[1].Value;
//                            string value = headerMatch.Groups[2].Value;

//                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value)) {
//                                game.Headers[name] = value;
//                            } else {
//                                game.Errors.Add("Malformed header: " + line);
//                                game.HasErrors = true;
//                            }
//                        }
//                    } else {
//                        // part of the body
//                        body += line + Environment.NewLine;
//                    }
//                }
//            }

//            if (unloadedGame) {
//                game.PgnBody = body;
//                yield return game;
//            }

//        }
//    }
//}
