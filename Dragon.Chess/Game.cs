using System;
using System.Collections.Generic;
using System.Text;

namespace Dragon.Chess {
    public class Game {

        /// <summary>
        /// The primary key from the database
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// A user created string title - used for display purposes
        /// </summary>
        /// <example>Kasparov's Immortal</example>
        public string Title { get; set; }

        /// <summary>
        /// Foreign key to the players table if the white player is a known player
        /// </summary>
        public long WhitePlayerId { get; set; }

        /// <summary>
        /// Foreign key to the players table if the black player is a known player
        /// </summary>
        public long BlackPlayerId { get; set; }

        public string WhitePlayerName { get; set; }
        public string BlackPlayerName { get; set; }

        public string WhiteTitle { get; set; }
        public string BlackTitle { get; set; }

        public int? WhiteFideId { get; set; }
        public int? BlackFideId { get; set; }

        public int? WhiteRating { get; set; }
        public int? BlackRating { get; set; }

        /// <summary>
        /// Describes the rating type of the players
        /// </summary>
        /// <example>USCF or FIDE or Lichess Bullet</example>
        public string RatingType { get; set; }

        public int? TimeControlId { get; set; }
        public string TimeControl { get; set; }

        /// <summary>
        /// GameDate will only be filled if the date is known
        /// Otherwise some subset of Year, Month, and Date might be populated
        /// </summary>
        public DateTime? GameDate { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }


        public DateTime? EventDate { get; set; }
        public int? EventYear { get; set; }
        public int? EventMonth { get; set; }
        public int? EventDay { get; set; }

        public DateTime? CreationDate { get; set; }

        public int? PlyCount { get; set; }

        /// <summary>
        /// 1-0, 0-1, 1/2, or *
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// A full description of how the result happened
        /// </summary>
        /// <example>White resigned, draw by repetition, etc.</example>
        public string ResultExpanded { get; set; }

        public int? Round { get; set; }
        public int? BoardNumber { get; set; }
        public int? OpeningId { get; set; }
        public int OwnerUserId { get; set; }

        public int? AnnotatorPlayerId { get; set; }
        public string AnnotatorName { get; set; }

        // event
        public int? EventId { get; set; }
        // if we have more than a name, we will create a new event entry
        public string EventName { get; set; }

        public string SiteName { get; set; }

        // club
        public int? ClubId { get; set; }
        public string ClubName { get; set; }

        // location
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationCity { get; set; }
        public string LocationState { get; set; }
        public string LocationCountry { get; set; }

        public string GameComment { get; set; }

        public Dictionary<int, Move> Moves { get; set; }

        //public Player WhitePlayer { get; set; }
        //public Player BlackPlayer { get; set; }
        //public Event Event { get; set; }
        //public Opening Opening { get; set; }
        //public Position StartPosition { get; set; }
        //public BoardState StartState { get; set; }

        //public sbyte[] Position { get; set; }
        //public int WhiteCastling { get; set; }
        //public int BlackCastling { get; set; }


        public Game() {
            this.Moves = new Dictionary<int, Move>();
        }
    }
}
