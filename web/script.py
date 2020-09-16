from browser import document, html, bind, alert


SIZE = 15

COLORS = {  # HTML color for each premium squares
    "TW": "#DB1B1B",
    "DW": "#F68787",
    "TL": "#25A9DC",
    "DL": "#87D7F6",
    "start": "#25DC88",
    "white": "#DDE0DE",
    "current": "#EEC14A"
}
BOARD = dict()  # Default color of each square on the game board
PREMIUMS = dict() # Premiums squares on the board
state = dict()

# Initialize dictionary of premium squares
def init_premiums():
    global PREMIUMS
    q1 = {  # premiums in the board's 1st quadrant
        (0, 0): "TW",
        (1, 5): "TL", (5, 1): "TL", (5, 5): "TL",
        (1, 1): "DW", (2, 2): "DW", (3, 3): "DW", (4, 4): "DW",
        (0, 3): "DL", (3, 0): "DL", (2, 6): "DL", (6, 2): "DL", (6, 6): "DL"
    }
    PREMIUMS = q1.copy()
    for i, j in q1:
        PREMIUMS[(i, 14-j)] = q1[(i, j)]  # same row, opposite column (q2)
        PREMIUMS[(14-i, 14-j)] = q1[(i, j)]  # opposite row, opposite column (q3)
        PREMIUMS[(14-i, j)] = q1[(i, j)]  # opposite row, same column (q4)
    
    premiums_axes = {
        (7, 0): "TW", (0, 7): "TW", (14, 7): "TW", (7, 14): "TW",
        (3, 7): "DL", (7, 3): "DL", (7, 11): "DL", (11, 7): "DL",
    }
    PREMIUMS.update(premiums_axes)
init_premiums()

# Initialize HTML game board & game state
def init():
    global state
    start_pos = (7, 7)
    for i in range(SIZE):
        row = html.TR()
        for j in range(SIZE):
            state[(i, j)] = None  # Initialize game state
            square = html.TD('', id=f"square_{i}_{j}", Class="square")
            if (i, j) in PREMIUMS:
                kind = PREMIUMS[(i, j)]
            elif (i, j) == start_pos:
                kind = "start"
            else:
                kind = "white"
            square.style.backgroundColor = COLORS[kind]
            BOARD[(i, j)] = COLORS[kind]
            row <= square
        document["game-board"] <= row
init()


current = None
typing_blank = False

def get_square_from_pos(pos):
    i, j = pos
    return document[f"square_{i}_{j}"]

def set_square_border(pos, border):
    square = get_square_from_pos(pos)
    if border:
        square.style.border = "1px solid black"
    else:
        square.style.border = "1px solid white"

def set_square_bg_img(pos, img_path):
    square = get_square_from_pos(pos)
    if img_path:
        square.style.backgroundImage = f"url({img_path})"
    else:
        square.style.backgroundImage = ""

def make_square_current(pos):
    global current
    current = pos
    square = get_square_from_pos(current)
    square.style.backgroundColor = COLORS["current"]
    set_square_border(current, True)

def unmake_current():
    global current
    pos = current
    current = None
    square = get_square_from_pos(pos)
    square.style.backgroundColor = BOARD[pos]
    set_square_border(pos, False)

def change_current(direction):
    i, j = current
    if direction == "up":
        if i == 0:
            return
        pos = (i - 1, j)
    elif direction == "down":
        if i == SIZE - 1:
            return
        pos = (i + 1, j)
    elif direction == "left":
        if j == 0:
            return
        pos = (i, j - 1)
    elif direction == "right":
        if j == SIZE - 1:
            return
        pos = (i, j + 1)
    unmake_current()
    make_square_current(pos)


def type_letter(letter):
    global state
    if typing_blank:
        img_path = f"tiles/blanks/{letter}.jpg"
        letter = letter.lower()
    else:
        img_path = f"tiles/{letter}.jpg"
    set_square_bg_img(current, img_path)
    state[current] = letter

def del_letter():
    global state
    if not state[current]:
        return
    set_square_bg_img(current, None)
    state[current] = None

