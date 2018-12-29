# Sistem - A Stereogram generator

## How to build

If you're on windows use (this includes the WPF GUI):
```
dotnet build Sistem.sln
```

If you're on another platform use:
```
dotnet build Sistem.Standard.sln
```

## Stereogram.cs - Core logic

This is the .NET Core implementation for the Stereogram Generator I called Sistem. (Sis stands for Single Image Stereogram, get it :P)

If you're looking to port this to another coding language, this is where to start. This class handles all the logic. It only requires a Depthmap to generate a random dot stereogram. If you add a pattern it will use that. There's a lot of properties to control the output, but they will be set to default values if not set.

## Command line application

The commandline application is a basic implementation of the Stereogram generator. I haven't tested it thoroughly but most of the regular scenario's seem to work.

The binary releases should be stand-alone, you can call the CLI as follows:
```
sis -d yourdepthmap.png
```

If you're compiling the application yourself in dotnet you can call the commandline after compiling application with:
```
dotnet sis.dll -d yourdepthmap.png
```

Use the -? parameter to see options I've included already.

## Sistem UI

This is currently using a slightly older implementation of the Stereogram.cs class, I will try to include the net .NET Core implementation here. Once done I'll add it to the solution.

