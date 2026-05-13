# a simple miniscript program
func add(a, b):
	return a + b


func fibonacci(n):
	if n <= 1:
		return n
	return fibonacci(n - 1) + fibonacci(n - 2)

print("Testing Addition:")
print(add(5, 7))

print("Testing Fibonacci:")
print(fibonacci(7))

print("Testing While Loop:")
var i = 0
while i < 5:
	print("Loop iteration: " + i)
	i = i + 1
