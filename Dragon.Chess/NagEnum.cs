using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    [Flags]
    public enum Nag {

        // move annotations
        [Nag(Name = "Good move", Symbol = "!", NagNumber = 1)]
        GoodMove = 1,
        [Nag(Name = "Mistake", Symbol = "?", NagNumber = 2)]
        PoorMove = 2,
        [Nag(Name = "Excellent move", Symbol = "!!", NagNumber = 3)]
        VeryGoodMove = 4,
        [Nag(Name = "Blunder", Symbol = "??", NagNumber = 4)]
        VeryPoorMove = 8,
        [Nag(Name = "Interesting move", Symbol = "!?", NagNumber = 5)]
        InterestingMove = 16,
        [Nag(Name = "Dubious move", Symbol = "?!", NagNumber = 6)]
        DubiousMove = 32,
        [Nag(Name = "Only move", Symbol = "□", NagNumber = 7)]
        OnlyMove = 64,
        [Nag(Name = "Zugzwang", Symbol = "⨀", NagNumber = 22, AdditionalNumbers = new int[] { 23 })]
        Zugzwang = 128,

        // position annotations
        [Nag(Name = "Equal position", Symbol = "=", NagNumber = 10, AdditionalNumbers = new int[] { 11, 12 })]
        EqualPosition = 256,
        [Nag(Name = "Unclear position", Symbol = "∞", NagNumber = 13)]
        UnclearPosition = 512,
        [Nag(Name = "White is slightly better", Symbol = "⩲", NagNumber = 14)]
        WhiteSlightAdvantage = 1024,
        [Nag(Name = "Black is slightly better", Symbol = "⩱", NagNumber = 15)]
        BlackSlightAdvantage = 2048,
        [Nag(Name = "White is clearly better", Symbol = "±", NagNumber = 16)]
        WhiteAdvantage = 4096,
        [Nag(Name = "Black is clearly better", Symbol = "∓", NagNumber = 17)]
        BlackAdvantage = 8192,
        [Nag(Name = "White is winning", Symbol = "+-", NagNumber = 20)]
        WhiteWinning = 16384,
        [Nag(Name = "Black is winning", Symbol = "-+", NagNumber = 21)]
        BlackWinning = 32768,

        // additional annotations
        [Nag(Name = "With Compensation", Symbol = "=∞", NagNumber = 44, AdditionalNumbers = new int[] { 45, 46, 47 })]
        WithCompensation = 65536,
        [Nag(Name = "Attack", Symbol = "→", NagNumber = 40, AdditionalNumbers = new int[] { 41 })]
        Attack = 131072,
        [Nag(Name = "Initiative", Symbol = "↑", NagNumber = 36, AdditionalNumbers = new int[] { 37, 38, 39 })]
        Initiative = 262144,
        [Nag(Name = "Counterplay", Symbol = "⇆", NagNumber = 132, AdditionalNumbers = new int[] { 130, 131, 133, 134, 145 })]
        Counterplay = 524288,
        [Nag(Name = "Time Pressure", Symbol = "⊕", NagNumber = 136, AdditionalNumbers = new int[] { 137, 138, 139 })]
        Zeitnot = 1048576,
        [Nag(Name = "Development", Symbol = "↑↑", NagNumber = 32, AdditionalNumbers = new int[] { 30, 31, 33, 34, 35 })]
        Development = 2097152,
        [Nag(Name = "Novelty", Symbol = "N", NagNumber = 146)]
        Novelty = 4194304,
        [Nag(Name = "With the idea", Symbol = "∆", NagNumber = 140)]
        WithTheIdea = 8388608,

        // search based annotations
        CriticalPosition = 16777216,
        PawnStructure = 33554432,
        Tabiya = 67108864


        //  134217728
        //  268435456
        //  536870912
        // 1073741824
        // 2147483648
    }
}
