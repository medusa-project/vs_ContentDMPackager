Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisOtherRightsInformation
  Public Property OtherRightsBasis As String

  Public Property OtherRightsNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    OtherRightsNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:otherRightsBasis", xmlns)
    For Each nd As XmlElement In nds
      OtherRightsBasis = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:otherRightsNote", xmlns)
    For Each nd As XmlElement In nds
      OtherRightsNotes.Add(nd.InnerText)
    Next
  End Sub

  Protected Sub New()
    'no empty constructor
  End Sub

  Public Sub New(ByVal basis As String)
    OtherRightsBasis = basis
    OtherRightsNotes = New List(Of String)
  End Sub


  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("otherRightsInformation")
    If Not String.IsNullOrWhiteSpace(OtherRightsBasis) Then
      xmlwr.WriteElementString("otherRightsBasis", OtherRightsBasis)
    End If
    For Each nt As String In OtherRightsNotes
      xmlwr.WriteElementString("otherRightsNote", nt)
    Next
    xmlwr.WriteEndElement()
  End Sub


End Class
