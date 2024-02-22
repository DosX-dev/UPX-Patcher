Module StdOut

    Sub Write(ByVal message As String, ByVal newLine As Boolean)
        Console.Out.Write(message & If(newLine, vbLf, String.Empty))
    End Sub

    Sub Log(ByVal message As String)
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.Out.Write(Date.Now().ToString("HH:mm:ss") & " -> ")
        Console.ResetColor()
        Console.Out.WriteLine(message)
    End Sub
End Module
