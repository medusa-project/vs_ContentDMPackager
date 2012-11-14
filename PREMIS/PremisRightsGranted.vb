Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisRightsGranted

  Public Property Act As String

  Public Property Restrictions As List(Of String)

  Public Property TermOfGrantStartDate As String

  Public Property TermOfGrantEndDate As String

  Public Property RightsGrantedNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    Restrictions = New List(Of String)
    RightsGrantedNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:act", xmlns)
    For Each nd As XmlElement In nds
      Act = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:restriction", xmlns)
    For Each nd As XmlElement In nds
      Restrictions.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("premis:termOfGrant/premis:startDate", xmlns)
    For Each nd As XmlElement In nds
      TermOfGrantStartDate = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:termOfGrant/premis:endDate", xmlns)
    For Each nd As XmlElement In nds
      TermOfGrantEndDate = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:rightsGrantedNote", xmlns)
    For Each nd As XmlElement In nds
      RightsGrantedNotes.Add(nd.InnerText)
    Next

  End Sub


  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal act As String, ByVal startDate As String)
    Me.Act = act
    TermOfGrantStartDate = startDate
    Restrictions = New List(Of String)
    RightsGrantedNotes = New List(Of String)
  End Sub

  Public Sub New(ByVal act As String)
    Me.New(act, Now.ToString("s"))
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("rightsGranted")
    xmlwr.WriteElementString("act", Act)
    For Each rstr As String In Restrictions
      xmlwr.WriteElementString("restriction", rstr)
    Next
    xmlwr.WriteStartElement("termOfGrant")
    xmlwr.WriteElementString("startDate", TermOfGrantStartDate)
    If Not String.IsNullOrWhiteSpace(TermOfGrantEndDate) Then
      xmlwr.WriteElementString("endDate", TermOfGrantEndDate)
    End If
    xmlwr.WriteEndElement()
    For Each notes As String In RightsGrantedNotes
      xmlwr.WriteElementString("rightsGrantedNotes", notes)
    Next
    xmlwr.WriteEndElement()
  End Sub

End Class
