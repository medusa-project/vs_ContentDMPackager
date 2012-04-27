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
  ''' Generate a new Medusa Handle which follows this syntax:  HandlePrefix/HandleProject:Guid-CheckDigit
  ''' </summary>
  ''' <returns>Handle</returns>
  ''' <remarks>HandlePrefix is optional.  A check digit is added to the end of the GUID using the Verhoeff algorithm</remarks>
  Public Shared Function GenerateHandle() As String
    Dim prefix As String = ConfigurationManager.AppSettings.Item("HandlePrefix")
    Dim project As String = ConfigurationManager.AppSettings.Item("HandleProject")
    Dim uuid As Guid = Guid.NewGuid

    If Not String.IsNullOrWhiteSpace(prefix) Then
      prefix = prefix & "/"
    Else
      prefix = ""
    End If

    Dim uuidStr As String = uuid.ToString.Replace("-", "")
    Dim checkD As Char = CheckDigit.GenerateCheckCharacter(uuidStr)
    Dim handle As String = String.Format("{0}{1}:{2}-{3}", prefix, project, uuid.ToString, checkD)

    Return handle
  End Function

  ''' <summary>
  ''' If the given handle is valid return True; else return False.  This checks for valid syntax, prefix, project, and Guid Check Digit.
  ''' </summary>
  ''' <param name="handle"></param>
  ''' <returns></returns>
  ''' <remarks>HandlePrefix is optional.</remarks>
  Public Shared Function ValidateHandle(ByVal handle As String) As Boolean
    Dim re As New Regex("^\s*(?:(\d+)/)?([^:]+):([A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}-[A-F0-9])\s*$", RegexOptions.IgnoreCase)

    Dim m As Match = re.Match(handle)

    If m.Success Then
      Dim prefix As String = m.Groups.Item(1).Value
      Dim project As String = m.Groups.Item(2).Value
      Dim uuidPlusCheck As String = m.Groups.Item(3).Value

      If prefix <> ConfigurationManager.AppSettings.Item("HandlePrefix") Then
        Return False
      ElseIf project <> ConfigurationManager.AppSettings.Item("HandleProject") Then
        Return False
      ElseIf CheckDigit.ValidateCheckCharacter(uuidPlusCheck.Replace("-", "")) = False Then
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
  ''' this function will correct it.</remarks>
  Public Shared Function GetMimeFromFile(ByVal fileName As String, ByVal proposedMime As String) As String

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

    result = FindMimeFromData(IntPtr.Zero, fileName, buf, MaxContent, proposedMime, 0, mimeout, 0)

    'correct non-standard mimes that IE reports
    If mimeout = "image/pjpeg" Then
      mimeout = "image/jpeg"
    ElseIf mimeout = "image/x-png" Then
      mimeout = "image/png"
    End If

    Return mimeout

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

End Class
