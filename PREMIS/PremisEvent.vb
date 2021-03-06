﻿Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Collections
Imports System.Text.RegularExpressions

Public Class PremisEvent
  Inherits PremisEntity

  Public Property EventIdentifier As PremisIdentifier

  Public Property EventType As String

  Public Property EventDateTime As Date

  Public Property EventDetail As String

  Public Property EventOutcomeInformation As List(Of PremisEventOutcomeInformation)

  Public Property LinkedAgents As Dictionary(Of PremisAgent, List(Of String))

  Public Property LinkedObjects As Dictionary(Of PremisObject, List(Of String))

  Public Overrides ReadOnly Property LocalIdentifierValue As String
    Get
      If Me.EventIdentifier.IdentifierType = "LOCAL" Then
        Return Me.EventIdentifier.IdentifierValue
      Else
        Return Nothing
      End If
    End Get
  End Property

  Public Sub New(elem As XmlElement)
    EventOutcomeInformation = New List(Of PremisEventOutcomeInformation)
    LinkedAgents = New Dictionary(Of PremisAgent, List(Of String))
    LinkedObjects = New Dictionary(Of PremisObject, List(Of String))

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:eventIdentifier", xmlns)
    For Each nd As XmlElement In nds
      EventIdentifier = New PremisIdentifier(nd.Item("eventIdentifierType", PremisContainer.PremisNamespace).InnerText, nd.Item("eventIdentifierValue", PremisContainer.PremisNamespace).InnerText)
    Next

    nds = elem.SelectNodes("premis:eventType", xmlns)
    For Each nd As XmlElement In nds
      EventType = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:eventDateTime", xmlns)
    For Each nd As XmlElement In nds
      EventDateTime = DateTime.Parse(nd.InnerText)
    Next

    nds = elem.SelectNodes("premis:eventDetail", xmlns)
    For Each nd As XmlElement In nds
      EventDetail = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:eventOutcomeInformation", xmlns)
    For Each nd As XmlElement In nds
      EventOutcomeInformation.Add(New PremisEventOutcomeInformation(nd))
    Next

    XmlId = elem.GetAttribute("xmlID")
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal type As String)
    EventIdentifier = New PremisIdentifier(idType, idValue)
    EventType = type
    EventDateTime = Now
    EventOutcomeInformation = New List(Of PremisEventOutcomeInformation)
    LinkedAgents = New Dictionary(Of PremisAgent, List(Of String))
    LinkedObjects = New Dictionary(Of PremisObject, List(Of String))
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal type As String, ByVal dateTime As Date)
    Me.New(idType, idValue, type)
    EventDateTime = dateTime
  End Sub

  Public Sub LinkToAgent(ByVal agt As PremisAgent, Optional twoWay As Boolean = False)
    LinkedAgents.Add(agt, New List(Of String))
    If twoWay = True Then
      'Reverse Link
      If Not agt.LinkedEvents.Contains(Me) Then
        agt.LinkToEvent(Me)
      End If
    End If
  End Sub

  Public Sub LinkToAgent(ByVal agt As PremisAgent, ByVal role As String, Optional twoWay As Boolean = False)
    Me.LinkToAgent(agt, twoWay)
    LinkedAgents.Item(agt).Add(role)
  End Sub

  Public Sub LinkToObject(ByVal obj As PremisObject, Optional twoWay As Boolean = True)
    LinkedObjects.Add(obj, New List(Of String))
    If twoWay = True Then
      'Reverse Link
      If Not obj.LinkedEvents.Contains(Me) Then
        obj.LinkToEvent(Me)
      End If
    End If
  End Sub

  Public Sub LinkToObject(ByVal obj As PremisObject, ByVal role As String, Optional twoWay As Boolean = True)
    Me.LinkToObject(obj, twoWay)
    LinkedObjects.Item(obj).Add(role)
  End Sub

  Public Overrides Sub GetXML(ByVal xmlwr As XmlWriter, pcont As PremisContainer, Optional IncludeSchemaLocation As Boolean = False)
    xmlwr.WriteStartElement("event", PremisContainer.PremisNamespace)
    xmlwr.WriteAttributeString("version", PremisContainer.PremisVersion)
    If Not String.IsNullOrWhiteSpace(XmlId) Then
      xmlwr.WriteAttributeString("xmlID", XmlId)
    End If
    If IncludeSchemaLocation = True Then
      xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", String.Format("{0} {1}", PremisContainer.PremisNamespace, PremisContainer.PremisSchema))
    End If

    xmlwr.WriteStartElement("eventIdentifier")
    xmlwr.WriteElementString("eventIdentifierType", EventIdentifier.IdentifierType)
    xmlwr.WriteElementString("eventIdentifierValue", EventIdentifier.IdentifierValue)
    xmlwr.WriteEndElement()

    xmlwr.WriteElementString("eventType", EventType)

    xmlwr.WriteElementString("eventDateTime", EventDateTime.ToString("s"))

    If Not String.IsNullOrWhiteSpace(EventDetail) Then
      xmlwr.WriteElementString("eventDetail", EventDetail)
    End If

    For Each evtOutInfo As PremisEventOutcomeInformation In EventOutcomeInformation
      evtOutInfo.GetXML(xmlwr)
    Next

    For Each agt As PremisAgent In LinkedAgents.Keys
      xmlwr.WriteStartElement("linkingAgentIdentifier")
      'The first agent identifier is considered the primary and is used for linking
      xmlwr.WriteElementString("linkingAgentIdentifierType", agt.AgentIdentifiers.Item(0).IdentifierType)
      xmlwr.WriteElementString("linkingAgentIdentifierValue", agt.AgentIdentifiers.Item(0).IdentifierValue)
      For Each role As String In LinkedAgents.Item(agt)
        xmlwr.WriteElementString("linkingAgentRole", role)
      Next
      xmlwr.WriteEndElement()
    Next

    For Each obj As PremisObject In LinkedObjects.Keys
      xmlwr.WriteStartElement("linkingObjectIdentifier")
      'The first object identifier is considered the primary and is used for linking
      xmlwr.WriteElementString("linkingObjectIdentifierType", obj.ObjectIdentifiers.Item(0).IdentifierType)
      xmlwr.WriteElementString("linkingObjectIdentifierValue", obj.ObjectIdentifiers.Item(0).IdentifierValue)
      For Each role As String In LinkedObjects.Item(obj)
        xmlwr.WriteElementString("linkingObjectRole", role)
      Next
      xmlwr.WriteEndElement()
    Next
    xmlwr.WriteEndElement()

  End Sub

  Public Overrides Function GetDefaultFileName(prefix As String, ext As String) As String
    Return PremisEvent.GetFileName(Me.EventIdentifier, prefix, ext)

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

