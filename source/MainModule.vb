Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Logging

Module Program
    Public rnd As New Random()

    Public Sub Main()
        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.Write(vbLf & " UPX-Patcher (")

        Console.ForegroundColor = ConsoleColor.DarkCyan
        Console.Write("https://github.com/DosX-dev/UPX-Patcher")

        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.WriteLine(")" & vbLf)

        Console.ResetColor()

        Dim args = Environment.GetCommandLineArgs()

        If args.Length = 1 Then
            Console.WriteLine("Usage: {0} <file_path>", AppDomain.CurrentDomain.FriendlyName)
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

                    If bytesReplacer.isPatternPresent(fileName, Encoding.ASCII.GetBytes("DOSX")) Then
                        Throw New Exception("This file already patched.")
                    Else
                        Throw New Exception("This file is not packed with UPX.")
                    End If

                End If

            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine(ex.Message)
                Console.ResetColor()
                Environment.Exit(1)
            End Try



            Console.WriteLine("Sections confusing...")

            bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #0
                                                &H58, &H30},
                                               Encoding.ASCII.GetBytes("DOSX"))

            bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #1
                                                &H58, &H31},
                                               Encoding.ASCII.GetBytes("FISH"))

            bytesReplacer.PatchBytes(fileName, {&H55, &H50, ' #2
                                                &H58, &H32},
                                               Encoding.ASCII.GetBytes("CODE"))

            Console.WriteLine("Version block confusing...")

            Dim randomVersionPrefix(3) As Byte
            rnd.NextBytes(randomVersionPrefix)

            bytesReplacer.PatchBytes(fileName,
                                            {
                                                &H33, &H2E, &H39 ' "3.9" ..
                                            }, randomVersionPrefix)

            Dim randomVersionId(6) As Byte
            rnd.NextBytes(randomVersionId)

            bytesReplacer.PatchBytes(fileName,
                                            {
                                                &H0, ' padding
                                                &H55, &H50, &H58, ' "UPX"
                                                &H21, ' "!"
                                                &HD ' compressed data separator
                                            }, randomVersionId)

            Console.WriteLine("Adding fake version block...")

            bytesReplacer.PatchBytes(fileName,
                                            {
                                                &H0, ' padding
                                                &H0, &H0, &H0, &H0, ' 00 00 00 00 -> "DosX"
                                                &H0, ' version separator
                                                &H0, &H0, &H0, ' 00 00 00 -> "UPX"
                                                &H0 ' 00 -> "!"
                                            }, {
                                                &H0, ' padding
                                                &H44, &H6F, &H73, &H58, ' "DosX"
                                                &H0, ' version separator
                                                &H55, &H50, &H58, ' "UPX"
                                                &H21 ' "!"
                                            }
            )

            Console.WriteLine("Replacing standart DOS Stub message...")

            bytesReplacer.PatchBytes(fileName, Encoding.ASCII.GetBytes("This program cannot be run in DOS mode."),
                                               Encoding.ASCII.GetBytes("https://github.com/DosX-dev/UPX-Patcher"))

            Console.WriteLine("Done!")
        End If
    End Sub

End Module
