<h1>Scrabble God</h1>

The cheat engine for the Scrabble game. This program searches for the best move possible from a specific board configuration using optimized trial-and-error method. While the time complexity can sometimes be large, the program as well as the .NET runtime itself knows when to cache previously found results and produce very fast performance, even when the board is complex and user's rack has two wildcards (each wildcard requires 26 times more tries for each English letter).

<h2>How-to:</h2>
This project consists of 2 parts: the back-end and the front-end.

- The <b>front-end</b> coded in simple HTML and Python using <a href="https://www.brython.info/">Brython</a>, a Javascript's alternative for web scripting. This part provides a graphical Scrabble board in which user can input a game state and the tiles available to play the next move. By <i>calling the Scrabble God</i>, the page sends a HTTP request with the full board's state as well as user's tiles to the back-end for processing. Once a respond is received, it is displayed to the large textbox which details which letters to pick, where to play, and the potential score obtained.
- The <b>back-end</b> coded in C#, must be ran seperately in the terminal. It waits patiently for the right HTTP request to proceed to searching the best solution given a board and a tile rack. By default, it opens and listens on port 4000, but can be configurable in `Program.cs` and must be recompiled once changed. Similarly, the `script.py` file on the front-end part contains the URL and port of the back-end that need to be changed accordingly.

<b><i>Notes:</i></b> `System.Net.HttpListener` is currently not functionnal with HTTPS request when listening to a non-local IP address (i.e. 127.0.0.1 or localhost). Furthermore, this class doesn't seem to work on Ubuntu listening to a non-local IP address. I haven't tried on other Linux systems. For now I can only confirm that it works fine with non-SSL requests on Windows.

<h2>Update in 2022:</h2>
This project was last updated nearly 2 years ago. Since then, the C# language and the `Combinatorics` package (on which this project depends) have evolved. I have updated the repo to make the program work again in 2022.

On the front end, since the webpage loads Brython's script directly from its website, it too has evolved which causes some UI elements to break on latest version of Google Chrome. However Microsoft Edge, for some reasons, still displays the page correctly. I haven't tested on other browsers.
