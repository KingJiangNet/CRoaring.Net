# CRoaring.Net v1.0.0-alpha

A .Net wrapper for [CRoaring](https://github.com/RoaringBitmap/CRoaring) - a C implementation of [RoaringBitmap](https://github.com/RoaringBitmap/RoaringBitmap).

## Usage
```cs
using (var rb1 = new RoaringBitmap())
using (var rb2 = new RoaringBitmap())
{
	rb1.Add(1, 2, 3, 4, 5, 100, 1000);
	rb1.Optimize();
	
	rb2.Add(3, 4, 5, 7, 50);
	rb2.Optimize();

	using (var result = rb1.And(rb2))
	{
		Console.WriteLine(result.Contains(2));
		Console.WriteLine(result.Contains(4));
		Console.WriteLine(result.Contains(5));
	}
}
```

## Compiling
### For Linux
Requirements:
- [GCC](https://gcc.gnu.org/)

Run the `build-linux.sh` script

### For Windows (using Linux)
Requirements:
- [MinGW-w64](https://mingw-w64.org)

Run the `build-win.sh` script.

### For Windows (using Visual Studio)
Requirements:
- [VS2015 Update 3](https://www.visualstudio.com/downloads/)
- Microsoft Visual Studio 2015 Community Edition update 3 or better.
- Within Visual Studion, have "Clang with Microsoft CodeGen" installed (found under Cross Platform Mobile Development / Visual C++ Mobile Development in the Modify features list).

Build the CRoaring and CRoaring.Net projects.

Microsoft CodeGen currently doesn't support the intrinsics required for building for x86_64. 
Instead, it is recommended you build using the Linux route above with a virtual machine or [Ubuntu on Windows](https://msdn.microsoft.com/en-us/commandline/wsl/about).

##Relevant Tips when working with C99/C11-standard compliant code in Visual Studio

Consider disabling Intellisense since it can't parse standard compliant C code: Tools -> Options -> Text Editor -> C/C++ -> Advanced, and set Disable Error Reporting to True.

Once you have a clang CodeGen project, you can go under Configuration Properties, C/C++, Language, C Language Standard and choose between C89, C99, C11, C99 GNU, C11 GNU.

## Testing CRoaring.Net

Run the `test.sh` script.
