namespace Dragon.Chess {
    // One alternative to this that we used in later versions of Cray Blitzs and in Crafty as well, 
    // is to represent pieces like this: pawn=1, knight=2, king=3, bishop=5, rook=6 and queen=7. 
    // The advantage of this is that if you AND the piece value with 4, and get a non-zero result, 
    // you know that is a sliding piece. If the result of the AND is zero, this is not a sliding piece. 
    // Next, if this is a sliding piece and you AND the piece value with a 1, and the result is non-zero, 
    // you have a piece that can slide diagonallyl if you AND the piece value with a 2 and the result is 
    // non-zero, you have a piece that slides vertically/horizontally like a rook. Note that the queen 
    // has both of these bits set and that it can slide in all 8 directions. This may or may not be 
    // useful in your program, but since it still only requires 3 bits to represent a piece, 
    // there is nothing to lose by using this.

    public enum Piece : sbyte {
        Empty = 0,
        WhitePawn = 1,
        WhiteKnight = 2,
        WhiteBishop = 5,
        WhiteRook = 6,
        WhiteQueen = 7,
        WhiteKing = 3,
        BlackPawn = -1,
        BlackKnight = -2,
        BlackBishop = -5,
        BlackRook = -6,
        BlackQueen = -7,
        BlackKing = -3,

        /// <summary>
        /// Used internally to represent an intermediate state
        /// </summary>
        Undefined = 9
    }

    public enum ColorlessPiece : int {
        Empty = 0,
        Pawn = 1,
        Knight = 2,
        Bishop = 5,
        Rook = 6,
        Queen = 7,
        King = 3,

        /// <summary>
        /// Used internally to represent an intermediate state
        /// </summary>
        Undefined = 9
    }
}
