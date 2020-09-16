using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrabble
{
    class Program
    {
        static ScrabbleGod god;
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("WELCOME TO THE SCRABBLE GOD PROGRAM");
            Console.WriteLine("===================================");
            Console.WriteLine();
            
            god = new ScrabbleGod();
            while (true)
            {
                Console.WriteLine();
                Console.Write(">>> ");
                var command = Console.ReadLine();
                if (command.ToLower() == "quit")
                {
                    Console.WriteLine("Bye!");
                    Console.WriteLine("Press any key to leave the Scrabble God's Kingdom...");
                    Console.ReadKey();
                    break;
                }
                else
                {
                    try { ParseCommand(command); }
                    catch (FormatException e) { Console.WriteLine(e.ToString()); }
                }
            }
        }

        static string DisplayMove(List<((int i, int j) pos, char tile)> move)
        {
            if (move.Count() == 1)
                return $"placing tile {move[0].tile} on square {move[0].pos}";
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
            return $"placing {direction} tiles {letters} starting on square {first}";
        }

        static void ParseCommand(string command)
        {
            var commandBreakdown = command.Split(' ');
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
                var displayMove = DisplayMove(move);
                Console.WriteLine($"The best move gains {score} points by {displayMove}");
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
                    Console.WriteLine("No move found.");
                else
                {
                    var displayMove = DisplayMove(move);
                    Console.WriteLine($"The best move gains {score} points by {displayMove}");
                }
            }
            else
            {
                throw new FormatException("Command not understood. Try again.");
            }
        }
    }
}