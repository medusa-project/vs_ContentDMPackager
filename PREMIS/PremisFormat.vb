Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisFormat

  'formatDesignation
  Public Property FormatName As String
  Public Property FormatVersion As String

  'formatRegistry
  Public Property FormatRegistryName As String
  Public Property FormatRegistryKey As String
  Public Property FormatRegistryRole As String

  Public Property FormatNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    FormatNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:formatDesignation/premis:formatName", xmlns)
    For Each nd As XmlElement In nds
      FormatName = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:formatDesignation/premis:formatVersion", xmlns)
    For Each nd As XmlElement In nds
      FormatVersion = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:formatRegistry/premis:formatRegistryName", xmlns)
    For Each nd As XmlElement In nds
      FormatRegistryName = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:formatRegistry/premis:formatRegistryKey", xmlns)
    For Each nd As XmlElement In nds
      FormatRegistryKey = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:formatRegistry/premis:formatRegistryRole", xmlns)
    For Each nd As XmlElement In nds
      FormatRegistryRole = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:formatNote", xmlns)
    For Each nd As XmlElement In nds
      FormatNotes.Add(nd.InnerText)
    Next

  End Sub


  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal name As String)
    FormatName = name
    FormatNotes = New List(Of String)
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("format")

    If Not String.IsNullOrWhiteSpace(FormatName) Then
      xmlwr.WriteStartElement("formatDesignation")
      xmlwr.WriteElementString("formatName", FormatName)
      If Not String.IsNullOrWhiteSpace(FormatVersion) Then
        xmlwr.WriteElementString("formatVersion", FormatVersion)
      End If
      xmlwr.WriteEndElement()
    End If

    If Not String.IsNullOrWhiteSpace(FormatRegistryName) Then
      xmlwr.WriteStartElement("formatRegistry")
      xmlwr.WriteElementString("formatRegistryName", FormatRegistryName)
      xmlwr.WriteElementString("formatRegistryKey", FormatRegistryKey)
      If Not String.IsNullOrWhiteSpace(FormatRegistryRole) Then
        xmlwr.WriteElementString("formatRegistryRole", FormatRegistryRole)
      End If
      xmlwr.WriteEndElement()
    End If

    For Each note As String In FormatNotes
      xmlwr.WriteElementString("formatNote", note)
    Next

    xmlwr.WriteEndElement()

  End Sub

End Class

