Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisEventOutcomeInformation

  Public Property EventOutcome As String

  Public Property EventOutcomeDetails As List(Of PremisEventOutcomeDetail)

  Public Sub New()
    EventOutcomeDetails = New List(Of PremisEventOutcomeDetail)
  End Sub

  Public Sub New(ByVal outcome As String)
    Me.New()
    EventOutcome = outcome
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("eventOutcomeInformation")
    If Not String.IsNullOrWhiteSpace(EventOutcome) Then
      xmlwr.WriteElementString("eventOutcome", EventOutcome)
    End If
    For Each det As PremisEventOutcomeDetail In EventOutcomeDetails
      det.GetXML(xmlwr)
    Next
    xmlwr.WriteEndElement()
  End Sub

End Class

