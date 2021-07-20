# VB.Net_DotNetFinder
This project is a command line tool that looks through files to find .Net Common-Language-Runtime files.

It makes fast work of finding files in a project that can be decompiled using dotPeek or JustDecompile.

---------------------------------------------------------------
Displays a list of files which meet the criteria for CLR files.

DotNetFinder [drive:[path]] [/S] [/V] [/D [/A]]

  [drive:[path]] 
        Specifies drive and directory, or relative directory.

  /S    Displays files in specified directory and all subdirectories.
  /D    Displays folder path along with filenames.
  /A    When used with /D and searching on a relative path, shows the absolute
        path to the files, otherwise has no effect.
  /V    Verbose - show file access errors.

Examples:
DotNetFinder thisFolder/deepFolder
Shows list of CLR files in relative directory thisFolder/deepFolder.

DotNetFinder thisFolder/deepFolder /D
Shows relative path and list of CLR files in the relative directory.

DotNetFinder thisFolder/deepFolder /D /A
Shows absolute path and list of CLR files in the relative directory.
