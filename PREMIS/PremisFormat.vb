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

