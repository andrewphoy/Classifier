using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    /// <summary>
    /// This is a highly optimized (for storage size) way to encode a move
    /// The primary key is the combination of GameId and MoveId
    /// Every game has one (or more) moves that have a ParentMoveId == 0
    /// </summary>
    public class EncodedMove : EncodedMoveBase {
        public long GameId { get; set; }
        public short MoveId { get; set; }
        public short ParentMoveId { get; set; }
        public long PositionId { get; set; }
        
        /// <summary>
        /// The order of variations, 0 is the main line,
        /// 1 is the first variation, etc.
        /// </summary>
        public byte VariationOrder { get; set; }

        // flags that get encoded as bits and stored in a single byte

        public EncodedPosition EncodedPosition { get; set; }


    }
}
