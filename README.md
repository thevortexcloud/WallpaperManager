**NOTE**: For those viewing this repo on github: This repo is an automated mirror of the repo that is hosted on a private gitlab, as such the CI script is designed to be used on gitlab. That said bug reports, suggestions, and what not are welcome here.

# Introduction

This is a program for tagging and managing a large collection of images. It is primarily intended to be used for desktop
wallpapers. As such it is designed to be used on a desktop.

It allows for categorising images by subject matter/origin (AKA Franchise) and by people in the image (EG characters from a TV
show). It also allows for including related metadata such as the author of the image, and where it was obtained from.

All of the UI has been written in Avalonia XAML/C#. As such the entire program is in theory cross platform. However it
has only ever been tested on Linux as such no guarantee is made that it will work on other platforms.

This repository is still **a work in progress** some things do not work at all or may not work well. However most core
functionality has received a first pass implementation (there is many lot can be optimised or improved), but the program
is technically usable. There are also many planned additional features (such as KDE desktop integration, and possibly ML Net auto
tagging). As such the steps below are only provided as a general guide.

# Build instructions

## Prerequisite

1. [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
2. A modern Linux distribution with an X11 desktop environment such as [Ubuntu](https://ubuntu.com/)
   or [Fedora](https://getfedora.org/) (Windows will also in theory work, although instructions are not provided)
3. SQLite

### Build Steps

Clone the repository with git

Then run

```bash
cd Cake.Wallpaper.Manager && dotnet build Cake.Wallpaper.Manager.GUI/
```

Navigate to the output directory under `./bin/Debug` and locate the `appsettings.json` file, which should look like
this:

```json
{
  "ConnectionString": "",
  "WallpaperPath": ""
}
```

# Configuration

## Database schema

A copy of the database schema can be obtained from the test project with the `Schema.sql` file. To create a database from this you will need to use a tool such as [SQLiteBrowser](https://sqlitebrowser.org/) and then execute the contents of the of the `Schema.sql` file.

## App settings

Edit the settings to match your environment according to the following table:

| Setting Name      | Example Value                              | Description                                                                                                                                        |
|-------------------|:-------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| Connection String | `Data Source=/home/myuser/mywallpaper.db;` | The location of the SQLITE database to store the wallpaper metadata, this should have been created in the [Database schema](#database-schema) step |
| Wallpaper path    | `/home/myuser/pictures/wallpapers/`        | The location where wallpapers can be found on disk that should be managed by the program                                                           |
