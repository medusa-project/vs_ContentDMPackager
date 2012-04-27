Imports System.IO
Imports Uiuc.Library.Premis

''' <summary>
''' In order to keep the core Premis classes as generic as possible, any code that is unique to the Medusa Repository project should go here.
''' </summary>
''' <remarks></remarks>
Public Class MedusaHelpers


  ''' <summary>
  '''  This function partitions the given premisContainer and then saves a separate XML file for each grouping of files according to the Medusa content models
  ''' </summary>
  ''' <param name="cont"></param>
  ''' <param name="folder"></param>
  ''' <param name="createSubDirs"></param>
  ''' <remarks></remarks>
  Public Shared Sub SavePartitionedXML(cont As PremisContainer, folder As String, createSubDirs As Boolean)

    Dim rootObj = cont.Objects.First 'assume the first object in the container is the root object
    Dim rootPath = rootObj.GetDefaultFileName("", "")
    Dim conList As New List(Of KeyValuePair(Of String, PremisContainer))

    MedusaHelpers.PartitionContainer(rootObj, rootPath, conList)

    If createSubDirs Then
      For Each kv As KeyValuePair(Of String, PremisContainer) In conList
        Directory.CreateDirectory(Path.Combine(folder, kv.Key))
        For Each obj In kv.Value.Objects.Where(Function(o) o.ObjectCategory = PremisObjectCategory.File)
          File.Move(Path.Combine(folder, obj.GetFilenameIdentifier.IdentifierValue), Path.Combine(folder, kv.Key, obj.GetFilenameIdentifier.IdentifierValue))
        Next
        kv.Value.SaveXML(Path.Combine(folder, kv.Key, kv.Value.Objects.First.GetDefaultFileName("premis_", "xml")))
      Next
    Else
      For Each kv As KeyValuePair(Of String, PremisContainer) In conList
        kv.Value.SaveXML(Path.Combine(folder, kv.Value.Objects.First.GetDefaultFileName("premis_", "xml")))
      Next
    End If

  End Sub

  ''' <summary>
  ''' Partition this container into multiple other containers matching our Medusa content model
  ''' </summary>
  ''' <param name="rootObj">The root starting premis object</param>
  ''' <param name="rootDir">The relative directory path for the root object</param>
  ''' <param name="partList">A list of key-value pairs with the key being the relative directory path and the value being
  ''' the corresponding premis container</param>
  ''' <remarks>This routine will need to be modified as the Medusa Fedora content models are changed or as new relationship types and subtypes are added.</remarks>
  Private Shared Sub PartitionContainer(rootObj As PremisObject, rootDir As String, partList As List(Of KeyValuePair(Of String, PremisContainer)))

    Dim newCont As New PremisContainer()
    MedusaHelpers.AddObjectsAndChildren(rootObj, newCont)
    Dim kv As New KeyValuePair(Of String, PremisContainer)(rootDir, newCont)
    partList.Add(kv)

    For Each r As PremisRelationship In rootObj.Relationships
      Select Case r.RelationshipType
        Case "COLLECTION"
          'collection is external to this package so do nothing

          Select Case r.RelationshipSubType
            Case "IS_MEMBER_OF"

            Case Else
              Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "METADATA"
          'metadata is kept with its related objects so do nothing

          Select Case r.RelationshipSubType
            Case "HAS_ROOT"

            Case Else
              Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "DERIVATION"
          'derivations are kept with their related object and currently only apply to metadata so do nothing

          Select Case r.RelationshipSubType
            Case "HAS_SOURCE", "IS_SOURCE_OF"

            Case Else
              Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_IMAGE_ASSET"
          'basic image assets are partitioned into separate fedora objects so create new subdir and corresponding premis container

          Select Case r.RelationshipSubType
            Case "PRODUCTION_MASTER", "ARCHIVAL_MASTER", "SCREEN_SIZE", "THUMBNAIL"
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_COMPOUND_ASSET"
          'compound assets are partitioned into separate containers, subdirectories are created depending on the relationship

          Select Case r.RelationshipSubType
            Case "FIRST_CHILD"
              'create subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList)
              Next

            Case "NEXT_SIBLING"
              'use same subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(Path.GetDirectoryName(rootDir), o.GetDefaultFileNameIndex), partList)
              Next

            Case "PREVIOUS_SIBLING"
              'previous siblings should have already been created

            Case "PARENT"
              'parents should have already been created

            Case Else
              Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case Else
          Throw New ApplicationException(String.Format("Unexpected PREMIS Relationship Type: {0}", r.RelationshipType))
      End Select
    Next

  End Sub

  Private Shared Sub AddEventAndChildren(evt As PremisEvent, cont As PremisContainer)
    If Not cont.Events.Contains(evt) Then
      cont.Events.Add(evt)

      For Each agt As PremisAgent In evt.LinkedAgents.Keys
        MedusaHelpers.AddAgentAndChildren(agt, cont)
      Next

      For Each obj As PremisObject In evt.LinkedObjects.Keys.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
        If Not obj.PreservationLevels.Exists(Function(p) p.PreservationLevelValue = "ORIGINAL_CONTENT_FILE" Or p.PreservationLevelValue = "DERIVATIVE_CONTENT_FILE") Then
          MedusaHelpers.AddObjectsAndChildren(obj, cont)
        End If
      Next

    End If
  End Sub

  Private Shared Sub AddAgentAndChildren(agt As PremisAgent, cont As PremisContainer)
    If Not cont.Agents.Contains(agt) Then
      cont.Agents.Add(agt)

      'NOTE: linked events and rights should end up in the contained via other means, so we do not enumerate them here

    End If
  End Sub

  Private Shared Sub AddRightsStatementAndChildren(rgtS As PremisRightsStatement, cont As PremisContainer)
    'make sure not already added
    For Each r As PremisRights In cont.Rights
      If r.RightsStatements.Contains(rgtS) Then Exit Sub
    Next

    Dim rgt As New PremisRights(rgtS)
    If Not cont.Rights.Contains(rgt) Then
      cont.Rights.Add(rgt)

      For Each agt As PremisAgent In rgtS.LinkedAgents.Keys
        MedusaHelpers.AddAgentAndChildren(agt, cont)
      Next

    End If
  End Sub

  Private Shared Sub AddObjectsAndChildren(obj As PremisObject, cont As PremisContainer)
    If Not cont.Objects.Contains(obj) Then
      cont.Objects.Add(obj)

      For Each pEvt As PremisEvent In obj.LinkedEvents
        MedusaHelpers.AddEventAndChildren(pEvt, cont)
      Next

      For Each pRgtS As PremisRightsStatement In obj.LinkedRightsStatements
        MedusaHelpers.AddRightsStatementAndChildren(pRgtS, cont)
      Next

      For Each relat As PremisRelationship In obj.Relationships
        'only objects which are not in their own container will be added here
        Select Case relat.RelationshipType
          Case "METADATA", "COLLECTION", "DERIVATION"
            Dim i As Integer = 0
            For Each obj2 As PremisObject In relat.RelatedObjects.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
              MedusaHelpers.AddObjectsAndChildren(obj2, cont)
              i = i + 1
            Next
            If i > 0 Then
              For Each evt As PremisEvent In relat.RelatedEvents
                MedusaHelpers.AddEventAndChildren(evt, cont)
                i = i + 1
              Next
            End If
        End Select
      Next
    End If
  End Sub

End Class
