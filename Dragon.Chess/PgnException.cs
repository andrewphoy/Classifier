using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class PgnException : Exception {
        public PgnException() : base() {

        }

        public PgnException(string message) : base(message) {

        }

        public PgnException(string message, Exception innerException) : base(message, innerException) {

        }
    }
}
