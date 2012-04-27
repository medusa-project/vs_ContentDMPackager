Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisCopyrightInformation

  Public Property CopyrightStatus As String

  Public Property CopyrightJurisdiction As String

  Public Property CopyrightStatusDeterminationDate As String

  Public Property CopyrightNotes As List(Of String)

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
