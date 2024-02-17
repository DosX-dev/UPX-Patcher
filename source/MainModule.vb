Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Logging

Module Program ' Let's have fun!
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
            Return
        End If

        Dim fileName As String = args(1)

        If IO.File.Exists(fileName) Then

            Dim bytesReplacer As New Patcher

            If Not bytesReplacer.isPatternPresent(fileName, {&H55, &H50, ' #0
                                                             &H58, &H30}) Then

                If bytesReplacer.isPatternPresent(fileName, Encoding.ASCII.GetBytes("DOSX")) Then
                    Console.WriteLine("This file already patched.")
                Else
                    Console.WriteLine("This file is not packed with UPX.")
                End If
                Environment.Exit(1)

            End If

            Dim randomVersionId(6) As Byte
            rnd.NextBytes(randomVersionId)


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

            bytesReplacer.PatchBytes(fileName,
                                            {
                                                &H33, &H2E, &H39 ' "3.9" ..
                                            },
                                            {
                                                &H0, &H0, &H0 ' 00 00 00
                                            }
            )

            Console.WriteLine("Replacing standart DOS Stub message...")

            bytesReplacer.PatchBytes(fileName, Encoding.ASCII.GetBytes("This program cannot be run in DOS mode."),
                                               Encoding.ASCII.GetBytes("https://github.com/DosX-dev/UPX-Patcher"))

            Console.WriteLine("Done!")
        End If
    End Sub

End Module
