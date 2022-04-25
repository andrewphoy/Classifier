using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess.PgnTokens {
    internal class LiteralToken : PgnToken {

        public LiteralToken(string type) {
            TokenType = type;
        }
    }
}
