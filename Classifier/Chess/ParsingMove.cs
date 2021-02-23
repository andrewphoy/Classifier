using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dragon.Models;

namespace Dragon.Chess {
    public class ParsingMove {
        public ParsingMove(int moveId, int ply, string moveBody, int parentMoveId, string numberPattern) {
            this.MoveId = moveId;
            this.Ply = ply;
            this.MoveBody = moveBody;
            this.ParentMoveId = parentMoveId;
            this.NumberPattern = numberPattern;
            //this.HalfMoveCount

            this.Children = new List<ParsingMove>();
        }

        public ParsingMove(int moveId, int ply, string moveBody, int parentMoveId, string numberPattern, string comment, string nag) :
            this(moveId, ply, moveBody, parentMoveId, numberPattern) {

            this.Comment = comment;
            this.Nag = nag;
        }

        public int MoveId { get; set; }
        public int Ply { get; set; }
        public string MoveBody { get; set; }
        public int ParentMoveId { get; set; }
        public string NumberPattern { get; set; }
        public int HalfMoveCount { get; set; }
        public string Nag { get; set; }
        public string Comment { get; set; }
        public Position Position { get; set; }

        public List<ParsingMove> Children { get; private set; }
        public List<ChessTransition> Transitions { get; set; }

        public override string ToString() {
            return this.NumberPattern + this.MoveBody;
        }

        private long? _transitions = null;
        public long GetTransitions() {
            _transitions = _transitions ?? CalculateEncodedTransitions();
            return _transitions.Value;
        }

        private long CalculateEncodedTransitions() {
            long transitions = 0;
            for (int i = Transitions.Count - 1; i >= 0; i--) {
                transitions = transitions << 16;
                long t = (Transitions[i].ToStorageFormat() & 0xFFFF);
                transitions = transitions | t;

            }
            return transitions;
        }
    }
}
