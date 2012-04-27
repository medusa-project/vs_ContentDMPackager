Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class PremisRights
  Inherits PremisEntity

  Public Property RightsStatements As List(Of PremisRightsStatement)

  Public Property XmlId As String

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal rightsStatement As PremisRightsStatement)
    RightsStatements = New List(Of PremisRightsStatement)

    RightsStatements.Add(rightsStatement)
  End Sub

  Public Overrides Sub GetXML(ByVal xmlwr As XmlWriter, pCont As PremisContainer)
    xmlwr.WriteStartElement("rights", "info:lc/xmlns/premis-v2")
    xmlwr.WriteAttributeString("version", "2.1")
    If Not String.IsNullOrWhiteSpace(XmlId) Then
      xmlwr.WriteAttributeString("xmlID", XmlId)
    End If

    For Each stmt As PremisRightsStatement In RightsStatements
      stmt.GetXML(xmlwr)
    Next

    xmlwr.WriteEndElement()

  End Sub

  Public Overrides Function GetDefaultFileName(prefix As String, ext As String) As String

    Return PremisRights.GetFileName(Me.RightsStatements.First.RightsStatementIdentifier, prefix, ext)

  End Function

  Public Shared Function GetFileName(id As PremisIdentifier, prefix As String, ext As String) As String
    Dim localPart As String = String.Format("{1}", id.IdentifierType, id.IdentifierValue)

    If (Not String.IsNullOrWhiteSpace(ext)) AndAlso (Not ext.StartsWith(".")) Then
      ext = "." & ext
    End If

    Dim fname As String = String.Format("{0}{1}{2}", prefix.Trim, localPart.Trim, ext.Trim)

    For Each c In Path.GetInvalidFileNameChars
      fname = fname.Replace(c, "_")
    Next

    Return fname
  End Function
End Class

