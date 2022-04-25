using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess.PgnTokens {
    internal class HeaderToken : PgnToken {

        public string Name { get; }
        public string Value { get; }

        public HeaderToken(string name, string value) {
            this.Name = name;
            this.Value = value;
            base.TokenType = "Header";
        }


    }
}
