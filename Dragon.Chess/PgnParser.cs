//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Dragon.Chess.PgnTokens;

//namespace Dragon.Chess {
//    public static class PgnParser {

//        internal static Regex HeaderRegex = new Regex(@"\s*\[([\w\d]+)\s+""(.*?)""\]", RegexOptions.Compiled);

//        private class Game {

//        }

//        public static void LoadFromFileAsync(string path) {
//            using (var reader = new StreamReader(path)) {
//                foreach (var token in LexStream(reader)) {

//                }
//            }
//        }

//        private static IEnumerable<PgnToken> LexStream(StreamReader reader) {

//            Game game = new Game();

//            string line;
//            while ((line = reader.ReadLine()) != null) {
//                line = line.Trim();
//                if (string.IsNullOrEmpty(line)) {
//                    yield return PgnToken.EmptyLine;
//                    continue;
//                }
//                var match = HeaderRegex.Match(line);
//                if (match.Success) {
//                    yield return new HeaderToken(match.Groups[1].Value, match.Groups[2].Value);
//                    continue;
//                }

//                // not empty and not a header, must be some moves
//                if (line.StartsWith("%")) {
//                    // special escaping
//                    Console.WriteLine("Found % escaping");
//                    continue;
//                }

                
//                for (int ix = 0; ix < line.Length; ix++) {
//                    if (char.IsDigit(line[ix])) {
//                        string strMoveNumber = "";
//                        while (char.IsDigit(line[ix])) {
//                            strMoveNumber += line[ix];
//                            ix++;
//                        }

//                    }
//                }
//            }


//        }
//    }
//}
