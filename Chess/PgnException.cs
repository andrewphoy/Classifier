using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
