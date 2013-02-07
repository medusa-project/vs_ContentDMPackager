Imports System.Console
Imports Uiuc.Library.IdManagement
Imports System.IO
Imports System.Net

''' <summary>
''' A command line program for doing batch Handle operations against the UIUC Library's Handle Server
''' </summary>
''' <remarks></remarks>
Module Handler
  Private HandleMap As New Dictionary(Of String, Uri)

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

  ''' <summary>
  ''' Load the HandleMap file
  ''' </summary>
  ''' <param name="filepath"></param>
  ''' <remarks>The format of the file must be id,uri,handle.  The id is ignored.</remarks>
  Private Sub LoadHandleMap(filepath As String)
    Dim fs As New StreamReader(filepath)

    Do Until fs.EndOfStream
      Dim ln As String = fs.ReadLine
      If Not ln.StartsWith("#") Then
        Dim parts() As String = ln.Split(",", 3, StringSplitOptions.RemoveEmptyEntries)
        Dim u As New Uri(parts(1))
        Dim h As String = parts(2)
        HandleMap.Add(h, u)
      End If
    Loop
    fs.Close()

  End Sub

  Private Sub ProcessDelete()
    For Each k In HandleMap.Keys
      Dim local_id As String = IdManager.ParseLocalIdentifier(k)
      Dim hc As HandleClient.HandleClient = HandleClient.HandleClient.DeleteHandle(local_id)
      DoOutput(hc)
    Next

  End Sub

  Private Sub ProcessUpdate()
    For Each k In HandleMap.Keys
      Dim local_id As String = IdManager.ParseLocalIdentifier(k)
      Dim hc As HandleClient.HandleClient = HandleClient.HandleClient.CreateUpdateHandle(local_id, HandleMap.Item(k).ToString, Nothing, Nothing)
      DoOutput(hc)
    Next

  End Sub

  Private Sub DoOutput(hc As HandleClient.HandleClient)
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
