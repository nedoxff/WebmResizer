<p align="center"><img src="https://raw.githubusercontent.com/nedoxff/WebmResizer/master/Images/webmresizer.png" alt=""/><hr></p>

### Overview
WebmResizer is a tool that lets you create those funny Discord .webm videos that resize during playback. Because it is automated, you don't have much controls over the size, but still.

### Warning
**The code is not good. I can write better code. This is a simple bodge I made in 2 days for fun. It is usable, but please understand that there are bugs and potential crashes (which you can submit in the issues tab!)**

### Features
- Current resize modes:
- - Random
- - Reduce to one pixel
- Cat image on the background
- um uh mumhgfdgmfkdoho—

### How?
I got the idea from [here](https://stackoverflow.com/questions/65500779/constantly-resizing-webm-file), but instead of aspect ratios, I deal with the width and height of the video (because ffmpeg refuses to cooperate with me).

### Building
Very soon™️ I will upload a release with a build for Windows. However, if you are a Linux user or just want to build it from source, you can do this:

**You need .NET 6.0 to build this project.**
- Clone the repo: `git clone https://github.com/nedoxff/WebmResizer`
- Build it: `cd WebmResizer && dotnet build` (you can add `--configuration Release` for a release build)
- Find the executable in `bin/Debug/net6.0` (or `bin/Release/net6.0`)

### Usage
Make sure you have `ffmpeg` and `ffprobe` in your path. To test that you can open a terminal and enter `ffmpeg` and see if it finds an executable.

By default (when running `WebmResizer`) you will enter interactive mode. It has clear instructions so we will skip it.

There are 2 commands available from the CLI: `list` and `run`.

The `list` command shows all currently available resize types and their accepted names (as an argument to `run`)

The syntax of the `run` command is as follows:
`WebmResizer run --input [INPUT_FILE] --output [OUTPUT_FILE] --split_every [SPLIT_EVERY_MS] --type [RESIZE_TYPE]`

Or:

`WebmResizer run -i [INPUT_FILE] -o [OUTPUT_FILE] -s [SPLIT_EVERY_MS] -t [RESIZE_TYPE]`

Please understand that you will not get any output when using the `run` command.

### Showcase
<p align="center">
<img src="https://raw.githubusercontent.com/nedoxff/WebmResizer/master/Images/startup.png" alt=""><h5>Startup message</h5></img>
<img src="https://raw.githubusercontent.com/nedoxff/WebmResizer/master/Images/startup.png" alt=""><h5>Main interface (interactive mode)</h5></img>
</p>
