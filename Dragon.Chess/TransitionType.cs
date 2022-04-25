namespace Dragon.Chess {
    public enum TransitionType {
        Move = 3, // 11   <-- note that this is the only transition that ends with two set bits
        Add = 1, // 01
        Remove = 2, // 10
        EnPassant = 4, // 100
        Castling = 6, // 110
        PawnPromo = 0 // 00
    }
}
