Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisEventOutcomeInformation

  Public Property EventOutcome As String

  Public Property EventOutcomeDetails As List(Of PremisEventOutcomeDetail)

  Public Sub New(elem As XmlElement)
    EventOutcomeDetails = New List(Of PremisEventOutcomeDetail)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:eventOutcome", xmlns)
    For Each nd As XmlElement In nds
      EventOutcome = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:eventOutcomeDetail", xmlns)
    For Each nd As XmlElement In nds
      EventOutcomeDetails.Add(New PremisEventOutcomeDetail(nd))
    Next

  End Sub

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

