using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Scrabble
{
    static class Program
    {
        const string url = "https://+:4001/";

        static Board board;
        static HttpListener listener = new HttpListener();

        static void Main(string[] args)
        {
            board = InitBoard();

            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine($"Listening on {url} ...");

            Task listenTask = HandleIncomingConn();
            listenTask.GetAwaiter().GetResult();
            listener.Close();
        }

        static Board InitBoard()
        {
            var words = File.ReadAllLines("vocabs.txt");
            Console.WriteLine($"Loaded {words.Length} words into memory");
            return new Board(ref words);
        }

        static async Task HandleIncomingConn()
        {
            while (true)
            {
                // Wait until there is a connection
                var ctx = await listener.GetContextAsync();

                // Peel out requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                if (req.Url.AbsolutePath != "/scrabble")
                {
                    resp.Close();
                    continue;
                }

                // Get information about the request
                string respStr;
                var reqStr = req.QueryString.Get("message");
                Console.WriteLine($"New request: {reqStr}");
                try
                {
                    respStr = ParseReq(reqStr);
                }
                catch (FormatException ex)
                {
                    respStr = ex.Message;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(respStr);
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    continue;
                }

                // Write the response
                byte[] buffer = Encoding.UTF8.GetBytes(respStr);
                resp.ContentType = "text/utf-8";
                resp.AppendHeader("Access-Control-Allow-Origin", "*");
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = buffer.LongLength;
                await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Responded: {respStr}");
                Console.ResetColor();
                resp.Close();
            }
        }

        static string ParseReq(string req)
        {
            var commandBreakdown = req.Split(' ');
            var first = commandBreakdown[0];
            string rack;

            if (first == "start" && commandBreakdown.Length == 2)
            {
                rack = commandBreakdown[1].ToUpper();
                if (rack.Length != Board.RackSize)
                    throw new FormatException($"Starting rack must have exactly {Board.RackSize} tiles");
                else if (!rack.All(c => char.IsLetter(c) || c == '?'))
                    throw new FormatException("Rack must contain only letters and blanks (?)");

                board = new Board();
            }
            else if (first == "help" && commandBreakdown.Length == 3)
            {
                var state = commandBreakdown[2];
                rack = commandBreakdown[1].ToUpper();
                const string allowedChar = "0abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

                if (rack.Length > Board.RackSize)
                    throw new FormatException($"Rack must contain no more than {Board.RackSize} tiles");
                else if (!rack.All(c => char.IsLetter(c) || c == '?'))
                    throw new FormatException("Rack must contain only letters and blanks (?)");
                else if (state.Length != Board.SizeSquare)
                    throw new FormatException($"State input must have exactly {Board.SizeSquare} characters");
                else if (!state.All(c => allowedChar.Contains(c)))
                    throw new FormatException($"Squares input format is incorrect. Try again");

                board.LoadState(state);
            }
            else
                throw new FormatException("Command not understood. Try again.");

            Move move = God.GetBestMove(board, rack);
            if (move.Score == 0)
                return "No move found.";
            else
                return move.ToString();
        }
    }
}
