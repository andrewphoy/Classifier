using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dragon.Chess
{
    public static class NagConverter {

        /// <summary>
        /// Takes a string with nag literals and returns the component nags
        /// </summary>
        /// <example>Converts ! to $1</example>
        /// <param name="literal"></param>
        /// <returns></returns>
        public static string GetNagsForLiteral(string literal) {
            if (string.IsNullOrEmpty(literal)) {
                return "";
            }

            literal = literal.TrimStart();
            if (literal.StartsWith("!!")) {
                return "$3" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("??")) {
                return "$4" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("!?")) {
                return "$5" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("?!")) {
                return "$6" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("!")) {
                return "$1" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("?")) {
                return "$2" + GetNagsForLiteral(literal.Substring(1));
            }

            if (literal.StartsWith("+-")) {
                return "$18" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("-+")) {
                return "$19" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("±")) {
                return "$16" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("+/-")) {
                return "$16" + GetNagsForLiteral(literal.Substring(3));
            }
            if (literal.StartsWith("∓")) {
                return "$17" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("-/+")) {
                return "$17" + GetNagsForLiteral(literal.Substring(3));
            }
            if (literal.StartsWith("⩲")) {
                return "$14" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("+/=")) {
                return "$14" + GetNagsForLiteral(literal.Substring(3));
            }
            if (literal.StartsWith("⩱")) {
                return "$15" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("+/=")) {
                return "$15" + GetNagsForLiteral(literal.Substring(3));
            }
            if (literal.StartsWith("=")) {
                return "$10" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("∞")) {
                return "$13" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("□")) {
                return "$7" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("∆")) {
                return "$140" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("⌓")) {
                return "$142" + GetNagsForLiteral(literal.Substring(1));
            }
            if (literal.StartsWith("RR")) {
                return "$145" + GetNagsForLiteral(literal.Substring(2));
            }
            if (literal.StartsWith("N")) {
                return "$146" + GetNagsForLiteral(literal.Substring(1));
            }
            return GetNagsForLiteral(literal.Substring(1));
        }
    }
}
