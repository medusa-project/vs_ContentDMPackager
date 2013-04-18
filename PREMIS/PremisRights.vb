Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.ComponentModel

'TODO: The PREMIS Rights Entity is rather odd in that it has no identity but is just a container for RightsStatements which do have identity 
'Several of the methods inherited from PremisEntity assume that the entity has identity, to implement these methods I just use the identity of the
'first RightsStatement which is OK if the assumption is that a rights will onl;y ever contain a single rights statement, which is true for the
'current medusa implementation, but could lead to great confusion otherwise, so I have implemented the RightsStatements as a BindingList which
'throws an error if more than one RightsStatement is added.

Public Class PremisRights
  Inherits PremisEntity

  Public Property RightsStatements As BindingList(Of PremisRightsStatement)

  Private Sub RightsStatements_Changed(ByVal sender As Object, ByVal e As ListChangedEventArgs)
    If RightsStatements.Count > 1 Then
      Throw New Exception("This implementation of PremisRights allows only a single RightsStatement")
    End If
  End Sub

  Public Sub New(elem As XmlElement)
    RightsStatements = New BindingList(Of PremisRightsStatement)
    AddHandler RightsStatements.ListChanged, AddressOf RightsStatements_Changed

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
    RightsStatements = New BindingList(Of PremisRightsStatement)
    AddHandler RightsStatements.ListChanged, AddressOf RightsStatements_Changed

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

  Public Overrides ReadOnly Property LocalIdentifierValue As String
    Get
      If Me.RightsStatements.First.RightsStatementIdentifier.IdentifierType = "LOCAL" Then
        Return Me.RightsStatements.First.RightsStatementIdentifier.IdentifierValue
      Else
        Return Nothing
      End If
    End Get
  End Property

  Public Overrides Function GetDefaultFileName(prefix As String, ext As String) As String

    Return PremisRights.GetFileName(Me.RightsStatements.First.RightsStatementIdentifier, prefix, ext)

  End Function

  Public Shared Function GetFileName(id As PremisIdentifier, prefix As String, ext As String) As String
    Dim localPart As String = id.IdentifierValue

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

