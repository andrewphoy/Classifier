using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dragon.Chess;

namespace Classifier.Chess {
    public static class FenEncoder {

        /// <summary>
        /// Generates an 0x88 array for a specified FEN
        /// </summary>
        /// <param name="fen"></param>
        /// <returns></returns>
        public static sbyte[] GetArrayForFen(string fen) {
            if (!ValidateFen(fen)) {
                throw new ArgumentException("Could not parse FEN: " + fen, nameof(fen));
            }

            // ["5rk1/p4p2/1p2r2p/2p2Rp1/3b2P1/1P1P3P/P1PR3B/7K", "b", "-", "-", "0", "1"] 
            // 0) position
            // 1) colorToMove
            // 2) castling avail
            // 3) en passant file
            // 4) halfmove clock
            // 5) fullmove number

            var arr = new sbyte[128];

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
                            arr[offset + i] = (sbyte)(color * (int)piece);

                            //// is it a king?
                            //if (piece == ColorlessPiece.King) {
                            //    if (color == 1) {
                            //        this.WhiteKingPosition = offset + i;
                            //    } else {
                            //        this.BlackKingPosition = offset + i;
                            //    }
                            //}

                        } else {
                            // is an empty square
                            i += (int.Parse(charNext.ToString()) - 1);
                        }
                    }
                    ixChar++;
                }
                offset -= 16;
            }

            return arr;
        }


        public static bool ValidateFen(string fen) {
            var fenRegex = new Regex(@"\s*([rnbqkpRNBQKP1-8]+\/){7}([rnbqkpRNBQKP1-8]+)\s[bw-]\s(([a-hkqA-HKQ]{1,4})|(-))\s(([a-h][36])|(-))\s\d+\s\d+\s*");
            return fenRegex.IsMatch(fen);
        }
    }
}
