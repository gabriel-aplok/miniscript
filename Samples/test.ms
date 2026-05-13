# data types and variables
var integer_number = 10
var decimal_number = 20.5
var text = "hello world"
var boolean_true = true
var boolean_false = false
var null_value = null

print("--- data types test ---")
print(integer_number)
print(decimal_number)
print(text)
print(boolean_true)
print(null_value)

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
