Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisStatuteInformation

  Public Property StatuteJurisdiction As String

  Public Property StatuteCitation As String

  Public Property StatuteInformationDeterminationDate As String

  Public Property StatuteNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    StatuteNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:statuteJurisdiction", xmlns)
    For Each nd As XmlElement In nds
      StatuteJurisdiction = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:statuteCitation", xmlns)
    For Each nd As XmlElement In nds
      StatuteCitation = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:statuteInformationDeterminationDate", xmlns)
    For Each nd As XmlElement In nds
      StatuteInformationDeterminationDate = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:statuteNote", xmlns)
    For Each nd As XmlElement In nds
      StatuteNotes.Add(nd.InnerText)
    Next
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(citation As String, ByVal jurisdiction As String)
    StatuteJurisdiction = jurisdiction
    StatuteCitation = citation
    StatuteNotes = New List(Of String)
  End Sub


  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("statuteInformation")
    xmlwr.WriteElementString("statuteJurisdiction", StatuteJurisdiction)
    xmlwr.WriteElementString("statuteCitation", StatuteCitation)
    If Not String.IsNullOrWhiteSpace(StatuteInformationDeterminationDate) Then
      xmlwr.WriteElementString("statuteInformationDeterminationDate", StatuteInformationDeterminationDate)
    End If
    For Each nt As String In StatuteNotes
      xmlwr.WriteElementString("statuteNote", nt)
    Next
    xmlwr.WriteEndElement()
  End Sub


End Class
