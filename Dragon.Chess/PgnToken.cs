using Dragon.Chess.PgnTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    internal abstract class PgnToken {

        public string TokenType { get; internal set; }


        public static PgnToken EmptyLine = new LiteralToken("EmptyLine");
    }
}
