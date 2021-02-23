using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dragon.Chess;

namespace Classifier.Chess {
    public static class MoveComparer {

        public static bool IsEngineEqual(sbyte[] position, string san, string engine) {
            if (position == null) {
                throw new ArgumentNullException(nameof(position));
            }
            if (string.IsNullOrWhiteSpace(san)) {
                throw new ArgumentNullException(nameof(san));
            }
            if (string.IsNullOrWhiteSpace(engine)) {
                throw new ArgumentNullException(nameof(engine));
            }

            if (string.Equals(san, engine)) {
                return true;
            }

            // engine move is of the form e2e4
            // san is of the form 1.e4 or e4
            if (san.IndexOf('.') >= 0) {
                int ixLastDot = san.LastIndexOf('.');
                san = san.Substring(ixLastDot + 1);
            }

            if (engine.Length != 4) {
                throw new ArgumentOutOfRangeException("Expected engine move with two squares");
            }

            int fromFile = engine[0] - 'a';
            int fromRank = (int)char.GetNumericValue(engine[1]) - 1;
            int toFile = engine[2] - 'a';
            int toRank = (int)char.GetNumericValue(engine[3]) - 1;
            string toSquare = engine.Substring(2);

            sbyte piece = position[fromRank * 16 + fromFile];
            if (piece == 0) {
                return false;
            }

            string sPiece = piece.GetSanPiece();
            if (sPiece == "P") {
                if (string.Equals(toSquare, san)) {
                    return true;
                }
                if (string.Equals(engine[0] + "x" + toSquare, san)) {
                    return true;
                }
                return false;
            }

            if (!string.IsNullOrEmpty(sPiece)) {
                if (string.Equals(sPiece + toSquare, san)) {
                    return true;
                }
                if (string.Equals(sPiece + "x" + toSquare, san)) {
                    return true;
                }

                // disambiguation by file
                if (string.Equals(sPiece + engine[0] + toSquare, san)) {
                    return true;
                }
                if (string.Equals(sPiece + engine[0] + "x" + toSquare, san)) {
                    return true;
                }

                // disambiguation by rank
                if (string.Equals(sPiece + engine[1] + toSquare, san)) {
                    return true;
                }
                if (string.Equals(sPiece + engine[1] + "x" + toSquare, san)) {
                    return true;
                }

                // disambiguation by both
                if (string.Equals(sPiece + engine[0] + engine[1] + toSquare, san)) {
                    return true;
                }
                if (string.Equals(sPiece + engine[0] + engine[1] + "x" + toSquare, san)) {
                    return true;
                }
            }

            return false;
        }

    }
}
