# fg-script
FGScript lang implementation in C#

# Building and Testing
## CLI (Linux, Windows)
```bash
dotnet test
dotnet build --configuration Release --no-restore
```

## Visual Studio (Windows)
The project solution is located at the root directory

# fg-script lang overview

## Example 1 : fg-script tuple to json
```rust
fn to_json(auto value) -> str {
	if repr_of value is "tup" {
		str tmp = "{";
		num size = len(value);
		num i = 0;
		for (k, v) in value {
			str row = "\"" + k + "\""+ " : " + to_json(v);
			tmp = tmp + row;
			if i + 1 < size {
				tmp = tmp + ",";
			}
			i = i + 1;
		}
		tmp = tmp + "}";
		ret tmp;
	}
	if repr_of value is "str" {
		ret "\"" + value + "\"";
	}
	ret to_str(value);
}
```

## Example 2 : Fibonacci recursive
```rust
fn fib(num x) -> num {
	if x < 0 {
		ret 0;
	}
	if x < 1 {
		ret 1;
	}
	ret fib(x - 1) + fib(x - 2);
}
```

## Example 3 : fg-script features
```rust
// expose a function to the host language
expose fn sayHello(name) -> str {
    ret "Hello World! " + name;
}

// Native types : str, num (double representation), tup
// a value, a tuple, an array, a dictionary, everything is a tuple !

tup tuple = [1, 2, 3, 4, 5, [1, 2 ["something", 2]]];
tup tup_empt = [];
tpush(tup_empt, 1);
tpush(tup_empt, 2);
tpush(tup_empt, "3");
tshift(tup_empt); // remove first item "1" and return the modified tuple pointer 
tpop(tup_empt); // remove last item "3" and return the modified tuple pointer

// using tuples as a dynamic array
tup arr = []; // add a flags that enforces number indexing
arr[0] = 1;
arr[1] = 2;
// arr[2] = 3 // error
println(len(arr)); // 2
println(repr_of arr); // tup
println(repr_of arr == "tup"); // true

// string
str some_str = "Hello World";

num some_num = 5;
tup as_tup = some_num; // [5] no need to cast since it's already a tuple
str as_str = to_str(some_num); // stringify
str as_num = to_num(as_str); // numberify
tup as_tup_again = to_tup(as_tup); // [[5]]
tup as_tup_again = to_tup(as_str); // ["5"]

// operation with tuples with the same dimension*
tup vec1 = [1, 2, 3];
tup vec2 = [4, "5", 6];
tup res1 = vec1 + vec2; // [1, "25", 9]
tup res2 = vec1 * vec2; // * does not support operands num, str
tup res3 = vec1 - vec2; // - does not support operands num, str
tup res4 = vec1 / vec2; // / does not support operands num, str

// a dic is a tuple, with a label on each item
tup example_dic = [
	a : 6,
	b : 8,
	c :  [
		e : "Some text",
		h : 3,
		q : [1, 2, 3, 4, 5]
	]
];

// In this example [3] should be labeled with an explicit key
tup example = [a : 1, b : 2, [3]]; // ":" was expected, got LEFT_BRACKET instead

// type inference (!= generic, acts the same as auto in C++)
auto a1 = [1, 2];
auto a2 = "hello world";

fn main -> void {
	for i in 0 .. 10 {
		println("index " + i);
	}
	
	for (i, val) in [1, 2, 3, 4] {
		println("index " + i + ":" + val + "\n");
	}
	
	for (key, value) in example_dic {
		if repr_of value is "tup" {
			println("Raw value at " + key + " : " + value);
		}
	}
	
	some_data = requestHttpJSON();
	
	cond_expr = false;
	while cond_expr {
		do_stuff();
	}
}

main ();
```
