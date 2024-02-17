Imports System.IO

Class Patcher
    Public Function PatchBytes(ByVal filePath As String, ByVal pattern As Byte(), ByVal replacement As Byte()) As Boolean
        Dim matchFound As Boolean = False

        Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            Dim buffer(123456) As Byte
            Dim bytesRead As Integer = 0

            Do
                bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                For i As Integer = 0 To bytesRead - pattern.Length
                    Dim match As Boolean = True
                    For j As Integer = 0 To pattern.Length - 1
                        If buffer(i + j) <> pattern(j) Then
                            match = False
                            Exit For
                        End If
                    Next

                    If match Then
                        fileStream.Position = fileStream.Position - bytesRead + i
                        fileStream.Write(replacement, 0, replacement.Length)
                        fileStream.Flush()
                        matchFound = True
                        Exit For
                    End If
                Next

                If matchFound Then Exit Do
            Loop While bytesRead > 0
        End Using

        Return matchFound
    End Function

    Function isPatternPresent(fileName As String, pattern As Byte()) As Boolean
        Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read)
            Dim buffer(pattern.Length - 1) As Byte
            Dim bytesRead As Integer = fs.Read(buffer, 0, buffer.Length)

            While bytesRead > 0
                If bytesRead >= pattern.Length Then
                    For i As Integer = 0 To bytesRead - pattern.Length
                        Dim match As Boolean = True
                        For j As Integer = 0 To pattern.Length - 1
                            If buffer(i + j) <> pattern(j) Then
                                match = False
                                Exit For
                            End If
                        Next
                        If match Then
                            Return True
                        End If
                    Next
                End If
                Array.Copy(buffer, pattern.Length, buffer, 0, bytesRead - pattern.Length)
                bytesRead = fs.Read(buffer, bytesRead - pattern.Length, pattern.Length)
            End While
        End Using

        Return False
    End Function

End Class
