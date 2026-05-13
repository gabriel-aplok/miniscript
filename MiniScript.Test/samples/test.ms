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

# len test
var array = [1, 2, 3, 4, 5]
print("array length: {len(array)}")

# array modification
var my_list = [1, 2, 3]
my_list[0] = 100
print("list item: {my_list[0]}") # should print 100

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
