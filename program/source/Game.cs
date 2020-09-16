using System;
using System.Collections.Generic;

namespace Scrabble
{
    public class ScrabbleGame
    {
        private ScrabbleBoard board;
        private bool started = false;
        public ScrabbleBoard Board { get => board; }
        public bool Started { get => started; }
        private PlacementTest tester;

        public ScrabbleGame()
        {
            Console.WriteLine("Initializing new Scrabble Game...");
            board = new ScrabbleBoard();
            tester = new PlacementTest(this);
            Console.WriteLine("Scrabble Game initialized");
        }

        public (int, int) GetNextSquare((int i, int j) current, bool horizontal)
        {
            var state = board.State;
            var size = ScrabbleBoard.Size;
            var (i, j) = horizontal ? (current.i, current.j) : (current.j, current.i);
            while (j < size - 1)
            {
                j++;
                var check_pos = horizontal ? (i, j) : (j, i);
                if (state[check_pos] == 0)
                    return check_pos;
            }
            return (-1, -1);
        }

        public HashSet<(int, int)> FindObligatedSquares()
        {
            var size = ScrabbleBoard.Size;
            HashSet<(int, int)> squares;
            if (started)
            {
                squares = new HashSet<(int, int)>(225);
                var state = board.State;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        if (state[(i, j)] > 0)
                        {
                            var neighbors = new (int, int)[] { (i - 1, j), (i + 1, j), (i, j - 1), (i, j + 1) };
                            foreach (var nei in neighbors)
                                if (state.ContainsKey(nei) && state[nei] == 0)
                                    squares.Add(nei);
                        }
                squares.TrimExcess();

            }
            else
            {
                squares = new HashSet<(int, int)>(1);
                squares.Add(board.StartPos);
            }
            return squares;
        }

        public int Attempt(List<((int, int), char)> placed, bool horizontal)
        {
            var (code, _) = tester.DoTests(placed, horizontal);
            var premiums = board.Premiums;
            if (code != 0)
                return 0;
            else
            {
                var directionalWord = tester.DirectionalWord;
                var score = tester.DirectionalScore;
                foreach (var ((i, j), tile) in placed)
                {
                    var pos = (i, j);
                    var (before, after) = board.GetLettersScore(pos, !horizontal);
                    var word = before.letters + tile + after.letters;
                    if (word.Length == 1)
                        continue;
                    int times = 1;
                    var wordScore = before.score + after.score;
                    var letScore = board.GetFaceValue(tile);
                    if (premiums.ContainsKey(pos))
                    {
                        if (premiums[pos] == "TW")
                            times *= 3;
                        else if (premiums[pos] == "DW")
                            times *= 2;
                        else if (premiums[pos] == "TL")
                            letScore *= 3;
                        else if (premiums[pos] == "DL")
                            letScore *= 2;
                    }
                    score += times * (wordScore + letScore);
                }
                if (placed.Count == 7)
                    score += 50;
                return score;
            }
        }

        public void UpdateBoard(Dictionary<(int, int), char> update)
        {
            if (!started)
                started = true;
            board.UpdateState(update);
        }

        public void NewGame()
        {
            board.InitializeBoard(false);
            started = false;
        }
    }
}
