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
```
// basic json serializer using fg-script
fn to_json(auto value) -> str {
	// println("repr is " + repr_of value);
	if repr_of value is "tup" {
		str json = "{";
		num size = len(value);
		num i = 0;
		for (k, v) in value {
			str row = "\"" + k + "\"" + " : " + to_json(v);
			json = json + row;
			if i + 1 < size {
				json = json + ",";
			}
			i = i + 1;
		}
		json = json + "}";
		ret json;
	}
	ret "" + value;
}
```
```rust
// import function from csharp to a fg-script
extern requestHtml(str url) -> str;
extern requestHttpJSON(str url) -> tup;

// expose a function to the host language
expose fn sayHello(name) -> str {
    ret "Hello World! " + name;
}

// Native types : str, num (double representation), tup
// a value, a tuple, an array, a dictionary, everything is a tuple !

tup tuple = [1, 2, 3, 4, 5, [1, 2 ["something", 2]]];
tup tup_empt = [];
push(tup_empt, 1);
push(tup_empt, 2);
push(tup_empt, "3");
shift(tup_empt); // remove and return first item "1"
pop(tup_empt); // remove and return last item "3"
// tup_empt => (2)

// using tuples as a dynamic array
tup arr = arraytup(); // add a flags that enforces number indexing
arr[0] = 1;
arr[1] = 2;
// arr[2] = 3 // error
print(size(arr)); // 2
print(type(arr)); // tuple
print(isarray(arr)); // true
print(istuple(arr)); // true

// string
str some_str = "Hello World";

num some_num = 5;
tup as_tup = some_num; // (5) no need to cast since it's already a tuple
str as_str = str(some_num); // stringify
str as_num = num(as_str); // numberify
tup as_tup_again = tup(as_tup); // still (5)
tup as_tup_again = tup(as_str); // ("5")

// operation with tuples with the same dimension*
tup vec1 = [1, 2, 3];
tup vec2 = [4, "5", 6];
tup res1 = vec1 + vec2; // [1, "52", 9]
tup res2 = vec1 * vec2; // error : "*" operator is undefined for operands (num, str)
tup res3 = vec1 - vec2; // error : "-" operator is undefined for operands (num, str)
tup res4 = vec1 / vec2; // error : "/" operator is undefined for operands (num, str)

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
tup example = [a : 1, b : 2, [3]]; // throws an error

// type inference (!= generic, acts the same as auto in C++)
auto a1 = [1, 2];
auto a2 = "hello world";

fn fib(num x) -> num {
	if x < 0 {
		ret 0;
	}
	if x < 1 {
		ret 1;
	}
	ret fib(x - 1) + fib(x - 2);
}

fn main -> void {
	for i in 0 .. 10 {
		print("index " + i);
	}
	
	for (i, val) in [1, 2, 3, 4] {
		print("index " + i + ":" + val + "\n");
	}
	
	for (key, value) in example_dic {
		if type(value) is "tuple" {
			print("Raw value at " + key + " : " + value);
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