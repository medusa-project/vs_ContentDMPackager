Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Configuration

Public Class PremisObjectCharacteristics

  Public Property CompositionLevel As Integer = 0

  Public Property Fixities As List(Of PremisFixity)

  Public Property Size As Long?

  Public Property Formats As List(Of PremisFormat)

  Public Property CreatingApplications As List(Of PremisCreatingApplication)

  Public Property ObjectCharacteristicsExtensions As List(Of XmlDocument)

  Public Sub New(elem As XmlElement)
    Me.new()

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:compositionLevel", xmlns)
    For Each nd As XmlElement In nds
      CompositionLevel = Integer.Parse(nd.InnerText)
    Next

    nds = elem.SelectNodes("premis:fixity", xmlns)
    For Each nd As XmlElement In nds
      Fixities.Add(New PremisFixity(nd))
    Next

    nds = elem.SelectNodes("premis:size", xmlns)
    For Each nd As XmlElement In nds
      Size = Long.Parse(nd.InnerText)
    Next

    nds = elem.SelectNodes("premis:format", xmlns)
    For Each nd As XmlElement In nds
      Formats.Add(New PremisFormat(nd))
    Next

    nds = elem.SelectNodes("premis:creatingApplication", xmlns)
    For Each nd As XmlElement In nds
      CreatingApplications.Add(New PremisCreatingApplication(nd))
    Next

    nds = elem.SelectNodes("premis:objectCharacteristicsExtension", xmlns)
    For Each nd As XmlElement In nds
      ObjectCharacteristicsExtensions.Add(nd.Clone)
    Next

  End Sub

  Public Sub New()
    Fixities = New List(Of PremisFixity)
    Formats = New List(Of PremisFormat)
    CreatingApplications = New List(Of PremisCreatingApplication)
    ObjectCharacteristicsExtensions = New List(Of XmlDocument)
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
    ObjectCharacteristicsExtensions = New List(Of XmlDocument)
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

    For Each ext As XmlDocument In ObjectCharacteristicsExtensions
      xmlwr.WriteStartElement("objectCharacteristicsExtension")
      xmlwr.WriteNode(ext.CreateNavigator, False)
      xmlwr.WriteEndElement()
    Next

    xmlwr.WriteEndElement()

  End Sub



End Class

