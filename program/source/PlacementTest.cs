using System.Collections.Generic;

namespace Scrabble
{
    class PlacementTest
    {
        private ScrabbleGame game;
        private string directionalWord;
        private int directionalScore;
        public string DirectionalWord { get => directionalWord; }
        public int DirectionalScore { get => directionalScore; }

        public PlacementTest(ScrabbleGame game)
        {
            this.game = game;
        }

        public (int, string) DoTests(List<((int, int), char)> placed, bool horizontal)
        {
            // The commented code below tests for human error when placing tiles. Since the code was done correctly,
            // these tests are no longer necessary which can cause execution to slow down

            /*if (!TestTilesSameRowCol())
                return (-1, "Tiles must be on the same row or column");
            if (!TestTilesPlacedOnObligatedSquares())
            {
                if (game.Started)
                    return (-2, "Must place at least one tile next to ones already on the board");
                else
                    return (-3, "Start the game by placing one tile over the center square");
            }
            if (!TestContinuousPlacement())
                return (-4, "Tiles must be placed continuously along one direction");*/
            
            directionalWord = string.Empty;
            directionalScore = 0;
            var (valid, word) = TestValidWords(placed, horizontal);
            if (!valid)
                return (-5, $"{word} is not a valid Scrabble word");
            return (0, "No error found!");
        }

        private void FindDirectionalWord(List<((int i, int j) pos, char tile)> placed, bool horizontal)
        {
            var premiums = game.Board.Premiums;
            var state = game.Board.State;
            (int i, int j) posStart, posEnd;

            var placedSquares = new Dictionary<(int, int), char>(placed.Count);
            foreach (var (pos, tile) in placed)
                placedSquares.Add(pos, tile);
            posStart = placed[0].pos;
            posEnd = placed[^1].pos;

            var before = game.Board.GetLettersScore(posStart, horizontal).before;
            var after = game.Board.GetLettersScore(posEnd, horizontal).after;

            var (score, times) = (0, 1);
            score = score + before.score + after.score;
            var word = before.letters;

            int start, end, hold;
            if (horizontal)
            {
                start = posStart.j;
                end = posEnd.j;
                hold = posStart.i;
            }
            else
            {
                start = posStart.i;
                end = posEnd.i;
                hold = posStart.j;
            }
            for (int iter = start; iter <= end; iter++)
            {
                (int, int) pos;
                pos = horizontal ? (hold, iter) : (iter, hold);
                char tile;
                int letScore;
                if (placedSquares.ContainsKey(pos))
                {
                    tile = placedSquares[pos];
                    letScore = game.Board.GetFaceValue(tile);
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
                }
                else
                {
                    tile = state[pos];
                    letScore = game.Board.GetFaceValue(tile);
                }
                word += tile;
                score += letScore;
            }
            word += after.letters;
            score *= times;
            if (word.Length > 1)
            {
                directionalWord = word;
                directionalScore = score;
            }
        }

        // The commented part below contains test functions that are left out in the final release

        /*private bool TestTilesSameRowCol()
        {
            // All tiles must be on the same row/col (code -1)
            int idx = horizontal ? 0 : 1;
            int temp = -1;
            foreach (var (i, j) in placedSquares.Keys)
            {
                int[] pos = { i, j };
                if (temp == -1)
                {
                    temp = pos[idx];
                    continue;
                }
                if (pos[idx] != temp)
                    return false;
            }
            return true;
        }

        private bool TestTilesPlacedOnObligatedSquares()
        {
            // Must place at least 1 tile adjacent to previously placed tiles or on center square (code -2/-3)
            var squares = game.FindObligatedSquares();
            foreach (var pos in placedSquares.Keys)
                if (squares.Contains(pos))
                    return true;
            return false;
        }

        private bool TestContinuousPlacement()
        {
            // Tiles placed along one direction must be continuous e.g. no hole (code -4)
            var state = game.Board.State;
            int i, j, j_, j_end;
            if (horizontal)
            {
                i = posStart.Item1;
                j_ = posStart.Item2;
                j_end = posEnd.Item2;
            }
            else
            {
                i = posStart.Item2;
                j_ = posStart.Item1;
                j_end = posEnd.Item1;
            }
            for (j = j_; j <= j_end; j++)
            {
                (int, int) pos;
                if (horizontal) pos = (i, j);
                else pos = (j, i);
                if (!placedSquares.ContainsKey(pos) && state[pos] == 0)
                    return false;
            }
            return true;
        }*/

        private (bool, string) TestValidWords(List<((int, int), char)> placed, bool horizontal)
        {
            // All words formed must be valid words in vocab list (code -5)
            FindDirectionalWord(placed, horizontal);
            if (directionalWord != string.Empty && !game.Board.CheckVocab(directionalWord))
                return (false, directionalWord);
            foreach (var ((i, j), tile) in placed)
            {
                var (before, after) = game.Board.GetLettersScore((i, j), !horizontal);
                var word = before.letters + tile + after.letters;
                if (word.Length > 1 && !game.Board.CheckVocab(word))
                    return (false, word);
            }
            return (true, string.Empty);
        }
    }
}
