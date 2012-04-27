Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisEventOutcomeDetail

  Public Property EventOutcomeDetailNote As String

  Public Property EventOutcomeDetailExtensions As List(Of XmlDocument)

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

