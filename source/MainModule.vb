Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Logging

Module Program
    Public rnd As New Random()

    Public Sub Main()
        Console.ForegroundColor = ConsoleColor.DarkYellow
        StdOut.Write(vbLf & " UPX-Patcher (", False)

        Console.ForegroundColor = ConsoleColor.DarkCyan
        StdOut.Write("https://github.com/DosX-dev/UPX-Patcher", False)

        Console.ForegroundColor = ConsoleColor.DarkYellow
        StdOut.Write(")" & vbLf, True)

        Console.ResetColor()

        Dim args = Environment.GetCommandLineArgs()

        If args.Length = 1 Then
            StdOut.Write("Usage: " & AppDomain.CurrentDomain.FriendlyName & " <file_path>", True)
            Environment.Exit(0)
        End If

        Dim fileName As String = args(1)

        If IO.File.Exists(fileName) Then


            Dim bytesReplacer As New Patcher

            Try

                Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read)
                    If Not New BinaryReader(fs).ReadBytes(2).SequenceEqual(New Byte() {&H4D, &H5A}) Then
                        Throw New Exception("It doesn't look like a valid Windows executable.")
                    End If
                End Using


                If Not bytesReplacer.isPatternPresent(fileName, {&H55, &H50, ' #0
                                                                 &H58, &H30}) Then

                    If bytesReplacer.isPatternPresent(fileName, Encoding.ASCII.GetBytes("DosX")) Then
                        Throw New Exception("This file already patched.")
                    Else
                        Throw New Exception("This file is not packed with UPX.")
                    End If

                End If


                StdOut.Log("Sections confusing...")

                bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #0
                                                    &H58, &H30,
                                                    &H0},
                                                   Encoding.ASCII.GetBytes(".dosx"))

                bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #1
                                                    &H58, &H31,
                                                    &H0},
                                                   Encoding.ASCII.GetBytes(".fish"))

                bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #2
                                                    &H58, &H32,
                                                    &H0},
                                                   Encoding.ASCII.GetBytes(".code"))

                StdOut.Log("Version block confusing...")


                Dim offset As Long = bytesReplacer.FindStringOffset(fileName, "UPX!") ' version identifier


                If offset <> -1 Then
                    Dim bytesToReplace As Int16 = 15,
                        randomVersion(bytesToReplace) As Byte
                    rnd.NextBytes(randomVersion)

                    ' ?? ?? ?? ?? 00 'UPX!'

                    bytesReplacer.PatchBytesByOffset(fileName,
                                                     offset - bytesToReplace + 4, ' full version block
                                                     randomVersion)
                Else
                    Throw New Exception("Can't get UPX version block offset.")
                End If


                '''''''''''''''''' legacy ''''''''''''''''''''''
                ' Dim randomVersionId(6) As Byte
                ' rnd.NextBytes(randomVersionId)
                '
                ' bytesReplacer.PatchBytes(fileName,
                '                                 {
                '                                     &H0, ' padding
                '                                     &H55, &H50, &H58, ' "UPX"
                '                                     &H21, ' "!"
                '                                     &HD ' compressed data separator
                '                                 }, randomVersionId)
                ''''''''''''''''''''''''''''''''''''''''''''''''


                '  StdOut.Log("Adding fake version block...")
                '
                '
                '  bytesReplacer.PatchBytes(fileName,
                '                                  {
                '                                      &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, ' padding
                '                                      &H0, &H0, &H0, &H0, ' 00 00 00 00 -> "DosX"
                '                                      &H0, ' version separator
                '                                      &H0, &H0, &H0, ' 00 00 00 -> "UPX"
                '                                      &H0, ' 00 -> "!"
                '                                      &H0 ' padding
                '                                  }, {
                '                                      &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, ' padding
                '                                      &H44, &H6F, &H73, &H58, ' "DosX"
                '                                      &H0, ' version separator
                '                                      &H55, &H50, &H58, ' "UPX"
                '                                      &H21, ' "!"
                '                                      &H0 ' padding
                '                                  }
                '  )

                StdOut.Log("Replacing standart DOS Stub message...")

                bytesReplacer.PatchBytes(fileName, Encoding.ASCII.GetBytes("This program cannot be run in DOS mode."),
                                                   Encoding.ASCII.GetBytes("https://github.com/DosX-dev/UPX-Patcher"))

                StdOut.Log("WinAPI changing...")

                bytesReplacer.PatchBytes(fileName, Encoding.ASCII.GetBytes("ExitProcess"), ' function name size is 11 bytes
                                                   Encoding.ASCII.GetBytes("CopyContext"))

                StdOut.Log("EntryPoint patching...")

                Dim isBuild64 As Boolean = PE.Is64(fileName)

                If isBuild64 Then
                    bytesReplacer.PatchBytes(fileName, ' x86_64
                                                    {
                                                        &H0, ' db 0
                                                        &H53, ' pushal
                                                        &H56 ' mov esi
                                                    },
                                                    {
                                                        &H0, ' db 0
                                                        &H55, ' push ebp
                                                        &H56 ' mov esi
                                                    }
                                             )
                Else
                    bytesReplacer.PatchBytes(fileName, ' i386
                                                    {
                                                        &H0, ' db 0
                                                        &H60, ' pushal
                                                        &HBE ' mov esi
                                                    },
                                                    {
                                                        &H0, ' db 0
                                                        &H55, ' push ebp
                                                        &HBE ' mov esi
                                                    }
                                             )
                End If

            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine(ex.Message)
                Console.ResetColor()
                Environment.Exit(1)
            End Try

            StdOut.Log("Successfully patched!")
        End If
    End Sub
End Module
