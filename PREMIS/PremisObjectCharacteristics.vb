Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Configuration
Imports Uiuc.Library.MetadataUtilities

Public Class PremisObjectCharacteristics

  Public Property CompositionLevel As Integer = 0

  Public Property Fixities As List(Of PremisFixity)

  Public Property Size As Long?

  Public Property Formats As List(Of PremisFormat)

  Public Property CreatingApplications As List(Of PremisCreatingApplication)

  Protected Sub New()
    'no public empty constuctors allowed
    Fixities = New List(Of PremisFixity)
    Formats = New List(Of PremisFormat)
    CreatingApplications = New List(Of PremisCreatingApplication)
  End Sub

  Public Sub New(ByVal formatName As String)
    Me.New(New PremisFormat(formatName))
  End Sub

  Public Sub New(ByVal format As PremisFormat)
    Me.New(0, format)
  End Sub

  Public Sub New(ByVal compLevel As Integer, ByVal format As PremisFormat)
    CompositionLevel = compLevel
    Fixities = New List(Of PremisFixity)
    Formats = New List(Of PremisFormat)
    CreatingApplications = New List(Of PremisCreatingApplication)
    Formats.Add(format)
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("objectCharacteristics")

    xmlwr.WriteElementString("compositionLevel", CompositionLevel)

    For Each fx As PremisFixity In Fixities
      fx.GetXML(xmlwr)
    Next

    If Size.HasValue Then
      xmlwr.WriteElementString("size", Size)
    End If

    For Each fmt As PremisFormat In Formats
      fmt.GetXML(xmlwr)
    Next

    For Each creApp As PremisCreatingApplication In CreatingApplications
      creApp.GetXML(xmlwr)
    Next

    xmlwr.WriteEndElement()

  End Sub

  Public Shared Function CharacterizeFile(ByVal filepath As String, ByVal proposedMime As String) As PremisObjectCharacteristics
    Dim strm As Stream = File.OpenRead(filepath)
    Dim pObjChar As New PremisObjectCharacteristics()

    Dim alg As String = ConfigurationManager.AppSettings.Item("ChecksumAlgorithm")
    Dim sha1 As HashAlgorithm = HashAlgorithm.Create(alg)

    Dim hash() As Byte = sha1.ComputeHash(strm)

    strm.Close()

    Dim fInfo As New FileInfo(filepath)

    Dim pFix As New PremisFixity(alg, MetadataFunctions.BytesToHexStr(hash))
    pObjChar.Fixities.Add(pFix)
    pObjChar.Size = fInfo.Length

    Dim mime As String = MetadataFunctions.GetMimeFromFile(filepath, proposedMime)
    If Not String.IsNullOrWhiteSpace(mime) Then
      Dim pForm As New PremisFormat(mime)
      pObjChar.Formats.Add(pForm)
    End If



    Return pObjChar

  End Function



End Class

