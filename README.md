# Sistem - A Stereogram generator and expiremtal UI
Latest changes are mainly a backup for my new work so far. I'm trying to create a new GUI with more options than Sistem and a layer system, built on .NET 5.

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

# Sistem UI
This is currently using a slightly older implementation of the Stereogram.cs class. This application will eventuall be replaced with OpenStereogramCreator, which is still work in progress.

# Open Stereogram Creator
Where Sistem has only 1 layer, this application can use multiple layers for your stereogram. You can achieve great effects when using multiple layers with different alpha maps. At the moment the following layers are possible:

## Layer types
Below is a brief description of the different layer types.

### Image layer
Very basic. Just an image you can place somewhere in the stereogram, perhaps to show your copyright info?

### Random dot stereogram layer
The most basic implementation of a stereogram. The layer uses a depthmap and random dots to render the stereogram.

### Pattern stereogram layer
The generic but great looking stereogram that doesn't use random dots but an image with a pattern that is repeated.

### Full image stereogram layer
This is the same as the pattern stereogram layer, but it uses a complete image on the pattern. The application will render a stereogram based on an entire image. This will work best with small objects and patterns where most of the image is transparent.

The main purpore of this layer is to emphasize parts of the stereogram either by showing part of the objects or outlines.

