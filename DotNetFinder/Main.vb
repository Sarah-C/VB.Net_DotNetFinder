Imports System.IO

Public Class Main

    Public folderCount As Integer = 0
    Public fileCount As Integer = 0

    Public workingDirectory As String = System.AppDomain.CurrentDomain.BaseDirectory

    Public subfolders As Boolean = False
    Public verbose As Boolean = False
    Public reportDirectory As Boolean = False
    Public reportRelativeToPath As Boolean = False
    Public isRelativePath As Boolean = False

    Public Shared Sub Main(ByVal args() As String)
        Dim program As New Main(args)
        If Debugger.IsAttached Then Console.ReadKey()
    End Sub

    Public Sub New(ByVal args() As String)
        Dim investigateDirectory As String = workingDirectory
        If args.Count > 0 Then
            Dim combined As String = ""
            For Each arg In args
                combined += Replace(arg.ToLower, """", "")
            Next
            Dim path As String = combined
            If combined.Contains("/") Then
                Dim options As String = ""
                path = Trim(Left(combined, InStr(combined, "/") - 1))
                If path <> "" Then path = Replace(path, "/", "\")
                'We can check combined.contains, but we'd waste time looking through the path.
                options = Trim(Right(combined, Len(combined) - path.Length))
                If options.Contains("/?") Or options.Contains("/help") Or options.Contains("/h") Then consoleWriteHelp() : Exit Sub
                subfolders = options.Contains("/s")
                verbose = options.Contains("/v")
                reportDirectory = options.Contains("/d")
                reportRelativeToPath = options.Contains("/a")
            End If
            If path <> "" Then
                If Directory.Exists(path) Then
                    investigateDirectory = path
                Else
                    Console.WriteLine("""" & path & """ does not exist.")
                    Exit Sub
                End If
            End If
            isRelativePath = Not (path.IndexOf(":") = 1)
            reportRelativeToPath = (reportRelativeToPath And isRelativePath)
        End If
        directoryScan(investigateDirectory)
        If subfolders Then Console.WriteLine("Folders checked: " & folderCount)
        Console.WriteLine("Files checked: " & fileCount)
    End Sub

    Public Sub consoleWriteHelp()
        Dim filename As String = Process.GetCurrentProcess().ProcessName
        '####################################################################### < Screen edge.
        Console.WriteLine(<preFormatted>
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
Shows absolute path and list of CLR files in the relative directory.</preFormatted>.Value.Replace("DotNetFinder", filename))
    End Sub

    Public Sub directoryScan(ByVal startDirectory As String)
        folderCount += 1
        Try
            For Each thisFile As String In Directory.GetFiles(startDirectory)
                fileCount += 1
                If checkFileIsCLR(thisFile) Then
                    If reportDirectory Then
                        If reportRelativeToPath Then
                            Console.WriteLine(workingDirectory & thisFile)
                        Else
                            Console.WriteLine(thisFile)
                        End If
                    Else
                        Console.WriteLine(Path.GetFileName(thisFile))
                    End If
                End If
            Next
            If subfolders Then
                For Each thisDirectory As String In Directory.GetDirectories(startDirectory)
                    directoryScan(thisDirectory)
                Next
            End If
        Catch generalException As System.Exception
            If verbose Then Console.WriteLine(generalException.Message)
        End Try
    End Sub

    Public Function checkFileIsCLR(ByVal path As String) As Boolean
        Dim fs As Stream = Nothing
        Try
            fs = New FileStream(path, FileMode.Open, FileAccess.Read)
            If fs.Length < 140 Then Return False
            Dim reader As New BinaryReader(fs)
            fs.Position = 128
            If reader.ReadByte <> 80 Then Return False
            If reader.ReadByte <> 69 Then Return False
            'PE Header starts @ 0x3C (60). Its a 4 byte header.
            fs.Position = &H3C
            Dim peHeader As UInteger = reader.ReadUInt32()
            If peHeader > fs.Length Then Return False
            'Moving to PE Header start location...
            fs.Position = peHeader + 24
            Dim posEndOfHeader As Long = fs.Position
            Dim magic As UShort = reader.ReadUInt16()
            Dim [off] As Integer = &H60 ' Offset to data directories for 32Bit PE images
            ' See section 3.4 of the PE format specification.
            If magic = &H20B Then '0x20b == PE32+ (64Bit), 0x10b == PE32 (32Bit)
                [off] = &H70 ' Offset to data directories for 64Bit PE images
            End If
            fs.Position = posEndOfHeader + [off] + 112
            Dim peek As UInt32 = reader.ReadUInt32()
            fs.Close()
            Return (peek <> 0)
        Catch generalException As Exception
            If verbose Then Console.WriteLine(generalException.Message)
            Return False
        Finally
            If fs IsNot Nothing Then fs.Close()
        End Try
    End Function

End Class