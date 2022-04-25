using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public int UciStart { get; set; }
        public int UciEnd { get; set; }
        public int UciPiece { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsVariation { get; set; }
        public Move LegalMove { get; set; }

        //// someday I'll have to see how much this chicanery saves
        //public short UciEncoded {
        //    get {
        //        BitArray bits = new BitArray(16);

        //        int mbStart = (UciStart / 16) * 8 + (UciStart % 16);
        //        int mbEnd = (UciEnd / 16) * 8 + (UciEnd % 16);

        //        for (int i = 0; i < 6; i++) {
        //            bits[i] = (mbStart & (1 << i)) > 0;
        //        }
        //        for (int i = 0; i < 6; i++) {
        //            bits[i + 6] = (mbEnd & (1 << i)) > 0;
        //        }

        //        bits[12] = (UciPiece & 1) > 0;
        //        bits[13] = (UciPiece & 2) > 0;
        //        bits[14] = (UciPiece & 4) > 0;

        //        if (IsPromotion) {
        //            bits[15] = true;
        //        }

        //        byte[] bytes = new byte[2];
        //        bits.CopyTo(bytes, 0);
        //        return BitConverter.ToInt16(bytes, 0);
        //    }
        //}

        public Position Position { get; set; }

        public List<ParsingMove> Children { get; private set; }
        public List<Transition> Transitions { get; set; }

        public override string ToString() {
            return this.NumberPattern + this.MoveBody;
        }

        //private long? _transitions = null;
        //public long GetTransitions() {
        //    _transitions = _transitions ?? CalculateEncodedTransitions();
        //    return _transitions.Value;
        //}

        //private long CalculateEncodedTransitions() {
        //    long transitions = 0;
        //    for (int i = Transitions.Count - 1; i >= 0; i--) {
        //        transitions = transitions << 16;
        //        long t = (Transitions[i].ToStorageFormat() & 0xFFFF);
        //        transitions = transitions | t;

        //    }
        //    return transitions;
        //}
    }
}
