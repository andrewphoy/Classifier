using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ServiceStack.Text;
using Dragon.Models;
using Dapper;

namespace Dragon.Chess {
    public static class PgnImporter {

        public static Player ParsePlayer(string name) {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            Player search = Player.CreateFromName(name);
            var dbPlayer = Current.Db.Query<Player>("SELECT * FROM Players WHERE FirstName = @first AND LastName = @last", new { first = search.FirstName, last = search.LastName }).FirstOrDefault();
            if (dbPlayer != null) {
                return dbPlayer;
            }

            // no player found
            return search;
        }

        public static bool InsertGame(PgnGame pgn , ParsingGame game, Guid source) {
            int id = InsertGameRecord(pgn, game, source);
            StoreStartState(game, id);

            Dictionary<byte[], int> positions = UploadPositions(game);

            List<dynamic> insertMoves = new List<dynamic>();
            List<dynamic> textAnnotations = new List<dynamic>();
            List<dynamic> nags = new List<dynamic>();
            foreach (ParsingMove move in game.Moves.Values) {
                if (move.MoveId == 0) {
                    continue;
                }
                insertMoves.Add(new {
                    GameId = id,
                    MoveId = move.MoveId,
                    PositionId = positions[move.Position.Encoded],
                    MoveBody = move.MoveBody,
                    ParentMoveId = move.ParentMoveId,
                    Ply = move.Ply,
                    HalfMoveCount = move.HalfMoveCount,
                    Transitions = move.GetTransitions()
                });
                if (!string.IsNullOrEmpty(move.Comment)) {
                    textAnnotations.Add(new {
                        GameId = id,
                        MoveId = move.MoveId,
                        Text = move.Comment
                    });
                }
                if (!string.IsNullOrEmpty(move.Nag)) {
                    nags.Add(new {
                        GameId = id,
                        MoveId = move.MoveId,
                        NagString = move.Nag.Replace(" ", "")
                    });
                }
            }

            //TODO revisit this code - might be possible/preferable to use a temp table
            var insertSql = "INSERT INTO Moves (GameId, MoveId, PositionId, MoveBody, ParentMoveId, Ply, HalfMoveCount, Transitions) VALUES (@GameId, @MoveId, @PositionId, @MoveBody, @ParentMoveId, @Ply, @HalfMoveCount, @Transitions)";
            int insertCount = Current.Db.Execute(insertSql, insertMoves);

            // finally upload comments and nags
            if (nags.Count > 0) {
                Current.Db.Execute("INSERT INTO Nags (GameId, MoveId, NagString) VALUES (@GameId, @MoveId, @NagString)", nags);
            }
            if (textAnnotations.Count > 0) {
                Current.Db.Execute("INSERT INTO TextAnnotations (GameId, MoveId, Text) VALUES (@GameId, @MoveId, @Text)", textAnnotations);
            }

            return true;
        }

        private static int InsertGameRecord(PgnGame pgn, ParsingGame game, Guid source) {
            string sql = @"INSERT INTO Games (Title, OwnerUserId, WhitePlayerId, BlackPlayerId, WhitePlayerName, BlackPlayerName,
WhiteRating, BlackRating, RatingType, TimeControlId, TimeControl, GameDate, Year, Month, Day, CreationDate, PlyCount, MoveCount, Result,
ResultExpanded, Round, OpeningId, AnnotatorPlayerId, AnnotatorName, EventId, EventName, ClubId, ClubName, LocationId,
LocationName, LocationCity, LocationState, LocationCountry, GameComment, PgnSource) VALUES (@Title, @OwnerUserId, @WhitePlayerId, @BlackPlayerId,
@WhitePlayerName, @BlackPlayerName, @WhiteRating, @BlackRating, @RatingType, @TimeControlId, @TimeControl, @GameDate, @Year, @Month, @Day,
GETUTCDATE(), @PlyCount, @MoveCount, @Result, @ResultExpanded, @Round, @OpeningId, @AnnotatorPlayerId, @AnnotatorName, @EventId, @EventName, 
@ClubId, @ClubName, @LocationId, @LocationName, @LocationCity, @LocationState, @LocationCountry, @GameComment, @PgnSource); 
SELECT CAST(SCOPE_IDENTITY() as int);";

            string whiteName = pgn.WhitePlayerName;
            string blackName = pgn.BlackPlayerName;
            string annotatorName = pgn.AnnotatorName;

            var white = ParsePlayer(pgn.WhitePlayerName);
            if (white != null) {
                pgn.WhitePlayer = white;
                whiteName = (white.FirstName + " " + white.LastName).Trim();
                pgn.WhitePlayerId = white.Id;
            }

            var black = ParsePlayer(pgn.BlackPlayerName);
            if (black != null) {
                pgn.BlackPlayer = black;
                blackName = (black.FirstName + " " + black.LastName).Trim();
                pgn.BlackPlayerId = black.Id;
            }

            var annotator = ParsePlayer(pgn.AnnotatorName);
            if (annotator != null) {
                annotatorName = (annotator.FirstName + " " + annotator.LastName).Trim();
                pgn.AnnotatorPlayerId = annotator.Id;
            }

            //TODO parse event

            //TODO parse site

            //TODO parse club

            string title = pgn.GetTitle();
            
