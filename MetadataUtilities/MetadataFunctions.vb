Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Runtime.InteropServices.Marshal
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.IO
Imports System.Text

Public Class MetadataFunctions

  ''' <summary>
  ''' Based on the syntax of the call number (using regular expressions) return the classification authority value; either ddc for
  ''' Dewey, lcc for Library of Congress, sudocs for the Superintendent of Documents Classification System, or empty string if unknown.
  ''' </summary>
  ''' <param name="callno"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetCallNumberAuthority(ByVal callno As String) As String
    'TODO: Need unit test for these

    Dim reDDC As New Regex("^\s*([FQ][\._]\s?)?X?\d\d\d\D|^\s*([FQ][\._]\s?)?[ABC][\._]\d|^\s*([FQ][\._]\s?)?[ABC][\._][A-Z]")
    Dim reLCC As New Regex("^\s*([FQ][\._]\s?)?[A-H,J-N,P-V,Z][A-Z]{0,2}\d{1,4}")
    Dim reSUDOCS As New Regex("^\s*([FQ][\._]\s?)?[A-Z]{1,4} \d+\.")

    If reDDC.Match(callno).Success Then
      Return "ddc"
    ElseIf reLCC.Match(callno).Success Then
      Return "lcc"
    ElseIf callno.Contains(":") AndAlso reSUDOCS.Match(callno).Success Then
      Return "sudocs"
    Else
      Return ""
    End If

  End Function

  ''' <summary>
  ''' Generate a new Medusa Local Identifier which follows this syntax:  HandleProject:Guid-CheckDigit
  ''' </summary>
  ''' <returns>Handle</returns>
  ''' <remarks>HandlePrefix is optional.  A check digit is added to the end of the GUID using the Verhoeff algorithm</remarks>
  Public Shared Function GenerateLocalIdentifier() As String
    Dim project As String = ConfigurationManager.AppSettings.Item("Handle.Project")
    Dim uuid As Guid = Guid.NewGuid


    Dim uuidStr As String = uuid.ToString.Replace("-", "")
    Dim checkD As Char = CheckDigit.GenerateCheckCharacter(uuidStr)
    Dim handle As String = String.Format("{0}:{1}-{2}", project, uuid.ToString, checkD)

    Return handle
  End Function

  ''' <summary>
  ''' Given a complete handle, return just the local part (minus the registered handle prefix)
  ''' </summary>
  ''' <param name="handle"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetLocalIdentifier(handle As String) As String
    Dim re As New Regex("^\s*(\d+)/([^:]+:[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}-[A-F0-9](?:\.\d+)?)\s*$", RegexOptions.IgnoreCase)
    Dim ret As String

    Dim m As Match = re.Match(handle)

    If m.Success Then
      Dim prefix As String = m.Groups.Item(1).Value
      Dim project As String = m.Groups.Item(2).Value
      ret = project
    Else
      Throw New Exception("Invalid Handle: " & handle)
    End If

    Return ret
  End Function

  ''' <summary>
  ''' If the given handle is valid return True; else return False.  This checks for valid syntax, prefix, project, and Guid Check Digit.
  ''' </summary>
  ''' <param name="handle"></param>
  ''' <returns></returns>
  ''' <remarks>HandlePrefix is optional.</remarks>
  Public Shared Function ValidateHandle(ByVal handle As String) As Boolean
    Dim re As New Regex("^\s*(?:(\d+)/)?([^:]+):([A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}-[A-F0-9])(?:\.(\d+))?\s*$", RegexOptions.IgnoreCase)

    Dim m As Match = re.Match(handle)

    If m.Success Then
      Dim prefix As String = m.Groups.Item(1).Value
      Dim project As String = m.Groups.Item(2).Value
      Dim uuidPlusCheck As String = m.Groups.Item(3).Value
      Dim localId As String = m.Groups.Item(4).Value

      If (Not String.IsNullOrWhiteSpace(prefix)) AndAlso prefix <> ConfigurationManager.AppSettings.Item("Handle.Prefix") Then
        Return False
      ElseIf project <> ConfigurationManager.AppSettings.Item("Handle.Project") Then
        Return False
      ElseIf CheckDigit.ValidateCheckCharacter(uuidPlusCheck.Replace("-", "")) = False Then
        Return False
      ElseIf (Not String.IsNullOrWhiteSpace(localId)) AndAlso (Not IsNumeric(localId)) Then
        Return False
      End If
    Else
      Return False
    End If

    Return True
  End Function

  Public Declare Function FindMimeFromData Lib "urlmon.dll" (ByVal pBC As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> ByVal pwzUrl As String, <MarshalAs(UnmanagedType.LPArray, ArraySubType:=UnmanagedType.I1, SizeParamIndex:=3)> ByVal pBuffer As Byte(), ByVal cbSize As Integer, <MarshalAs(UnmanagedType.LPWStr)> ByVal pwzMimeProposed As String, ByVal dwMimeFlags As Integer, <MarshalAs(UnmanagedType.LPWStr)> ByRef ppwzMimeOut As String, ByVal dwReserved As Integer) As Integer

  ''' <summary>
  ''' Return the MIME type of the given file
  ''' </summary>
  ''' <param name="fileName"></param>
  ''' <returns></returns>
  ''' <remarks>This function uses the same function used by IE to detect MIME types.  
  ''' See MSDN article titled "MIME Type Detection in Internet Explorer" However, there is one (at least)
  ''' non-standard behavior.  It returns image/pjpeg for JPEG images.  It should just be image/jpeg,
  ''' this function will correct it.  
  ''' See <a href="http://msdn.microsoft.com/en-us/library/ms775147%28VS.85%29.aspx#Known_MimeTypes">List of detected MIME Types</a>.</remarks>
  Public Shared Function GetMimeFromFile(ByVal fileName As String, ByVal proposedMime As String) As String

    If String.IsNullOrWhiteSpace(proposedMime) OrElse proposedMime.ToLower = "application/octet-stream" OrElse proposedMime.ToLower = "binary/octet-stream" Then
      proposedMime = GetContentType(fileName)
    End If

    Dim mimeout As String = ""
    Dim MaxContent As Integer
    Dim fs As FileStream
    Dim buf() As Byte
    Dim result As String

    If Not System.IO.File.Exists(fileName) Then Throw New FileNotFoundException(fileName + " not found")

    MaxContent = CInt(New FileInfo(fileName).Length)
    If MaxContent > 4096 Then MaxContent = 4096

    fs = New FileStream(fileName, FileMode.Open)
    ReDim buf(MaxContent)
    fs.Read(buf, 0, MaxContent)
    fs.Close()

    result = FindMimeFromData(IntPtr.Zero, fileName, buf, MaxContent, proposedMime, &H1 + &H20 + &H2, mimeout, 0)

    'correct non-standard mimes that IE reports
    If mimeout = "image/pjpeg" Then
      mimeout = "image/jpeg"
    ElseIf mimeout = "image/x-png" Then
      mimeout = "image/png"
    End If

    Return mimeout

  End Function

  ''' <summary>
  ''' Same as GetMimeFromFile except it uses the file extension to lookup the mime from the registry
  ''' </summary>
  ''' <param name="fileName"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetContentType(fileName As String) As String
    Dim contentType As String = "application/octet-stream"
    Dim ext As String = System.IO.Path.GetExtension(fileName).ToLower()
    Dim registryKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext)
    If registryKey IsNot Nothing AndAlso registryKey.GetValue("Content Type") IsNot Nothing Then
      contentType = registryKey.GetValue("Content Type").ToString()
    End If
    Return contentType
  End Function


  Public Shared Function BytesToHexStr(ByVal bytes() As Byte) As String
    Dim str As StringBuilder = New StringBuilder
    Dim i As Integer = 0
    Do While (i < bytes.Length)
      str.AppendFormat("{0:X2}", bytes(i))
      i = (i + 1)
    Loop
    Return str.ToString
  End Function

  ''' <summary>
  ''' Given a complete content-type, such as 'type/sub-type' value return just the 'type'
  ''' </summary>
  ''' <param name="mime"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function MimeType(mime As String) As String
    Dim delim() As Char = {"/"}
    Dim parts = mime.Split(delim, 2)
    Return parts(0)
  End Function

  ''' <summary>
  ''' Given a complete content-type, such as 'type/sub-type' value return just the 'sub-type'
  ''' </summary>
  ''' <param name="mime"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function MimeSubType(mime As String) As String
    Dim delim() As Char = {"/"}
    Dim parts = mime.Split(delim, 2)
    If parts.Count = 2 Then
      Return parts(1)
    Else
      Return ""
    End If
  End Function
End Class
