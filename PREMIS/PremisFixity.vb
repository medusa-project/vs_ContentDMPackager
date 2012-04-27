Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisFixity
  Public Property MessageDigestAlgorithm As String
  Public Property MessageDigest As String
  Public Property MessageDigestOriginator As String


  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal algorithm As String, ByVal digest As String)
    MessageDigestAlgorithm = algorithm
    MessageDigest = digest
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("fixity")

    xmlwr.WriteElementString("messageDigestAlgorithm", MessageDigestAlgorithm)
    xmlwr.WriteElementString("messageDigest", MessageDigest)

    If Not String.IsNullOrWhiteSpace(MessageDigestOriginator) Then
      xmlwr.WriteElementString("messageDigestOriginator", MessageDigestOriginator)
    End If

    xmlwr.WriteEndElement()

  End Sub
End Class

