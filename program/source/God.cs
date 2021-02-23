using Combinatorics.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrabble
{
    public static class God
    {
        public static Move GetBestMove(Board board, string rack)
        {
            // First create a set of all possible combinations of tiles (if there are blanks)
            var rackCombo = new HashSet<string>();
            int nbBlanks = rack.Count(c => c == '?');

            if (nbBlanks == 0)
                rackCombo.Add(rack);
            else
            {
                var alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                var nonBlanks = rack.Where(c => c != '?');
                foreach (var cb in new Combinations<char>(alpha, nbBlanks))
                {
                    var combo = cb.Concat(nonBlanks);
                    rackCombo.Add(new string(combo.ToArray()));
                }
            }
            rackCombo.TrimExcess();

            int lenRack = rack.Length;
            var required = board.GetRequiredSquares();

            if (required.Count == 1 && required.Single() == Board.StartPos)
                // Special case when board is empty and searching for the game's first move
                return FirstMove(lenRack, rackCombo);
            else
            {
                // When game has already started (there are already tiles on the board)
                // Create a transposed version of current board
                var tBoard = new Board(board);
                tBoard.Transpose();
                var tRequired = new HashSet<(int row, int col)>(required.Count);
                foreach (var (row, col) in required)
                    tRequired.Add((col, row));

                // Get best move from each rotation of the board
                var hMove = BestMove(board, lenRack, rackCombo, required);
                var vMove = BestMove(tBoard, lenRack, rackCombo, tRequired);

                // If vertical move is better than horizontal, transpose the move back to original perspective
                if (vMove.Score > 0 && vMove.Score > hMove.Score)
                {
                    var squares = new List<(int, int)>(vMove.Squares.Count);
                    vMove.Squares.ToList().ForEach(pos => squares.Add((pos.col, pos.row)));
                    return new Move
                    {
                        Squares = squares,
                        Tiles = vMove.Tiles,
                        Horizontal = false,
                        Score = vMove.Score
                    };
                }
                else
                    return hMove;
            }
        }

        static Move FirstMove(int lenRack, HashSet<string> rackCombo)
        {
            // Construct the start row to put letters
            var startRow = new List<(int, int)> { Board.StartPos };
            for (int i = 1; i < lenRack; i++)
            {
                startRow.Insert(0, (Board.StartPos.row, Board.StartPos.col - i));
                startRow.Add((Board.StartPos.row, Board.StartPos.col + i));
            }
            int cIdx = Board.RackSize - 1;

            Move best = new Move();
            for (int nbTiles = 2; nbTiles <= lenRack; nbTiles++)
                for (int i = Math.Max(0, cIdx - nbTiles + 1); i <= cIdx && i <= startRow.Count - nbTiles; i++)
                {
                    var squares = startRow.GetRange(i, nbTiles);
                    var tried = new HashSet<string>();
                    foreach (string rack in rackCombo)
                        foreach (var combo in new Combinations<char>(rack.ToCharArray(), nbTiles))
                            foreach (var permu in new Permutations<char>(combo))
                            {
                                var word = new string(permu.ToArray());
                                if (Board.CheckVocab(word) && tried.Add(word))
                                {
                                    int score = 0, mul = 1;
                                    for (int j = 0; j < nbTiles; j++)
                                    {
                                        var (tile, pos) = (permu[j], squares[j]);
                                        var fv = Board.FaceValue(tile);
                                        if (Board.Kind(pos) == SquareKind.DL)
                                            fv *= 2;
                                        else if (Board.Kind(pos) == SquareKind.TL)
                                            fv *= 3;
                                        else if (Board.Kind(pos) == SquareKind.DW || pos == Board.StartPos)
                                            mul *= 2;
                                        else if (Board.Kind(pos) == SquareKind.TW)
                                            mul *= 3;
                                        score += fv;
                                        
                                    }
                                    score *= mul;
                                    if (nbTiles == Board.RackSize)
                                        score += Board.BingoBonus;

                                    var move = new Move
                                    {
                                        Squares = squares,
                                        Tiles = permu,
                                        Horizontal = true,
                                        Score = score
                                    };
                                    if (move.Score > best.Score)
                                        best = move;
                                }
                            }
                }
            return best;
        }

        static Move BestMove(Board board, int lenRack, HashSet<string> rackCombo, HashSet<(int row, int col)> required)
        {
            // First construct a table of valid moves by position and tiles
            var tilesToTest = rackCombo.Count == 1
                ? new HashSet<char>(rackCombo.Single())
                : new HashSet<char>(Board.TILES);

            var valid = new Dictionary<(int, int), HashSet<char>>(required.Count);
            foreach (var pos in required)
            {
                valid[pos] = new HashSet<char>();
                foreach (char tile in tilesToTest)
                {
                    // Testing if vertical word is valid
                    var vWord = board.OneSide(pos, 'U') + tile + board.OneSide(pos, 'D');
                    if (Board.CheckVocab(vWord))
                    {
                        valid[pos].Add(tile);
                        valid[pos].Add(char.ToLower(tile));
                    }
                }
            }

            Move best = new Move();

            // Iterate over every required squares and attempt placing tiles horizontally
            foreach (var pos in required)
            {
                var row = new List<(int row, int col)> { pos };
                bool goL = true, goR = true;
                int anchorIdx = 0;
                for (int i = 0; i < lenRack - 1; i++)
                {
                    if (!goL && !goR)
                        break;
                    if (goL)
                    {
                        var prev = board.PrevSquare(row[0]);
                        if (prev == (-1, -1))
                            goL = false;
                        else
                        {
                            row.Insert(0, prev);
                            anchorIdx++;
                        }
                    }
                    if (goR)
                    {
                        var next = board.NextSquare(row.Last());
                        if (next == (-1, -1))
                            goR = false;
                        else
                            row.Append(next);
                    }
                }
                int lenRow = row.Count;

                for (int nbTiles = 1; nbTiles <= lenRack; nbTiles++)
                    for (int i = Math.Max(0, anchorIdx - nbTiles + 1); i <= anchorIdx && i <= lenRow - nbTiles; i++)
                    {
                        var squares = row.GetRange(i, nbTiles);
                        var validIdx = squares.FindAll(sq => valid.ContainsKey(sq)).Select(sq => squares.IndexOf(sq));
                        var tried = new HashSet<string>();
                        foreach (string rack in rackCombo)
                            foreach (var combo in new Combinations<char>(rack.ToCharArray(), nbTiles))
                                foreach (var permu in new Permutations<char>(combo))
                                    if (tried.Add(new string(permu.ToArray())) 
                                        && validIdx.All(idx => valid[squares[idx]].Contains(permu[idx])))
                                    {
                                        var move = new Move
                                        {
                                            Squares = squares,
                                            Tiles = permu,
                                            Horizontal = true
                                        };
                                        move.Evaluate(board);
                                        if (move.Score > best.Score)
                                            best = move;
                                    }
                    }
            }
            return best;
        }
    }
}
