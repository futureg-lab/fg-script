# fg-script
FGScript lang implementation in C#

# fg-script lang overview

```c
// import function from csharp to a fg-script
@extern requestHtml(str)
@extern requestHttpJSON(str)

// expose a function to the host language
@expose fn sayHello(name) {
    "Hello World! " + name
}

// a value, a tuple, an array, a dictionary, everything is a tuple !

some_num = 5
some_num2 = 3.14
some_str = "Hello World"

tuple = (1, 2, 3, 4, 5, (1, 2 ("something", 2)))

// a dic is a tuple, with a label on each item
example_dic = (
	a : 6,
	b : 8,
	c : (
		e : "Some text",
		h : 3,
		q : (1, 2, 3, 4, 5)
	)
)

fn fibo (n) {
	// return is implicit
	if n <= 2 {
		1
	}
	fibo (n - 1) + fibo (n - 2)
}

fn main {
	for i = 0 .. 10 {
		print("index " + i)
	}
	
	for (i, val) : (1, 2, 3, 4) {
		print("index " + i + ":" + val + "\n")
	}
	
	for (key, value) : example_dic {
		if type(value) is "tuple" {
			print("Raw value at " + key + " : " + value)
		}
	}
	
	some_data = requestHttpJSON()
	
	cond_expr = false
	while cond_expr {
		do_stuff()
	}
}

main ();
```