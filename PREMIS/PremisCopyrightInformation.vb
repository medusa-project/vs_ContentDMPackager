Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisCopyrightInformation

  Public Property CopyrightStatus As String

  Public Property CopyrightJurisdiction As String

  Public Property CopyrightStatusDeterminationDate As String

  Public Property CopyrightNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    CopyrightNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:copyrightStatus", xmlns)
    For Each nd As XmlElement In nds
      CopyrightStatus = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:copyrightJurisdiction", xmlns)
    For Each nd As XmlElement In nds
      CopyrightJurisdiction = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:copyrightStatusDeterminationDate", xmlns)
    For Each nd As XmlElement In nds
      CopyrightStatusDeterminationDate = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:copyrightNote", xmlns)
    For Each nd As XmlElement In nds
      CopyrightNotes.Add(nd.InnerText)
    Next
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal status As String, ByVal jurisdiction As String)
    CopyrightStatus = status
    CopyrightJurisdiction = jurisdiction
    CopyrightNotes = New List(Of String)
  End Sub


  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("copyrightInformation")
    xmlwr.WriteElementString("copyrightStatus", CopyrightStatus)
    xmlwr.WriteElementString("copyrightJurisdiction", CopyrightJurisdiction)
    If Not String.IsNullOrWhiteSpace("") Then
      xmlwr.WriteElementString("copyrightStatusDeterminationDate", CopyrightStatusDeterminationDate)
    End If
    For Each nt As String In CopyrightNotes
      xmlwr.WriteElementString("copyrightNote", nt)
    Next
    xmlwr.WriteEndElement()
  End Sub


End Class