@bind("td.square", "click")
def square_click(e):
    global current
    square = e.currentTarget
    i, j = map(int, square["id"].split("_")[1:])
    if not current:
        make_square_current((i, j))
    elif current != (i, j):
        unmake_current()
        make_square_current((i, j))
    elif current == (i, j):
        unmake_current()

inputting = False

@bind(document, "keydown")
def keydown(e):
    if inputting:
        return
    global typing_blank
    key = e.which
    if key in range(65, 91) and current:  # type a letter
        letter = str.upper(chr(key))
        type_letter(letter)
        typing_blank = False
    elif key in (8, 46):  # backspace or delete
        del_letter()
        typing_blank = False
    elif not typing_blank:
        if key == 191:  # type blank with ? key
            typing_blank = True
            img_path = f"tiles/blanks/_.jpg"
            set_square_bg_img(current, img_path)
            # set_square_border(current, True)
        elif key == 37 and current:  # left arrow key
            change_current("left")
        elif key == 38 and current:  # up arrow key
            change_current("up")
        elif key == 39 and current:  # right arrow key
            change_current("right")
        elif key == 40 and current:  # down arrow key
            change_current("down")


@bind(document["rack-letters"], "input")
def input_change(e):
    value = e.currentTarget.value
    if len(value) > 0:
        last_char = ord(value[-1])
        if last_char != 63 and last_char not in range(65, 91) and last_char not in range(97, 123):
            value = value[:-1]
    e.currentTarget.value = str.upper(value)

@bind(".input-area", "focus")
def input_focus(e):
    global inputting
    inputting = True

@bind(".input-area", "blur")
def input_blur(e):
    global inputting
    inputting = False

def print_in_textarea(text):
    text_area = document["status-text"]
    text_area.value = text

@bind(document["load-state"], "click")
def load_state(e):
    global state, current
    value = document["status-text"].value
    buffer_state = dict()
    if len(value) != SIZE**2:
         alert(f"Format error: State input must have exactly {SIZE**2} characters. {len(value)} characters found.")
         return
    for n in range(SIZE**2):
        if not str.isalpha(value[n]) and value[n] != '0':
            alert("Format error: State input is not correctly formatted (only letters and digit 0 are allowed)")
            return
        i, j = n // SIZE, n % SIZE
        if value[n] == '0':
            buffer_state[(i, j)] = None
        else:
            buffer_state[(i, j)] = value[n]
    if current:
        unmake_current()
    state = buffer_state
    for pos in state:
        letter = state[pos]
        if not letter:
            img_path = None
        elif str.isupper(letter):
            img_path = f"tiles/{letter}.jpg"
        else:
            img_path = f"tiles/blanks/{str.upper(letter)}.jpg"
        set_square_bg_img(pos, img_path)
            

@bind(document["generate-state"], "click")
def generate_state(e):
    text = ""
    for i in range(SIZE):
        for j in range(SIZE):
            if not state[(i, j)]:
                text += '0'
            else:
                text += state[(i, j)]
    print_in_textarea(text)


@bind(document["clear-board"], "click")
def clear_board(e):
    for i in range(SIZE):
        for j in range(SIZE):
            if not state[(i, j)]:
                continue
            state[(i, j)] = None
            set_square_bg_img((i, j), None)

@bind(document["generate-command"], "click")
def generate_command(e):
    for pos in state:
        if state[pos]:
            started = True
            break
    else:
        started = False
    rack = document["rack-letters"].value
    if not started and len(rack) != 7:
        alert("Must have 7 tiles in rack since board is empty")
        return
    elif not started:
        print_in_textarea(f"start {rack}")
    else:
        command = f"help {rack} "
        for i in range(SIZE):
            for j in range(SIZE):
                if not state[(i, j)]:
                    c = '0'
                elif state[(i, j)][0] == '?':
                    c = str.lower(state[(i, j)][-1])
                else:
                    c = state[(i, j)]
                command += f"{c}"
        print_in_textarea(command)