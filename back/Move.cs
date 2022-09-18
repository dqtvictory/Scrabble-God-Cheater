namespace Scrabble;

public class Move
{
    public IList<(int row, int col)> Squares;
    public IList<char> Tiles;
    public bool Horizontal;
    public int Score = 0;
    public bool IsBingo => Tiles.Count == Board.RackSize;

    /// <summary>
    /// Assuming tiles for the move form valid words vertically, calculate move's score.
    /// This method assumes tiles are placed horizontally
    /// </summary>
    /// <param name="board">The current board</param>
    public void Evaluate(Board board)
    {
        var (rowStart, rowEnd) = (Squares.First(), Squares.Last());

        // Init main word with all the tiles before the starting pos
        string mainWord = board.OneSide(rowStart, 'L');
        int mainScore = Board.SumFV(mainWord);
        int sideScore = 0;
        int mainMul = 1;

        // Iterating squares along the main word
        for (int c = rowStart.col; c <= rowEnd.col; c++)
        {
            char tile;
            (int row, int col) pos = (rowStart.row, c);
            int i = Squares.IndexOf(pos);

            if (i > -1)
            {
                // This square is in the move list
                tile = Tiles[i];
                string tb = board.OneSide(pos, 'U'), ta = board.OneSide(pos, 'D');
                string vWord = tb + tile + ta;
                int sb = Board.SumFV(tb), sa = Board.SumFV(ta);

                // Side word is already validated, calculate its score
                var kind = Board.Kind(pos);
                int fv = Board.FaceValue(tile);
                int mul = vWord.Length == 1 ? 0 : 1;

                if (kind == SquareKind.DL)
                    fv *= 2;
                else if (kind == SquareKind.TL)
                    fv *= 3;
                else if (kind == SquareKind.DW)
                {
                    mul *= 2;
                    mainMul *= 2;
                }
                else if (kind == SquareKind.TW)
                {
                    mul *= 3;
                    mainMul *= 3;
                }
                sideScore += mul * (sb + sa + fv);
                mainScore += fv;
            }
            else
            {
                // This square is not in the move list, check the tile in the current board's state
                tile = board.GetTile(pos);
                mainScore += Board.FaceValue(tile);
            }
            mainWord += tile;
        }

        // Finalize main word then check if it's valid
        string ma = board.OneSide(rowEnd, 'R');
        mainWord += ma;
        if (mainWord.Length == 1)
            mainScore = 0;
        else if (!Board.CheckVocab(mainWord))
            return;
        else
            mainScore += Board.SumFV(ma);

        // At this point all words formed have been validated, now calculate the final score
        int b = IsBingo ? Board.BingoBonus : 0;
        Score = sideScore + (mainMul * mainScore) + b;
    }

    public override string ToString()
    {
        string move = $"{Score}\n{new string(Tiles.ToArray())}\n";
        move += string.Join("\n", Squares);
        return move;
    }
}
