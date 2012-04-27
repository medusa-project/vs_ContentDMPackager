Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class PremisObject
  Inherits PremisEntity

  Public Property ObjectIdentifiers As List(Of PremisIdentifier)

  Public Property PreservationLevels As List(Of PremisPreservationLevel)

  Public Property ObjectCategory As PremisObjectCategory

  Public Property ObjectCharacteristics As List(Of PremisObjectCharacteristics)

  Public Property OriginalName As String

  Public Property LinkedEvents As List(Of PremisEvent)

  Public Property LinkedRightsStatements As List(Of PremisRightsStatement)

  Public Property LinkedIntellectualEntityIdentifiers As List(Of PremisIdentifier)

  Public Property Relationships As List(Of PremisRelationship)

  Public Property XmlId As String

  Public Sub LinkToEvent(ByVal evt As PremisEvent)
    LinkedEvents.Add(evt)
    'Add Reverse link
    If Not evt.LinkedObjects.ContainsKey(Me) Then
      evt.LinkToObject(Me)
    End If
  End Sub

  Public Sub LinkToRightsStatement(ByVal rStmt As PremisRightsStatement)
    LinkedRightsStatements.Add(rStmt)
    'Add Reverse link
    If Not rStmt.LinkedObjects.ContainsKey(Me) Then
      rStmt.LinkToObject(Me)
    End If
  End Sub

  Public Sub LinkToIntellectualEntity(ByVal idType As String, ByVal idValue As String)
    Dim id As New PremisIdentifier(idType, idValue)
    LinkedIntellectualEntityIdentifiers.Add(id)
  End Sub

  Public Sub RelateToObject(ByVal typ As String, ByVal subTyp As String, ByVal idType As String, ByVal idValue As String)
    Dim pRel As New PremisRelationship(typ, subTyp, idType, idValue)
    Me.Relationships.Add(pRel)
  End Sub

  Public Sub RelateToObject(ByVal typ As String, ByVal subTyp As String, ByVal obj As PremisObject)
    Dim pRel As New PremisRelationship(typ, subTyp, obj)
    Me.Relationships.Add(pRel)
  End Sub

  Public Sub RelateToObject(ByVal typ As String, ByVal subTyp As String, ByVal obj As PremisObject, ByVal evt As PremisEvent)
    'If this object already has relationship with the same type and subtype and event just add the obj to that relationship

    Dim relats = Me.Relationships.Where(Function(r) r.RelationshipType = typ And r.RelationshipSubType = subTyp And _
                                          r.RelatedEvents.Exists(Function(e) e.EventIdentifier.IdentifierType = evt.EventIdentifier.IdentifierType And _
                                                                   e.EventIdentifier.IdentifierValue = evt.EventIdentifier.IdentifierValue))

    If relats.Count = 1 Then
      relats.First.RelatedObjects.Add(obj)

    Else

      Dim pRel As New PremisRelationship(typ, subTyp, obj)
      pRel.RelatedEvents.Add(evt)
      Me.Relationships.Add(pRel)

    End If

    If Not evt.LinkedObjects.ContainsKey(Me) Then
      evt.LinkedObjects.Add(Me, New List(Of String))
    End If
    evt.LinkedObjects.Add(obj, New List(Of String))

  End Sub


  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal category As PremisObjectCategory, ByVal objChars As PremisObjectCharacteristics)
    Me.New(idType, idValue, category)
    ObjectCharacteristics.Add(objChars)
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal category As PremisObjectCategory, ByVal formatName As String)
    Me.New(idType, idValue, category)
    ObjectCharacteristics.Add(New PremisObjectCharacteristics(formatName))
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal category As PremisObjectCategory)
    ObjectIdentifiers = New List(Of PremisIdentifier)
    PreservationLevels = New List(Of PremisPreservationLevel)
    ObjectIdentifiers.Add(New PremisIdentifier(idType, idValue))
    ObjectCategory = category
    ObjectCharacteristics = New List(Of PremisObjectCharacteristics)
    LinkedEvents = New List(Of PremisEvent)
    LinkedRightsStatements = New List(Of PremisRightsStatement)
    LinkedIntellectualEntityIdentifiers = New List(Of PremisIdentifier)
    Relationships = New List(Of PremisRelationship)
  End Sub

  Public Overrides Sub GetXML(ByVal xmlwr As XmlWriter, pCont As PremisContainer)
    xmlwr.WriteStartElement("object", "info:lc/xmlns/premis-v2")
    xmlwr.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", [Enum].GetName(GetType(PremisObjectCategory), ObjectCategory).ToLower)
    xmlwr.WriteAttributeString("version", "2.1")
    If Not String.IsNullOrWhiteSpace(XmlId) Then
      xmlwr.WriteAttributeString("xmlID", XmlId)
    End If

    For Each id As PremisIdentifier In ObjectIdentifiers
      xmlwr.WriteStartElement("objectIdentifier")
      xmlwr.WriteElementString("objectIdentifierType", id.IdentifierType)
      xmlwr.WriteElementString("objectIdentifierValue", id.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each lvl As PremisPreservationLevel In PreservationLevels
      lvl.GetXML(xmlwr)
    Next

    For Each objChar As PremisObjectCharacteristics In ObjectCharacteristics
      objChar.GetXML(xmlwr)
    Next

    If Not String.IsNullOrWhiteSpace(OriginalName) Then
      xmlwr.WriteElementString("originalName", OriginalName)
    End If

    For Each rel As PremisRelationship In Relationships
      rel.GetXML(xmlwr)
    Next

    For Each lnkEvt As PremisEvent In LinkedEvents
      xmlwr.WriteStartElement("linkingEventIdentifier")
      xmlwr.WriteElementString("linkingEventIdentifierType", lnkEvt.EventIdentifier.IdentifierType)
      xmlwr.WriteElementString("linkingEventIdentifierValue", lnkEvt.EventIdentifier.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each lnkInt As PremisIdentifier In LinkedIntellectualEntityIdentifiers
      xmlwr.WriteStartElement("linkingIntellectualEntityIdentifier")
      xmlwr.WriteElementString("linkingIntellectualEntityIdentifierType", lnkInt.IdentifierType)
      xmlwr.WriteElementString("linkingIntellectualEntityIdentifierValue", lnkInt.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each lnkRStmt As PremisRightsStatement In LinkedRightsStatements
      xmlwr.WriteStartElement("linkingRightsStatementIdentifier")
      xmlwr.WriteElementString("linkingRightsStatementIdentifierType", lnkRStmt.RightsStatementIdentifier.IdentifierType)
      xmlwr.WriteElementString("linkingRightsStatementIdentifierValue", lnkRStmt.RightsStatementIdentifier.IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    xmlwr.WriteEndElement()

  End Sub

  Public Overrides Function GetDefaultFileName(prefix As String, ext As String) As String

    Return PremisObject.GetFileName(Me.ObjectIdentifiers.First, [Enum].GetName(GetType(PremisObjectCategory), ObjectCategory).ToLower, prefix, ext)

  End Function

  Public Shared Function GetFileName(id As PremisIdentifier, category As String, prefix As String, ext As String) As String
    Dim localPart As String = String.Format("{2}", category, id.IdentifierType, id.IdentifierValue)

    If (Not String.IsNullOrWhiteSpace(ext)) AndAlso (Not ext.StartsWith(".")) Then
      ext = "." & ext
    End If

    Dim fname As String = String.Format("{0}{1}", prefix.Trim, localPart.Trim)

    For Each c In Path.GetInvalidFileNameChars
      fname = fname.Replace(c, "_")
    Next

    If Path.HasExtension(fname) AndAlso Not IsNumeric(Path.GetExtension(fname)) Then
      fname = Path.ChangeExtension(fname, ext)
      If String.IsNullOrWhiteSpace(ext) Then
        fname = fname.TrimEnd(".")
      End If
    Else
      fname = fname & ext
    End If

    Return fname
  End Function

  Public Function GetFilenameIdentifier() As PremisIdentifier
    Dim lid As PremisIdentifier = Me.ObjectIdentifiers.FirstOrDefault(Function(id) id.IdentifierType = "FILENAME")

    Return lid
  End Function


End Class


Public Enum PremisObjectCategory
  Representation = 0
  File = 1
  Bitstream = 2
End Enum

