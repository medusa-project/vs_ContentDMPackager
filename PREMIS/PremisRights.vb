Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class PremisRights
  Inherits PremisEntity

  Public Property RightsStatements As List(Of PremisRightsStatement)

  Public Sub New(elem As XmlElement)
    RightsStatements = New List(Of PremisRightsStatement)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:rightsStatement", xmlns)
    For Each nd As XmlElement In nds
      RightsStatements.Add(New PremisRightsStatement(nd))
    Next


    XmlId = elem.GetAttribute("xmlID")
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal rightsStatement As PremisRightsStatement)
    RightsStatements = New List(Of PremisRightsStatement)

    RightsStatements.Add(rightsStatement)
  End Sub

  Public Overrides Sub GetXML(ByVal xmlwr As XmlWriter, pCont As PremisContainer, Optional IncludeSchemaLocation As Boolean = False)
    xmlwr.WriteStartElement("rights", PremisContainer.PremisNamespace)
    xmlwr.WriteAttributeString("version", PremisContainer.PremisVersion)
    If IncludeSchemaLocation = True Then
      xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", String.Format("{0} {1}", PremisContainer.PremisNamespace, PremisContainer.PremisSchema))
    End If
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

