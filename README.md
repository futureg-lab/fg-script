# fg-script
FGScript lang implementation in C#
## Typical use cases
```bash
# Run a script
fg-script hello.fg
# Print the AST
fg-script hello.fg --source
# Interative mode
fg-script
# Display help
fg-script -h
```
# Building and Testing
## CLI (Linux, Windows)
```bash
dotnet test
dotnet build --configuration Release --no-restore
```

## Visual Studio (Windows)
The project solution is located at the root directory

# fg-script lang overview

## Example 1: fg-script tuple to json
```rust
fn to_json(auto value) -> str {
    if (repr_of value) is "tup" {
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
    if (repr_of value) is "str" {
        ret "\"" + value + "\"";
    }
    ret to_str(value);
}
```
## Example 2: Flatten a tuple
```rust
fn flatten_helper(tup out, tup arr) -> void {
    for i in arr {
        if ((repr_of i) is "tup") {
            flatten_helper(out, i);
        } else {
            tpush(out, i);
        }
    }
}

fn flatten(tup arr) -> tup {
    tup inp = [];
    flatten_helper(inp, arr);
    ret inp;
}
```

## Example 3: Fibonacci recursive
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

## Example 4: Compute 100 digits of pi
```rust
fn pi(num n) -> void {
    num d = n * 3;
    tup arr = [];
    for i in 1 .. d {
        tpush(arr, 2);
    }
    for i in 1 .. d {
        num c = 0;
        num k = d - 1;
        while k >= 0 {
            num v = 10 * arr[k] + c;
            num r = v % (2*k + 1);
            num q = floor(v / (2*k + 1));
            if k > 0 {
                arr[k] = r;
                c = q * k;
            } else {
                arr[k] = q % 10;
                print(floor(q / 10));
                c = 0;
            }
            k = k - 1;
        }
    }
}

pi(100);
```

## Example 5: fg-script syntax overview
```rust
// Native types : str, num (double representation), tup
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

fn run() -> void {
    for i in 0 .. 10 {
    	println("index " + i);
    }
    
    for (i, val) in [1, 2, 3, 4] {
        println("index " + i + ":" + val + "\n");
    }
    
    for (key, value) in example_dic {
        if (repr_of value) is "tup" {
            println("Raw value at " + key + " : " + value);
        }
    }

    some_data = requestHttpJSON();
    
    cond_expr = false;
    while cond_expr {
        do_stuff();
    }
}

run ();
```
