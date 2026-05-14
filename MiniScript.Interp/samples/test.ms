# ----------------------------------------------------
# I'm just testing out what I'm adding, sorry for the mass of random things.
# ----------------------------------------------------

import "utils.ms" # I put samples/ cuz the miniscript executor doesn't recognize the relative path and the script isn't in the same folder.

print(greet("Gabriel"))
print("Running version: {version}")

# data types and variables
var integer_number = 10
var decimal_number = 20.5
var text = "hello world"
var boolean_true = true
var boolean_false = false
var null_value = null
var name = "Gabriel"
var score = 1337

print("--- data types test ---")
print(integer_number)
print(decimal_number)
print(text)
print(boolean_true)
print(null_value)

print("--- string interpolation test ---")
print("Hello {name}!")
print("Your score is {score}")
print("Next level in {10000 - score} points")
print("Complex: {name} has {score * 2} points")

# mathematical operations
print("--- math test ---")
var sum = 5 + 5
var subtraction = 10 - 2
var multiplication = 4 * 2
var division = 20 / 2
var modulo = 10 % 3
var negative = -5

print(sum)
print(subtraction)
print(multiplication)
print(division)
print(modulo)
print(negative)

# comparisons
print("--- comparisons test ---")
print(10 > 5)
print(10 < 5)
print(10 == 10)
print(10 != 5)
print(5 >= 5)
print(3 <= 10)

# control structures (if/else)
print("--- conditionals test ---")
var age = 18
if age >= 18:
    print("adult")
else:
    print("minor")

# loops (while)
print("--- while loop test ---")
var counter = 0
while counter < 3:
    print("loop iteration:")
    print(counter)
    counter = counter + 1

# functions and scope
print("--- functions test ---")
func calculate_area(width, height):
    var area = width * height
    return area

var area_result = calculate_area(5, 4)
print("calculated area is:")
print(area_result)

# simple recursion (fibonacci)
func fib(n):
    if n <= 1:
        return n
    return fib(n - 1) + fib(n - 2)

print("fibonacci of 5:")
print(fib(5))

# built-in functions
print("--- current time ---")
print(clock())

# print("--- input ---")
# print("enter your name:")
# var name = input()
# print("hello " + name)


# ----------------------------------------------------
# logical operators test
# ----------------------------------------------------
print("--- logical operators test ---")

var age = 20
var has_ticket = true
var is_vip = false
var is_staff = false

var can_enter = age >= 18 and has_ticket
var free_entry = is_vip or is_staff
var restricted = not can_enter

if can_enter or free_entry:
    print("welcome!")


# ----------------------------------------------------
# arrays and lists test
# ----------------------------------------------------
print("--- arrays/lists index access test ---")

var fruits = ["apple", "banana", "cherry"]
var scores = [10, 20, 30 + 5]

print("first fruit: {fruits[0]}")
print("second score: {scores[1]}")

var i = 2
print("dynamic index: {fruits[i]}")

# nesting test
var matrix = [[1, 2], [3, 4]]
print("matrix 0,0: {matrix[0][0]}")
print("matrix 1,0: {matrix[1][0]}")

# array modification
var my_list = [1, 2, 3]
my_list[0] = 100
print("list item: {my_list[0]}") # should print 100


# ----------------------------------------------------
# dictionaries test
# ----------------------------------------------------
print("--- dictionaries test ---")

# dictionary usage
var player = {
    "name": "gabriel",
    "health": 100,
    "inventory": ["item1", "item2", "item3"]
}

print("welcome {player['name']}")

# dictionary modification
player["health"] = player["health"] - 20
print("current health: {player['health']}")

# adding new keys
player["mana"] = 50
print("mana added: {player['mana']}")


# ----------------------------------------------------
# for loops test
# ----------------------------------------------------
print("--- for loops test ---")

# iterating over an array
var heroes = ["batman", "superman", "wonder woman"]
for hero in heroes:
    print("Hero: {hero}")

