# Open Stereogram Creator
Where Sistem has only 1 layer, this application can use multiple layers for your stereogram. You can achieve great effects when using multiple layers with different alpha maps.

This application is mainly aimed at assisting me when creating stereograms. This means it's not focused on user experience at the moment. It does what I want it to do. Suggestions are always welcome though.

## Layer types
Below is a brief description of the different layer types.

### Image layer
Very basic. Just an image you can place somewhere in the stereogram, perhaps to show your copyright info?

**Usage**
You load an image after adding an Image Layer. The Image is displayed. Top and Left are the amount of pixels to shift the image.

### Random dot stereogram layer
The most basic implementation of a stereogram. The layer uses a depthmap and random dots to render the stereogram.

**Usage**
You load a depth map after adding this layer. The dot stereogram is rendered automatically. You can change the parameters of the noise to your liking.

### Pattern stereogram layer
The generic but great looking stereogram that doesn't use random dots but an image with a pattern that is repeated.

**Usage**
After adding this layer you need to add a depth map and a pattern image. The pattarn image will be used for rendering a stereogram with the depth map provided.

### Full image stereogram layer
This is the same as the pattern stereogram layer, but it uses a complete image on the pattern. The application will render a stereogram based on an entire image. This will work best with small objects and patterns where most of the image is transparent.

The main purpore of this layer is to emphasize parts of the stereogram either by showing part of the objects or outlines.

**Usage**
I'm still figuring this out and I've had some rather unexpected results. You'll want to use a semi-transparent image to show the pattern, or the image can be a full size image with just a detail of the objects you display in the depth-map. Then you have to find the correct settings to overlap the pattern with the depth-map.

### Repeater layer
An image that is repeated across the image. It can add an extra layer of depth.

**Usage**
Load a (small) image and tell the application how to display it. The pattern will be repeated over and over, but can be as complicated as you want. Differences in distance will mean some instances of the image will look closer or further away.

### Reverse layer
When rendering a set of identical objects in a row, one can easily see them in 3d using cross-view. When using parallel view, however, the order of the objects must be reversed to get that same effect.

**Usage**
Basically just load the image and tell the application how many object there are. The image will be cut into that many columns and those columns will be shown reversed.

# Sistem
Stereogram generator core

## Stereogram.cs - Core logic
This is the .NET Core implementation for the Stereogram Generator I called Sistem.

If you're looking to port this to another coding language, this is where to start. This class handles all the logic. It only requires a Depthmap to generate a random dot stereogram. If you add a pattern it will use that. There's a lot of properties to control the output, but they will be set to default values if not set.

# Command line application
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
This is currently using a slightly older implementation of the Stereogram.cs class. This application will eventually be replaced with OpenStereogramCreator, which is still work in progress.

# How to build
Outdated: Not sure if just building is enough, someday I will check this.

If you're on windows use (this includes the WPF GUI):
```
dotnet build Sistem.sln
```

If you're on another platform use:
```
dotnet build Sistem.Standard.sln
```
