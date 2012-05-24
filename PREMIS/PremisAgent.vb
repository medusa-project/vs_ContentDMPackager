﻿Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class PremisAgent
  Inherits PremisEntity

  Public Property AgentIdentifiers As List(Of PremisIdentifier)

  Public Property AgentNames As List(Of String)

  Public Property AgentType As String

  Public Property AgentNotes As List(Of String)

  Public Property LinkedEvents As List(Of PremisEvent)

  Public Property LinkedRightsStatements As List(Of PremisRightsStatement)

  Public Property XmlId As String

  Public Sub LinkToEvent(ByVal evt As PremisEvent)
    LinkedEvents.Add(evt)
    'Reverse link
    If Not evt.LinkedAgents.ContainsKey(Me) Then
      evt.LinkToAgent(Me)
    End If
  End Sub

  Public Sub LinkToRightsStatement(ByVal rStmt As PremisRightsStatement)
    LinkedRightsStatements.Add(rStmt)
    'Reverse link
    If Not rStmt.LinkedAgents.ContainsKey(Me) Then
      rStmt.LinkToAgent(Me)
    End If
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub


  Public Sub New(ByVal idType As String, ByVal idValue As String)
    AgentIdentifiers = New List(Of PremisIdentifier)
    AgentIdentifiers.Add(New PremisIdentifier(idType, idValue))
    AgentNames = New List(Of String)
    AgentNotes = New List(Of String)
    LinkedEvents = New List(Of PremisEvent)
    LinkedRightsStatements = New List(Of PremisRightsStatement)

  End Sub

  Public Overrides Sub GetXML(ByVal xmlwr As XmlWriter, pCont As PremisContainer)
    xmlwr.WriteStartElement("agent", "info:lc/xmlns/premis-v2")
    xmlwr.WriteAttributeString("version", "2.1")
    If Not String.IsNullOrWhiteSpace(XmlId) Then
      xmlwr.WriteAttributeString("xmlID", XmlId)
    End If

    For Each id As PremisIdentifier In AgentIdentifiers
      xmlwr.WriteStartElement("agentIdentifier")
      xmlwr.WriteElementString("agentIdentifierType", id.IdentifierType)
      xmlwr.WriteElementString("agentIdentifierValue", id.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each nm As String In AgentNames
      xmlwr.WriteElementString("agentName", nm)
    Next

    If Not String.IsNullOrWhiteSpace(AgentType) Then
      xmlwr.WriteElementString("agentType", AgentType)
    End If

    For Each nt As String In AgentNotes
      xmlwr.WriteElementString("agentNote", nt)
    Next

    'only include events that are included in the parent container
    For Each lnkEvt As PremisEvent In LinkedEvents.Where(Function(e) pCont.Events.Contains(e))
      xmlwr.WriteStartElement("linkingEventIdentifier")
      xmlwr.WriteElementString("linkingEventIdentifierType", lnkEvt.EventIdentifier.IdentifierType)
      xmlwr.WriteElementString("linkingEventIdentifierValue", lnkEvt.EventIdentifier.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each lnkRStrm As PremisRightsStatement In LinkedRightsStatements
      xmlwr.WriteStartElement("linkingRightsStatementIdentifier")
      xmlwr.WriteElementString("linkingRightsStatementIdentifierType", lnkRStrm.RightsStatementIdentifier.IdentifierType)
      xmlwr.WriteElementString("linkingRightsStatementIdentifierValue", lnkRStrm.RightsStatementIdentifier.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    xmlwr.WriteEndElement()


  End Sub

  Public Overrides Function GetDefaultFileName(prefix As String, ext As String) As String

    Return PremisAgent.GetFileName(Me.AgentIdentifiers.First, prefix, ext)

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

  Public ReadOnly Property EmailIdentifierValue As String
    Get
      Return AgentIdentifiers.Where(Function(id) id.IdentifierType = "EMAIL").FirstOrDefault.IdentifierValue
    End Get
  End Property

End Class


