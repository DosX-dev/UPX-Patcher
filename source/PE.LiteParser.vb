Module PE
    Private _patcher As New Patcher

    ' d = 64; L = 32
    Function GetOffsetOfPE(fileName As String)
        Return _patcher.IndexOf(fileName, {&H50, &H45, ' get "PE\x0\x0" signature
                                           &H0, &H0})
    End Function

    Function Is64(fileName As String)
        Return _patcher.GetByte(fileName, GetOffsetOfPE(fileName) + &H4) = &H64
    End Function
End Module
