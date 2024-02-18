Imports System.IO
Imports System.Text

Class Patcher
    Public Function PatchBytes(ByVal filePath As String, ByVal pattern As Byte(), ByVal replacement As Byte()) As Boolean

        If Not File.Exists(filePath) Then
            Return False
        End If

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

    Function isPatternPresent(filePath As String, pattern As Byte()) As Boolean
        If Not File.Exists(filePath) Then
            Return False
        End If

        If pattern Is Nothing OrElse pattern.Length = 0 Then
            Return False
        End If

        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            If fs.Length < pattern.Length Then
                Return False
            End If

            Dim buffer(fs.Length - 1) As Byte

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

    Function FindStringOffset(ByVal fileName As String, ByVal searchString As String) As Long
        Dim offset As Long = -1
        Dim searchBytes As Byte() = Encoding.UTF8.GetBytes(searchString)

        Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read)
            Dim buffer(searchBytes.Length - 1) As Byte
            Dim bytesRead As Integer = fs.Read(buffer, 0, buffer.Length)
            While bytesRead > 0
                For i As Integer = 0 To bytesRead - searchBytes.Length
                    Dim match As Boolean = True
                    For j As Integer = 0 To searchBytes.Length - 1
                        If buffer(i + j) <> searchBytes(j) Then
                            match = False
                            Exit For
                        End If
                    Next
                    If match Then
                        offset = fs.Position - bytesRead + i
                        Return offset
                    End If
                Next
                bytesRead = fs.Read(buffer, 0, buffer.Length)
            End While
        End Using

        Return offset
    End Function



    Sub PatchBytesByOffset(ByVal fileName As String, ByVal offset As Long, ByVal replacementBytes As Byte())
        Using fs As New FileStream(fileName, FileMode.Open, FileAccess.ReadWrite)
            fs.Seek(offset, SeekOrigin.Begin)
            fs.Write(replacementBytes, 0, replacementBytes.Length)
        End Using
    End Sub

End Class