            dynamic data = new {
                Title = title,
                OwnerUserId = 0,
                WhitePlayerId = pgn.WhitePlayerId,
                BlackPlayerId = pgn.BlackPlayerId,
                WhitePlayerName = whiteName,
                BlackPlayerName = blackName,
                WhiteRating = pgn.WhiteRating,
                BlackRating = pgn.BlackRating,
                RatingType = pgn.RatingType,
                TimeControlId = 0,
                TimeControl = pgn.TimeControl,
                GameDate = pgn.GameDate,
                Year = pgn.Year,
                Month = pgn.Month,
                Day = pgn.Day,
                PlyCount = pgn.PlyCount,
                MoveCount = game.MoveCount(),
                Result = pgn.Result,
                ResultExpanded = pgn.ResultExpanded,
                Round = pgn.Round,
                OpeningId = 0,
                AnnotatorPlayerId = pgn.AnnotatorPlayerId,
                AnnotatorName = annotatorName,
                EventId = 0,
                EventName = pgn.EventName,
                ClubId = 0,
                ClubName = "",
                LocationId = 0,
                LocationName = pgn.Site,
                LocationCity = "",
                LocationState = "",
                LocationCountry = "",
                GameComment = game.GameComment,
                PgnSource = source
            };

            IEnumerable<int> result = Current.Db.Query<int>(sql, data);
            return result.First();
        }

        private static bool StoreStartState(ParsingGame game, int gameId) {
            string sql = @"SELECT Id FROM BoardStates
WHERE CurrentPly = @currentPly
AND ColorToMove = @colorToMove
AND PositionId = @positionId
AND HalfMoveCount = @halfMoveCount
AND WhiteKingPosition = @whiteKing
AND BlackKingPosition = @blackKing
AND WhiteCastling = @whiteCastling
AND BlackCastling = @blackCastling";

            if (game.StartState.EnPassant.HasValue) {
                sql += " AND EnPassant = @enPassant";
            } else {
                sql += " AND EnPassant IS NULL";
            }

            int positionId = 0;
            if (game.Moves != null && game.Moves.Count > 0) {
                if (game.Moves[1].Position != null) {
                    positionId = game.Moves[1].Position.Store();
                }
            }

            var data = new {
                currentPly = game.StartState.CurrentPly,
                colorToMove = game.StartState.ColorToMove,
                positionId = positionId,
                enPassant = game.StartState.EnPassant,
                halfMoveCount = game.StartState.HalfMoveCount,
                whiteKing = game.StartState.WhiteKingPosition,
                blackKing = game.StartState.BlackKingPosition,
                whiteCastling = game.StartState.WhiteCastling,
                blackCastling = game.StartState.BlackCastling
            };

            int? id = Current.Db.Query<int?>(sql, data).FirstOrDefault();

            if (!id.HasValue) {
                // insert the board state
                Current.Db.Execute(
                    @"INSERT INTO BoardStates (CurrentPly, ColorToMove, PositionId, EnPassant, HalfMoveCount, WhiteKingPosition, BlackKingPosition, WhiteCastling, BlackCastling)
VALUES (@currentPly, @colorToMove, @positionId, @enPassant, @halfMoveCount, @whiteKing, @blackKing, @whiteCastling, @blackCastling)", data);
                id = Current.Db.Query<int?>(sql, data).FirstOrDefault();
            }

            if (!id.HasValue) {
                // panic
                throw new PgnException("Could not insert the current board state into the database");
            }

            // insert into the GameBoardStates table
            Current.Db.Execute("INSERT INTO GameBoardStates (GameId, BoardStateId) VALUES (@gameId, @id)", new { gameId, id });

            return true;
        }

        private static Dictionary<byte[], int> UploadPositions(ParsingGame game) {
            Dictionary<byte[], int> resolvedPositions;
            List<byte[]> arrPositions = new List<byte[]>();

            // loop over the moves in the game to get the positions;
            foreach (ParsingMove move in game.Moves.Values) {
                if (move.MoveId != 0) {
                    arrPositions.Add(move.Position.CalculateEncoded());
                }
            }

            // get existing id's for the moves
            var selectSql = "SELECT Id, Encoded FROM Positions WHERE Encoded in @arrPositions";
            var comparer = new ByteArrayEqualityComparer();
            resolvedPositions = Current.Db.Query<Position>(selectSql, new { arrPositions }).ToDictionary(p => p.Encoded, p => p.Id, comparer);

            // extract any missing positions
            List<dynamic> missingPositions = new List<dynamic>();
            List<byte[]> arrMissingPositions = new List<byte[]>();
            foreach(byte[] rawPos in arrPositions) {
                if (!resolvedPositions.ContainsKey(rawPos)) {
                    missingPositions.Add(new { Encoded = rawPos });
                    arrMissingPositions.Add(rawPos);
                }
            }

            // upload missing positions
            if (missingPositions.Count > 0) {
                var insertSql = "INSERT INTO Positions (Encoded) VALUES (@encoded)";
                int insertCount = Current.Db.Execute(insertSql, missingPositions);
            }

            // get the id's for the positions that were previously missing
            var insertedPositions = Current.Db.Query<Position>(selectSql, new { arrPositions = arrMissingPositions });

            // create a single dictionary
            foreach(Position insPos in insertedPositions) {
                resolvedPositions[insPos.Encoded] = insPos.Id;
            }

            return resolvedPositions;
        }
    }
}
