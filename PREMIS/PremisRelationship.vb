Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Collections

Public Class PremisRelationship

  Public Property RelationshipType As String

  Public Property RelationshipSubType As String

  Public Property RelatedObjects As List(Of PremisObject)

  Public Property RelatedEvents As List(Of PremisEvent)

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal type As String, ByVal subType As String, ByVal obj As PremisObject)
    RelationshipType = type
    RelationshipSubType = subType

    RelatedObjects = New List(Of PremisObject)

    RelatedEvents = New List(Of PremisEvent)

    RelatedObjects.Add(obj)

  End Sub

  Public Sub New(ByVal type As String, ByVal subType As String, ByVal idType As String, ByVal idValue As String)
    Me.New(type, subType, New PremisObject(idType, idValue, PremisObjectCategory.Representation))
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("relationship")
    xmlwr.WriteElementString("relationshipType", RelationshipType)
    xmlwr.WriteElementString("relationshipSubType", RelationshipSubType)

    For Each obj As PremisObject In RelatedObjects
      xmlwr.WriteStartElement("relatedObjectIdentification")
      xmlwr.WriteElementString("relatedObjectIdentifierType", obj.ObjectIdentifiers.Item(0).IdentifierType)
      xmlwr.WriteElementString("relatedObjectIdentifierValue", obj.ObjectIdentifiers.Item(0).IdentifierValue)
      xmlwr.WriteEndElement()
    Next

    For Each evt As PremisEvent In RelatedEvents
      xmlwr.WriteStartElement("relatedEventIdentification")
      xmlwr.WriteElementString("relatedEventIdentifierType", evt.EventIdentifier.IdentifierType)
      xmlwr.WriteElementString("relatedEventIdentifierValue", evt.EventIdentifier.IdentifierValue)
      xmlwr.WriteEndElement()
    Next
    xmlwr.WriteEndElement()
  End Sub

End Class
