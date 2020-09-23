using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Scrabble
{
    class API
    {
        private static HttpListener listener;
        private const string url = "http://localhost:4000/";
        private static ScrabbleGod god;

        static async Task HandleIncomingConnections()
        {
            while (true)
            {
                // Will wait here until we hear from a connection
                var ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                if (req.Url.AbsolutePath != "/scrabble-god-api")
                {
                    resp.Close();
                    continue;
                }

                string respText;

                // Get info about the request
                var reqString = req.QueryString.Get("message");
                Console.WriteLine($"New Request: {reqString}");
                try { respText = ParseRequest(reqString); }
                catch(FormatException ex) { respText = ex.Message; }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    resp.Close();
                    continue;
                }

                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(respText);
                resp.ContentType = "text/utf-8";
                resp.AppendHeader("Access-Control-Allow-Origin", "*");
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"Responded: {respText}");
                resp.Close();
            }
        }

        static string StringRepMove(List<((int i, int j) pos, char tile)> move, int score)
        {
            if (move.Count() == 1)
                return $"Gain {score} points by placing tile {move[0].tile} on square {move[0].pos}";
            string direction;
            var (first, second) = (move[0].pos, move[1].pos);
            if (first.i == second.i)
                direction = "HORIZONTALLY";
            else
                direction = "VERTICALLY";
            var letters = string.Empty;
            foreach (var (pos, tile) in move)
            {
                if (char.IsLower(tile))
                    letters += $"({tile})";
                else
                    letters += tile;
            }
            return $"Gain {score} points by placing {direction} tiles '{letters}' starting on square {first}";
        }

        static string ParseRequest(string req)
        {
            var commandBreakdown = req.Split(' ');
            var first = commandBreakdown[0];
            if (first == "start" && commandBreakdown.Length == 2)
            {
                var tiles = commandBreakdown[1].ToUpper().ToCharArray();
                if (tiles.Length != 7)
                    throw new FormatException("Starting rack must have 7 tiles");
                else if (!tiles.All((char c) => char.IsLetter(c) || c == '?'))
                    throw new FormatException("Rack must contain only letters and blanks (?)");
                foreach (var tile in tiles)
                    if (tiles.Count((char c) => c == tile) > god.Game.Board.GetFaceCount(tile))
                        throw new FormatException($"Too many '{tile}' tiles");
                if (god.Game.Started)
                    god.Game.NewGame();
                var (score, move) = god.Cheat(tiles);
                return StringRepMove(move, score);
            }
            else if (first == "help" && commandBreakdown.Length == 3)
            {
                var state = commandBreakdown[2];
                const string allowedChar = "0abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

                var tiles = commandBreakdown[1].ToUpper().ToCharArray();
                if (tiles.Length > 7)
                    throw new FormatException("Rack must contain no more than 7 tiles");
                else if (!tiles.All((char c) => char.IsLetter(c) || c == '?'))
                    throw new FormatException("Rack must contain only letters and blanks (?)");
                else if (state.Length != 225)
                    throw new FormatException($"State input must have exactly {ScrabbleBoard.Size * ScrabbleBoard.Size} squares");
                else if (!state.All((char tile) => allowedChar.Contains(tile)))
                    throw new FormatException($"Squares input format is incorrect. Check again");
                foreach (var tile in tiles)
                    if (tiles.Count((char c) => c == tile) > god.Game.Board.GetFaceCount(tile))
                        throw new FormatException($"Number of {tile}'s should not exceed {tiles.Count((char c) => c == tile)}");

                var update = new Dictionary<(int, int), char>(225);
                for (int i = 0; i < ScrabbleBoard.Size; i++)
                    for (int j = 0; j < ScrabbleBoard.Size; j++)
                    {
                        var idx = ScrabbleBoard.Size * i + j;
                        char c = state[idx];
                        if (c == '0')
                            c = '\0';
                        update.Add((i, j), c);
                    }
                god.Game.UpdateBoard(update);
                var (score, move) = god.Cheat(tiles);
                if (score == 0)
                    return "No move found.";
                else
                {
                    return StringRepMove(move, score);
                }
            }
            else
            {
                throw new FormatException("Command not understood. Try again.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("WELCOME TO THE SCRABBLE GOD PROGRAM");
            Console.WriteLine("===================================");
            Console.WriteLine();

            god = new ScrabbleGod();

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine($"Listening for connections on {url}");

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}