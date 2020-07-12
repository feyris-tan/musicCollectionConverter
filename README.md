# What's this?
This is a quick and dirty tool I hacked together to convert my music collection from FLAC to OGG. You specfiy two paramters when you call the tool like this:
`musicCollectionConverter "Z:\Dumps\Importmusik" "D:\Handy\Musik"`
It will iterate through all subfolders in "Z:\Dumps\Importmusik" - It will read the tags in the FLAC file and create a folder for each album in "D:\Handy\Musik"

# How to compile?
1. Open up the solution file in Visual Studio.
2. Refresh the NuGET Packages
3. Build the solution, done.

# How to run?
If you're on Windows, grab a copy of oggenc2.exe from [here](https://www.rarewares.org/ogg-oggenc.php)
If you're on Linux, install vorbis-tools. It is packaged in Debian and Devuan, possibly other distributions as well.

Then, run the exe File and pass your input directory as the first, and the output directory as the second argument.

