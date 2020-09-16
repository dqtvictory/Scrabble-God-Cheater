using System;
using System.IO;
using System.Collections.Generic;

namespace Scrabble
{
    public class ScrabbleBoard
    {   
        private Dictionary<char, int> faceValues = new Dictionary<char, int>();
        private Dictionary<char, int> faceCounts = new Dictionary<char, int>();
        private Dictionary<int, HashSet<string>> vocabs = new Dictionary<int, HashSet<string>>();
        private Dictionary<(int, int), char> state = new Dictionary<(int, int), char>(225);
        private Dictionary<(int, int), string> premiums = new Dictionary<(int, int), string>();
        public const int Size = 15;
        public Dictionary<(int, int), char> State { get => state; }
        public readonly (int, int) StartPos = (7, 7);
        public Dictionary<(int, int), string> Premiums { get => premiums; }
        public ScrabbleBoard()
        {
            Console.WriteLine("Initializing Scrabble Board...");
            // Initialize vocab list
            Console.Write("Initializing vocabs... ");
            var words = File.ReadAllLines(@"vocabs.txt");
            foreach (var word in words)
            {
                var len = word.Length;
                if (!vocabs.ContainsKey(len))
                {
                    vocabs.Add(len, new HashSet<string>());
                }
                vocabs[len].Add(word);
            }
            vocabs.TrimExcess();
            Console.WriteLine("Vocabs loaded into memory");

            // Initialize tiles' face values dictionary
            Console.Write("Initializing face values and face counts... ");
            const string tiles = "ABCDEFGHIJKLMNOPQRSTUVWXYZ?";
            int[] values = { 1, 3, 3, 2, 1, 4, 2, 4, 1, 8, 5, 1, 3, 1, 1, 3, 10, 1, 1, 1, 1, 4, 4, 8, 4, 10, 0 };
            int[] counts = { 9, 2, 2, 4, 12, 2, 3, 2, 9, 1, 1, 4, 2, 6, 8, 2, 1, 6, 4, 6, 4, 2, 2, 1, 2, 1, 2 };
            for (int i = 0; i < tiles.Length; i++)
            {
                faceValues.Add(tiles[i], values[i]);
                faceCounts.Add(tiles[i], counts[i]);
            }
            faceValues.TrimExcess();
            Console.WriteLine("Face values and counts initialized");

            // Initialize board's state
            Console.Write("Initializing initial game state... ");
            InitializeBoard(true);
            Console.WriteLine("Initializing initial game state... ");

            // Initialize premium squares
            Console.Write("Initializing premimum squares... ");
            premiums.Add((0, 0), "TW");
            premiums.Add((1, 5), "TL");
            premiums.Add((5, 1), "TL");
            premiums.Add((5, 5), "TL");
            premiums.Add((1, 1), "DW");
            premiums.Add((2, 2), "DW");
            premiums.Add((3, 3), "DW");
            premiums.Add((4, 4), "DW");
            premiums.Add((0, 3), "DL");
            premiums.Add((3, 0), "DL");
            premiums.Add((2, 6), "DL");
            premiums.Add((6, 2), "DL");
            premiums.Add((6, 6), "DL");
            premiums.Add((0, 14), "TW");
            premiums.Add((1, 9), "TL");
            premiums.Add((5, 13), "TL");
            premiums.Add((5, 9), "TL");
            premiums.Add((1, 13), "DW");
            premiums.Add((2, 12), "DW");
            premiums.Add((3, 11), "DW");
            premiums.Add((4, 10), "DW");
            premiums.Add((0, 11), "DL");
            premiums.Add((3, 14), "DL");
            premiums.Add((2, 8), "DL");
            premiums.Add((6, 12), "DL");
            premiums.Add((6, 8), "DL");
            premiums.Add((14, 14), "TW");
            premiums.Add((13, 9), "TL");
            premiums.Add((9, 13), "TL");
            premiums.Add((9, 9), "TL");
            premiums.Add((13, 13), "DW");
            premiums.Add((12, 12), "DW");
            premiums.Add((11, 11), "DW");
            premiums.Add((10, 10), "DW");
            premiums.Add((14, 11), "DL");
            premiums.Add((11, 14), "DL");
            premiums.Add((12, 8), "DL");
            premiums.Add((8, 12), "DL");
            premiums.Add((8, 8), "DL");
            premiums.Add((14, 0), "TW");
            premiums.Add((13, 5), "TL");
            premiums.Add((9, 1), "TL");
            premiums.Add((9, 5), "TL");
            premiums.Add((13, 1), "DW");
            premiums.Add((12, 2), "DW");
            premiums.Add((11, 3), "DW");
            premiums.Add((10, 4), "DW");
            premiums.Add((14, 3), "DL");
            premiums.Add((11, 0), "DL");
            premiums.Add((12, 6), "DL");
            premiums.Add((8, 2), "DL");
            premiums.Add((8, 6), "DL");
            premiums.Add((7, 0), "TW");
            premiums.Add((0, 7), "TW");
            premiums.Add((14, 7), "TW");
            premiums.Add((7, 14), "TW");
            premiums.Add((3, 7), "DL");
            premiums.Add((7, 3), "DL");
            premiums.Add((7, 11), "DL");
            premiums.Add((11, 7), "DL");
            premiums.TrimExcess();
            Console.WriteLine("Premium squares initialized");
        }

        public void InitializeBoard(bool init)
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                {
                    if (init)
                        state.Add((i, j), '\0');
                    else
                        state[(i, j)] = '\0';
                }
        }

        public int GetFaceValue(char c)
        {
            if (65 <= c && c <= 90)
                return faceValues[c];
            return 0;
        }

        public int GetFaceCount(char c)
        {
            return faceCounts[c];
        }

        public Dictionary<(int, int), char> GetState()
        {
            return state;
        }

        public void UpdateState(Dictionary<(int, int), char> update)
        {
            foreach (var pos in update.Keys)
                state[pos] = update[pos];
        }

        public bool CheckVocab(string vocab)
        {
            return vocabs[vocab.Length].Contains(vocab.ToUpper());
        }

        public ((string letters, int score) before, (string letters, int score) after) GetLettersScore((int i, int j) pos, bool horizontal)
        {
            ((string letters, int score) before, (string letters, int score) after) = ((string.Empty, 0), (string.Empty, 0));
            var (i, j, j_) = horizontal ? (pos.i, pos.j, pos.j) : (pos.j, pos.i, pos.i);

            // Iterate before pos
            j = j_ - 1;
            while (j >= 0)
            {
                var check_pos = horizontal ? (i, j) : (j, i);
                if (state[check_pos] == 0)
                    break;
                before.letters = state[check_pos] + before.letters;
                before.score += GetFaceValue(state[check_pos]);
                j--;
            }

            // Iterate after pos
            j = j_ + 1;
            while (j < Size)
            {
                var check_pos = horizontal ? (i, j) : (j, i);
                if (state[check_pos] == 0)
                    break;
                after.letters += state[check_pos];
                after.score += GetFaceValue(state[check_pos]);
                j++;
            }
            return (before, after);
        }
    }
}
