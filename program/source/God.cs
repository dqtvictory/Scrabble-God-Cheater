using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrabble
{
    class ScrabbleGod
    {
        private ScrabbleGame game = new ScrabbleGame();
        public ScrabbleGame Game { get => game; }

        private static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetCombinations(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public (int, List<((int, int), char)>) Cheat(char[] tiles)
        {
            var state = game.Board.State;
            var squares = game.FindObligatedSquares();
            int highScore = 0;
            List<((int, int), char)> bestMove = new List<((int, int), char)>(7);

            // List all racks of tiles available for placement (useful if there are blanks)
            var len = tiles.Length;
            HashSet<char[]> racks;
            var blank = tiles.Count((char c) => c == '?');
            Array.Sort(tiles);

            if (blank == 0)
            {
                racks = new HashSet<char[]>(1);
                racks.Add(tiles);
            }
            else
            {
                var letters = new char[len];
                string abc = "abcdefghijklmnopqrstuvwxyz";
                if (blank == 1)
                {
                    racks = new HashSet<char[]>(26);
                    tiles[1..].CopyTo(letters, 1);
                    foreach (var c in abc)
                    {
                        letters[0] = c;
                        racks.Add(letters);
                    }
                }
                else
                {
                    racks = new HashSet<char[]>(351);
                    tiles[2..].CopyTo(letters, 2);
                    foreach (var combi in GetCombinations(abc, 2))
                    {
                        combi.ToArray().CopyTo(letters, 0);
                        racks.Add(letters);
                    }
                }
            }
            // Iterate every square on the board and attempt to place tiles
            ulong iterCount = 0;
            if (!game.Started)
            {
                // Special case when game not started. This reduce execution time
                var startRow = new (int, int)[13];
                for (int j = 0; j < 13; j++)
                    startRow[j] = (7, j + 1);
                for (int numTiles = 2; numTiles <= 7; numTiles++)
                {
                    var orderedTiles = new HashSet<char[]>();
                    for (int idx = 7 - numTiles; idx < 7; idx++)
                    {
                        var steps = startRow[idx..(idx + numTiles)];
                        foreach (var rack in racks)
                            foreach (var permu in GetPermutations(rack, numTiles))
                            {
                                var permuArr = permu.ToArray();
                                if (orderedTiles.Contains(permuArr)) continue;
                                iterCount++;
                                orderedTiles.Add(permuArr);
                                var placed = steps.Zip(permuArr).ToList();
                                var score = game.Attempt(placed, true);
                                if (score > highScore) 
                                {
                                    highScore = score;
                                    bestMove = placed;
                                }
                            }
                    }
                }
            }
            else
            {
                // When game has already started, check every square on board
                foreach (var pos in state.Keys)
                {
                    if (state[pos] > 0) continue;
                    (int, int) hCurrent, vCurrent;
                    List<(int, int)> hSteps, vSteps;
                    bool canPlaceTiles = false;

                    hCurrent = pos;
                    hSteps = new List<(int, int)>(len);
                    for (int numTiles = 1; numTiles <= len; numTiles++)
                    {
                        if (hCurrent == (-1, -1)) break;
                        hSteps.Add(hCurrent);
                        if (squares.Contains(hCurrent))
                            canPlaceTiles = true;
                        if (canPlaceTiles)
                        {
                            var orderedTiles = new HashSet<char[]>();
                            foreach (var rack in racks)
                                foreach (var permu in GetPermutations(rack, numTiles))
                                {
                                    var permuArr = permu.ToArray();
                                    if (orderedTiles.Contains(permuArr)) continue;
                                    iterCount++;
                                    orderedTiles.Add(permuArr);
                                    var placed = hSteps.Zip(permuArr).ToList();
                                    var score = game.Attempt(placed, true);
                                    if (score > highScore)
                                    {
                                        highScore = score;
                                        bestMove = placed;
                                    }
                                }
                        }
                        hCurrent = game.GetNextSquare(hCurrent, true);
                    }
                    canPlaceTiles = false;

                    vCurrent = pos;
                    vSteps = new List<(int, int)>(len);
                    for (int numTiles = 1; numTiles <= len; numTiles++)
                    {
                        if (vCurrent == (-1, -1)) break;
                        vSteps.Add(vCurrent);
                        if (squares.Contains(vCurrent))
                            canPlaceTiles = true;
                        if (canPlaceTiles)
                        {
                            var orderedTiles = new HashSet<char[]>();
                            foreach (var rack in racks)
                                foreach (var permu in GetPermutations(rack, numTiles))
                                {
                                    var permuArr = permu.ToArray();
                                    if (orderedTiles.Contains(permuArr)) continue;
                                    orderedTiles.Add(permuArr);
                                    var placed = vSteps.Zip(permuArr).ToList();
                                    var score = game.Attempt(placed, false);
                                    if (score > highScore)
                                    {
                                        highScore = score;
                                        bestMove = placed;
                                    }
                                }
                        }
                        vCurrent = game.GetNextSquare(vCurrent, false);
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine($"Number of iterations: {iterCount}");
            bestMove.TrimExcess();
            return (highScore, bestMove);
        }
    }
}
