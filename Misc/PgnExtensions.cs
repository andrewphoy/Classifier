using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dragon.Models;

namespace Dragon.Chess
{
    public static class PgnExtensions
    {

        public static bool AddHeader(this PgnGame game, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            bool resolvedHeader = true;

            switch (name.ToLower())
            {
                case "event":
                    game.EventName = value;
                    game.StrEvent = value;
                    game.Event.Name = value;
                    break;
                case "eventdate":
                    DateInfo di = DateInfo.Create(value);
                    game.Event.StartDate = di.Date;
                    game.Event.StartYear = di.Year;
                    game.Event.StartMonth = di.Month;
                    game.Event.StartDay = di.Day;
                    break;
                case "eventtype":
                    game.Event.Type = value;
                    break;
                case "eventcountry":
                    game.Event.Country = value;
                    break;
                case "eventrounds":
                    int eventRounds;
                    if (int.TryParse(value, out eventRounds))
                    {
                        game.Event.Rounds = eventRounds;
                    }
                    else
                    {
                        resolvedHeader = false;
                    }
                    break;
                case "site":
                    game.Site = value;
                    break;
                case "white":
                    game.WhitePlayerName = value;
                    break;
                case "black":
                    game.BlackPlayerName = value;
                    break;
                case "whiteelo":
                    int whiteRating;
                    if (int.TryParse(value, out whiteRating))
                    {
                        game.WhiteRating = whiteRating;
                        game.RatingType = "ELO";
                    }
                    else
                    {
                        resolvedHeader = false;
                    }
                    break;
                case "blackelo":
                    int blackRating;
                    if (int.TryParse(value, out blackRating))
                    {
                        game.BlackRating = blackRating;
                        game.RatingType = "ELO";
                    }
                    else
                    {
                        resolvedHeader = false;
                    }
                    break;
                case "round":
                    int round;
                    if (int.TryParse(value, out round))
                    {
                        game.Round = round;
                    }
                    else
                    {
                        resolvedHeader = false;
                    }
                    break;
                case "plycount":
                    int plyCount;
                    if (int.TryParse(value, out plyCount))
                    {
                        game.PlyCount = plyCount;
                    }
                    else
                    {
                        resolvedHeader = false;
                    }
                    break;
                case "eco":
                    game.EcoRaw = value.ToUpper();
                    break;
                case "annotator":
                    game.AnnotatorName = value;
                    break;
                case "result":
                    if (value.StartsWith("1/2"))
                    {
                        game.Result = "1/2";
                    }
                    else
                    {
                        game.Result = value;
                    }
                    break;
                case "timecontrol":
                    game.TimeControl = value;
                    break;
                case "date":
                    DateInfo gameDi = DateInfo.Create(value);
                    game.GameDate = gameDi.Date;
                    game.Year = gameDi.Year;
                    game.Month = gameDi.Month;
                    game.Day = gameDi.Day;
                    break;
                case "title":
                    game.Title = value;
                    break;
                default:
                    resolvedHeader = false;
                    break;
            }

            if (!resolvedHeader)
            {
                game.AdditionalHeaders[name] = value;
            }

            return true;
        }

        public static string GetTitle(this PgnGame game)
        {
            if (!string.IsNullOrEmpty(game.Title))
            {
                return game.Title;
            }

            string title, white, black;

            if (game.WhitePlayer != null)
            {
                white = game.WhitePlayer.LastName;
            }
            else
            {
                white = game.WhitePlayerName;
            }

            if (game.BlackPlayer != null)
            {
                black = game.BlackPlayer.LastName;
            }
            else
            {
                black = game.BlackPlayerName;
            }

            if (!string.IsNullOrEmpty(white) && !string.IsNullOrEmpty(black))
            {
                title = white + " - " + black;
                if (!string.IsNullOrEmpty(game.Result))
                {
                    if (game.Result == "1/2")
                    {
                        title += ", 1/2-1/2";
                    }
                    else if (game.Result.Trim() != "*")
                    {
                        title += ", " + game.Result;
                    }
                }
                return title;
            }

            // missing either white or black
            if (!string.IsNullOrEmpty(white) || !string.IsNullOrEmpty(black))
            {
                return white + black;
            }

            // missing both white and black
            return "Untitled Game";
        }
    }
}
