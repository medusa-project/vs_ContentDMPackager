Imports System.Console
Imports Uiuc.Library.HandleClient
Imports Uiuc.Library.MetadataUtilities
Imports System.IO
Imports System.Net

Module Handler
  Private HandleMap As New Dictionary(Of Uri, String)

  Public Function Main(ByVal args() As String) As Integer

    ServicePointManager.DefaultConnectionLimit = 10

    If args.Count = 2 Then
      Dim action As String = args(0)
      Dim filepath As String = args(1)
      ProcessBatch(action, filepath)
    Else
      Console.Out.WriteLine(My.Application.Info.AssemblyName & " action filepath")
      Console.Out.WriteLine()
      Console.Out.WriteLine("  action:   DELETE|UPDATE")
      Console.Out.WriteLine("  filepath: path to the handle csv file")
    End If

    Console.Error.WriteLine()
    Console.Error.WriteLine("Press Enter to finish")
    Console.In.ReadLine()

    Return 0
  End Function

  Private Sub ProcessBatch(action As String, filepath As String)
    LoadHandleMap(filepath)

    Select Case action.ToUpper
      Case "DELETE"
        ProcessDelete()
      Case "UPDATE"
        ProcessUpdate()
      Case Else
        Console.Error.WriteLine("Unexpected action: " & action)
    End Select

  End Sub

  Private Sub LoadHandleMap(filepath As String)
    Dim fs As New StreamReader(filepath)

    Do Until fs.EndOfStream
      Dim ln As String = fs.ReadLine
      If Not ln.StartsWith("#") Then
        Dim parts() As String = ln.Split(",", 2, StringSplitOptions.RemoveEmptyEntries)
        HandleMap.Add(New Uri(parts(1)), parts(0))
      End If
    Loop
    fs.Close()

  End Sub

  Private Sub ProcessDelete()
    For Each k In HandleMap.Keys
      Dim local_id As String = MetadataFunctions.GetLocalIdentifier(HandleMap.Item(k))
      Dim hc As HandleClient = HandleClient.DeleteHandle(local_id)
      DoOutput(hc)
    Next

  End Sub

  Private Sub ProcessUpdate()
    For Each k In HandleMap.Keys
      Dim local_id As String = MetadataFunctions.GetLocalIdentifier(HandleMap.Item(k))
      Dim hc As HandleClient = HandleClient.CreateUpdateUrlHandle(local_id, k.ToString, Nothing, Nothing)
      DoOutput(hc)
    Next

  End Sub

  Private Sub DoOutput(hc As HandleClient)
    If String.IsNullOrWhiteSpace(hc.error_message) Then
      If String.IsNullOrWhiteSpace(hc.updated_fields) Then
        Console.Out.WriteLine(hc.last_action & " " & hc.status & " " & hc.handle_value & " No Change")
      Else
        Console.Out.WriteLine(hc.last_action & " " & hc.status & " " & hc.handle_value & " Updates: " & hc.updated_fields)
      End If
    Else
      Console.Out.WriteLine(hc.last_action & " " & hc.status & " " & hc.handle_value & " " & hc.error_message)
    End If
  End Sub
End Module
