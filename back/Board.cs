namespace Scrabble;

public enum SquareKind
{
    Normal,
    DL,
    TL,
    DW,
    TW,
    Start,
    Selection
}

public class Board
{
    public const int Size = 15;
    public const int SizeSquare = Size * Size;
    public const int RackSize = 7;
    public const int BingoBonus = 50;
    public const string TILES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static readonly (int row, int col) StartPos = (7, 7);

    static readonly int[] faceValues = { 1, 3, 3, 2, 1, 4, 2, 4, 1, 8, 5, 1, 3, 1, 1, 3, 10, 1, 1, 1, 1, 4, 4, 8, 4, 10 };
    static HashSet<string> vocabs;
    static bool doneInit = false;
    
    static SquareKind[] squares = new SquareKind[SizeSquare];
    public char[] State = new char[SizeSquare];

    /// <summary>
    /// Initialize static members with an array of valid Scrabble words used to lookup vocabularies, then initialize a new Board instance
    /// </summary>
    /// <param name="words">Array of Scrabble words</param>
    public Board(ref string[] words)
    {
        // Initialize vocabs list
        vocabs = new HashSet<string>(words);

        if (doneInit)
            // Done here if static members have already been initialized
            return;

        // Initialize premium squares (including starting one)
        squares[GetIdx((0, 0))] = SquareKind.TW;
        squares[GetIdx((1, 5))] = SquareKind.TL;
        squares[GetIdx((5, 1))] = SquareKind.TL;
        squares[GetIdx((5, 5))] = SquareKind.TL;
        squares[GetIdx((1, 1))] = SquareKind.DW;
        squares[GetIdx((2, 2))] = SquareKind.DW;
        squares[GetIdx((3, 3))] = SquareKind.DW;
        squares[GetIdx((4, 4))] = SquareKind.DW;
        squares[GetIdx((0, 3))] = SquareKind.DL;
        squares[GetIdx((3, 0))] = SquareKind.DL;
        squares[GetIdx((2, 6))] = SquareKind.DL;
        squares[GetIdx((6, 2))] = SquareKind.DL;
        squares[GetIdx((6, 6))] = SquareKind.DL;
        squares[GetIdx((0, 14))] = SquareKind.TW;
        squares[GetIdx((1, 9))] = SquareKind.TL;
        squares[GetIdx((5, 13))] = SquareKind.TL;
        squares[GetIdx((5, 9))] = SquareKind.TL;
        squares[GetIdx((1, 13))] = SquareKind.DW;
        squares[GetIdx((2, 12))] = SquareKind.DW;
        squares[GetIdx((3, 11))] = SquareKind.DW;
        squares[GetIdx((4, 10))] = SquareKind.DW;
        squares[GetIdx((0, 11))] = SquareKind.DL;
        squares[GetIdx((3, 14))] = SquareKind.DL;
        squares[GetIdx((2, 8))] = SquareKind.DL;
        squares[GetIdx((6, 12))] = SquareKind.DL;
        squares[GetIdx((6, 8))] = SquareKind.DL;
        squares[GetIdx((14, 14))] = SquareKind.TW;
        squares[GetIdx((13, 9))] = SquareKind.TL;
        squares[GetIdx((9, 13))] = SquareKind.TL;
        squares[GetIdx((9, 9))] = SquareKind.TL;
        squares[GetIdx((13, 13))] = SquareKind.DW;
        squares[GetIdx((12, 12))] = SquareKind.DW;
        squares[GetIdx((11, 11))] = SquareKind.DW;
        squares[GetIdx((10, 10))] = SquareKind.DW;
        squares[GetIdx((14, 11))] = SquareKind.DL;
        squares[GetIdx((11, 14))] = SquareKind.DL;
        squares[GetIdx((12, 8))] = SquareKind.DL;
        squares[GetIdx((8, 12))] = SquareKind.DL;
        squares[GetIdx((8, 8))] = SquareKind.DL;
        squares[GetIdx((14, 0))] = SquareKind.TW;
        squares[GetIdx((13, 5))] = SquareKind.TL;
        squares[GetIdx((9, 1))] = SquareKind.TL;
        squares[GetIdx((9, 5))] = SquareKind.TL;
        squares[GetIdx((13, 1))] = SquareKind.DW;
        squares[GetIdx((12, 2))] = SquareKind.DW;
        squares[GetIdx((11, 3))] = SquareKind.DW;
        squares[GetIdx((10, 4))] = SquareKind.DW;
        squares[GetIdx((14, 3))] = SquareKind.DL;
        squares[GetIdx((11, 0))] = SquareKind.DL;
        squares[GetIdx((12, 6))] = SquareKind.DL;
        squares[GetIdx((8, 2))] = SquareKind.DL;
        squares[GetIdx((8, 6))] = SquareKind.DL;
        squares[GetIdx((7, 0))] = SquareKind.TW;
        squares[GetIdx((0, 7))] = SquareKind.TW;
        squares[GetIdx((14, 7))] = SquareKind.TW;
        squares[GetIdx((7, 14))] = SquareKind.TW;
        squares[GetIdx((3, 7))] = SquareKind.DL;
        squares[GetIdx((7, 3))] = SquareKind.DL;
        squares[GetIdx((7, 11))] = SquareKind.DL;
        squares[GetIdx((11, 7))] = SquareKind.DL;
        squares[GetIdx(StartPos)] = SquareKind.Start;

        doneInit = true;
    }
    
    /// <summary>
    /// Initialize new Board instance without initializing static members
    /// </summary>
    public Board() 
    {
        if (!doneInit)
            throw new Exception("Must initiate first board with array of words");
    }

    /// <summary>
    /// Initialize a new Board instance from a currently existing one
    /// </summary>
    /// <param name="board"></param>
    public Board(Board board) => State = (char[])board.State.Clone();

