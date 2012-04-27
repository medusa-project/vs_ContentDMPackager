Public Class CpdPage

  Public Property PageTitle As String

  Public Property PageFile As String

  Public Property PagePtr As Integer

  Public Property ParentNode As CpdNode

  Public Property LocalIdentifier As String 'Used for corresponding mods file

  Public Sub New(title As String, file As String, ptr As String)
    PageTitle = title
    PageFile = file
    PagePtr = ptr
  End Sub

End Class
