Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisLicenseInformation

  Public Property LicenseTerms As String

  Public Property LicenseNotes As List(Of String)

  Public Sub New(elem As XmlElement)
    LicenseNotes = New List(Of String)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:licenseTerms", xmlns)
    For Each nd As XmlElement In nds
      LicenseTerms = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:licenseNote", xmlns)
    For Each nd As XmlElement In nds
      LicenseNotes.Add(nd.InnerText)
    Next
  End Sub

  Public Sub New()
    LicenseNotes = New List(Of String)
  End Sub

  Public Sub New(ByVal terms As String)
    LicenseTerms = terms
    LicenseNotes = New List(Of String)
  End Sub


  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("licenseInformation")
    If Not String.IsNullOrWhiteSpace(LicenseTerms) Then
      xmlwr.WriteElementString("licenseTerms", LicenseTerms)
    End If
    For Each nt As String In LicenseNotes
      xmlwr.WriteElementString("licenseNote", nt)
    Next
    xmlwr.WriteEndElement()
  End Sub



End Class