    /// <summary>
    /// Get the face value of a tile
    /// </summary>
    /// <param name="tile">Tile (case-sensitive)</param>
    /// <returns>Letter's face value if capitalized (normal tiles), zero otherwise (blanks)</returns>
    public static int FaceValue(char tile) => tile >= 65 && tile <= 90 ? faceValues[tile - 65] : 0;

    /// <summary>
    /// Simply sum the face value of tiles (not taking account of premiums)
    /// </summary>
    /// <param name="tiles">Tiles whose face value to be summed</param>
    /// <returns>Sum of tiles' face values</returns>
    public static int SumFV(string tiles)
    {
        int sum = 0;
        Array.ForEach(tiles.ToCharArray(), t => sum += FaceValue(t));
        return sum;
    }

    /// <summary>
    /// Check if a word is a valid Scrabble vocabulary
    /// </summary>
    /// <param name="word">The word to be checked</param>
    /// <returns>True if the word is valid or of length 1, False otherwise</returns>
    public static bool CheckVocab(string word) => word.Length <= 1 || vocabs.Contains(word.ToUpper());

    /// <summary>
    /// Get current kind of square in a position
    /// </summary>
    /// <param name="pos">Position</param>
    /// <returns>Square's kind</returns>
    public static SquareKind Kind((int row, int col) pos) => squares[GetIdx(pos)];

    /// <summary>
    /// Get current tile in a position of board's state
    /// </summary>
    /// <param name="pos">Tuple of position's row and column</param>
    /// <returns>Current tile in position</returns>
    public char GetTile((int row, int col) pos) => State[GetIdx(pos)];

    /// <summary>
    /// Load a string-encoded state into internal memory's board state
    /// </summary>
    /// <param name="state">String-encoded state (225 characters)</param>
    public void LoadState(string state)
    {
        for (int i = 0; i < state.Length; i++)
        {
            if (state[i] == '0')
                State[i] = '\0';
            else
                State[i] = state[i];
        }
    }

    /// <summary>
    /// Transpose current board's state so that placing tiles vertically on the board is the same as 
    /// doing so horizontally on the transposed board
    /// </summary>
    public void Transpose()
    {
        char[] ts = new char[SizeSquare];
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                ts[GetIdx((r, c))] = GetTile((c, r));
        State = ts;
    }

    /// <summary>
    /// Get tiles from one side of a square as a string
    /// </summary>
    /// <param name="pos">Position of the square</param>
    /// <param name="side">Side which can be one either U, D, L or R</param>
    /// <returns>Tiles as a string in correct order (left to right or top to bottom)</returns>
    public string OneSide((int row, int col) pos, char side)
    {
        string tiles = string.Empty;

        if (side == 'U')
            for (int r = pos.row - 1; r >= 0; r--)
            {
                char t = GetTile((r, pos.col));
                if (t == 0) 
                    break;
                tiles = t + tiles;
            }
        else if (side == 'D')
            for (int r = pos.row + 1; r < Size; r++)
            {
                char t = GetTile((r, pos.col));
                if (t == 0) 
                    break;
                tiles += t;
            }
        else if (side == 'L')
            for (int c = pos.col - 1; c >= 0; c--)
            {
                char t = GetTile((pos.row, c));
                if (t == 0) 
                    break;
                tiles = t + tiles;
            }
        else if (side == 'R')
            for (int c = pos.col + 1; c < Size; c++)
            {
                char t = GetTile((pos.row, c));
                if (t == 0) 
                    break;
                tiles += t;
            }

        return tiles;
    }

    /// <summary>
    /// Get all squares from which the next move must include at least one in order to be valid
    /// </summary>
    /// <returns>Set of all required squares</returns>
    public HashSet<(int row, int col)> GetRequiredSquares()
    {
        var squares = new HashSet<(int, int)>(SizeSquare);

        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                if (GetTile((row, col)) != 0)
                {
                    var neighbors = new List<(int, int)>
                        {
                            (row - 1, col),
                            (row + 1, col),
                            (row, col - 1),
                            (row, col + 1)
                        };
                    foreach (var (r, c) in neighbors)
                        if (r >= 0 && r < Size && c >= 0 && c < Size && GetTile((r, c)) == 0)
                            squares.Add((r, c));
                }

        // If no square found, board is empty. Starting square must be included then
        if (squares.Count == 0)
            squares.Add(StartPos);

        squares.TrimExcess();
        return squares;
    }

    /// <summary>
    /// Get the next available square horizontally to place a tile based on current board state
    /// </summary>
    /// <param name="pos">Current square's position</param>
    /// <returns>The next available square, or (-1, -1) if found none</returns>
    public (int, int) NextSquare((int row, int col) pos)
    {
        for (int c = pos.col + 1; c < Size; c++)
            if (GetTile((pos.row, c)) == 0)
                return (pos.row, c);
        return (-1, -1);
    }

    /// <summary>
    /// Get the previous available square horizontally to place a tile based on current board state
    /// </summary>
    /// <param name="pos">Current square's position</param>
    /// <returns>The previous available square, or (-1, -1) if found none</returns>
    public (int, int) PrevSquare((int row, int col) pos)
    {
        for (int c = pos.col -1; c >= 0; c--)
            if (GetTile((pos.row, c)) == 0)
                return (pos.row, c);
        return (-1, -1);
    }

    /// <summary>
    /// Get the 1-dimension index number from a tuple of numbers of row and column
    /// </summary>
    /// <param name="pos">Tuple of row and column number</param>
    /// <returns>Index in 1-dimension</returns>
    public static int GetIdx((int r, int c) pos) => Size * pos.r + pos.c;
}
