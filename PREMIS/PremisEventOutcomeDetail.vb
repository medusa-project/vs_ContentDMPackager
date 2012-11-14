Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisEventOutcomeDetail

  Public Property EventOutcomeDetailNote As String

  Public Property EventOutcomeDetailExtensions As List(Of XmlDocument)

  Public Sub New(elem As XmlElement)
    EventOutcomeDetailExtensions = New List(Of XmlDocument)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:eventOutcomeDetailNote", xmlns)
    For Each nd As XmlElement In nds
      EventOutcomeDetailNote = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:eventOutcomeDetailExtension", xmlns)
    For Each nd As XmlElement In nds
      EventOutcomeDetailExtensions.Add(nd.Clone)
    Next

  End Sub
  Sub New()
    EventOutcomeDetailExtensions = New List(Of XmlDocument)
  End Sub

  Sub New(ByVal detailNote As String)
    Me.New()
    EventOutcomeDetailNote = detailNote
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("eventOutcomeDetail")
    If Not String.IsNullOrWhiteSpace(EventOutcomeDetailNote) Then
      xmlwr.WriteElementString("eventOutcomeDetailNote", EventOutcomeDetailNote)
    End If
    For Each ext As XmlDocument In EventOutcomeDetailExtensions
      xmlwr.WriteStartElement("eventOutcomeDetailExtension")
      xmlwr.WriteNode(New XmlNodeReader(ext.DocumentElement), False)
      xmlwr.WriteEndElement()
    Next
    xmlwr.WriteEndElement()
  End Sub

End Class

