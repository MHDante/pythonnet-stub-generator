# pythonnet-stub-generator
Creates Python Type Hints for Python.NET libraries

# Usage
You can install this through nuget. It then lets you use it as a global tool

```
dotnet tool install --global pythonnetstubgenerator.tool
GeneratePythonNetStubs --dest-path="../py_project/typings" --target-dlls="dll_folder/MyLib1.dll;other_folder/MyLib2.dll"
```

# HELP WANTED

Hey! I really would like to get this project up and running and in a place where I can be proud of it!
If you're an Intermediate C# programmer and would like to do some pair programming, let me know and we can work together on it!

Things I would like to do:
- Review/Refactor the main logic
- Create a simple tool that can be installed with either pip or nuget
- Add tests!
- Write documentation on specific features and gotchas.
- Use XML docs to generate docstrings for functions see [this thread](https://github.com/pythonnet/pythonnet/issues/374)

This would be much easier if I had someone to bounce ideas off of or work alongside of.
Not looking to dump all the work on you, just would like someone to discuss and bounce code off of.

# Contributing

If you fork this repo and make a standalone bugfix, please make a PR!
That way more people will have access to the fixes you've made.


---------

This tool is inspired by the work done by Steve Baer (@sbaer) et al.  https://github.com/mcneel/pythonstubs

Contributors:

Dante Camarena

Daniil Markelov