# iterating over a dictionary (iterates keys)
var stats = {
    "strength": 99,
    "agility": 85,
    "intelligence": 90
}

print("Stats:")
for key in stats:
    print("- {key}: {stats[key]}")

# iterating over a string
var word = "mini"
for letter in word:
    print("Letter: {letter}")


# ----------------------------------------------------
# standard library tests
# ----------------------------------------------------
print("--- math library test ---")
print("PI: {math['pi']}")
print("Absolute: {math['abs'](-15)}")
print("Floor: {math['floor'](4.8)}")
print("Ceil: {math['ceil'](4.2)}")
print("Pow: {math['pow'](2, 3)}")

print("--- string library test ---")
var text = "  MiniScript is awesome!  "

print("Original: '{text}'")
print("Trimmed: '{string['trim'](text)}'")
print("Upper: {string['to_upper'](text)}")

var clean_text = string['trim'](text)
print("Replace: {string['replace'](clean_text, 'awesome', 'powerful')}")
print("Contains 'Script': {string['contains'](clean_text, 'Script')}")

# split returns an array, so we can iterate over it!
print("--- split test ---")
var words = string['split'](clean_text, " ")
for word in words:
    print("Word: {word}")


# ----------------------------------------------------
# high level stdlib tests
# ----------------------------------------------------
print("--- high level stdlib test ---")

var text = "  hello world  "
var clean = string.trim(text)
print(string.to_upper(clean))

var radius = 5
var area = math.pi * math.pow(radius, 2)
print("Area: {area}")

# can even mix them
var items = string.split(string.trim("a, b, c"), ", ")
for item in items:
    print("Item: {item}")


# ----------------------------------------------------
# dynamic member test
# ----------------------------------------------------
print("--- dynamic member test ---")

# creating an array
var inventory = ["sword", "shield"]
print("Initial length: {inventory.length}")

# adding items
inventory.push("health potion")
print("New length: {inventory.length}")

# iterating over an array
for item in inventory:
    print("Item: {item}")

# removing items
var removed = inventory.pop()
print("Popped: {removed}")

# method chaining
var name = "  Gabriel Aplok  "
print(name.trim().to_upper())

# dynamic member access
var player = {"score": 0}
player.score = player.score + 50
print("Score: {player.score}")


# ----------------------------------------------------
# try/catch test
# ----------------------------------------------------
print("--- try/catch test ---")

try:
    var result = 10 / 0 # this should generate an error.
    print(result)
catch e:
    print("an error occurred: {e}")

try:
    import "unexistent.ms"
catch err:
    print("Import error: {err}")


func check_level(lvl):
    if lvl < 0:
        throw "Level cannot be negative!"
    return "Level is {lvl}"

try:
    print(check_level(-5)) # -5 to trigger error
catch e:
    print("caught an error: {e}")
finally:
    print("this will always run, no matter what.")

# try without catch, only with finally.
try:
    print("doing something dangerous...")
finally:
    print("cleaning up resources...")


print("the program continued running fine!")


# ----------------------------------------------------
# OS & IO tests
# ----------------------------------------------------
print("--- OS & IO Test ---")

# getting env vars
var user = os.getenv("USERNAME")
if user == null:
    user = os.getenv("USER") # Linux/Mac support

print("hello, system user: {user}")
print("platform: {os.platform}")
print("script directory: {os.cwd()}")

# working with files
var filename = "data.txt"
var content = "this is a secret message for {user}.\n"

print("writing to {filename}...")
io.write(filename, content)

if io.exists(filename):
    print("file content:")
    print(io.read(filename))

    # append
    io.append(filename, "added at: {clock()}\n")
    print("updated content:")
    print(io.read(filename))

# error handling
# try:
#     io.read("non_existent_file.txt")
# catch e:
#     print("Safe! Caught IO error: {e")
