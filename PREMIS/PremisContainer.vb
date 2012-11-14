Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Xml.Schema
Imports System.Xml.XPath

'TODO: Use the System.Xml.Serialization to do this

Public Class PremisContainer

  Public Const PremisNamespace As String = "info:lc/xmlns/premis-v2"

  Private Shared _schemas As XmlSchemaSet

  Private _nextId As Integer = 1 'used for generating internally unique local ids

  Public Property Objects As List(Of PremisObject)

  Public Property Agents As List(Of PremisAgent)

  Public Property Events As List(Of PremisEvent)

  Public Property Rights As List(Of PremisRights)

  Public Property ValidateXML As Boolean = True

  Public Function FindSingleObject(ByVal idType As String, ByVal idValue As String) As PremisObject
    Return FindObjects(idType, idValue).FirstOrDefault
  End Function

  Public Function FindSingleAgent(ByVal idType As String, ByVal idValue As String) As PremisAgent
    Return FindAgents(idType, idValue).FirstOrDefault
  End Function

  Public Function FindSingleEvent(ByVal idType As String, ByVal idValue As String) As PremisEvent
    Return FindEvents(idType, idValue).FirstOrDefault
  End Function

  Public Function FindSingleRightsStatement(ByVal idType As String, ByVal idValue As String) As PremisRightsStatement
    Return FindRightsStatements(idType, idValue).FirstOrDefault
  End Function

  Public Function FindObjects(ByVal idType As String, ByVal idValue As String) As List(Of PremisObject)
    Dim pid As New PremisIdentifier(idType, idValue)
    Dim ps = From p In Objects Where p.ObjectIdentifiers.Contains(pid)
    Return ps.ToList
  End Function

  Public Function FindAgents(ByVal idType As String, ByVal idValue As String) As List(Of PremisAgent)
    Dim pid As New PremisIdentifier(idType, idValue)
    Dim ps = From p In Agents Where p.AgentIdentifiers.Contains(pid)
    Return ps.ToList
  End Function

  Public Function FindEvents(ByVal idType As String, ByVal idValue As String) As List(Of PremisEvent)
    Dim ps = From p In Events Where p.EventIdentifier.IdentifierType = idType And p.EventIdentifier.IdentifierValue = idValue
    Return ps.ToList
  End Function

  Public Function FindRightsStatements(ByVal idType As String, ByVal idValue As String) As List(Of PremisRightsStatement)
    Dim ret As New List(Of PremisRightsStatement)
    For Each r In Rights
      For Each rs In r.RightsStatements
        If rs.RightsStatementIdentifier.IdentifierType = idType And rs.RightsStatementIdentifier.IdentifierValue = idValue Then
          ret.Add(rs)
        End If
      Next
    Next
    Return ret
  End Function

  Public Sub New()
    Objects = New List(Of PremisObject)
    Agents = New List(Of PremisAgent)
    Events = New List(Of PremisEvent)
    Rights = New List(Of PremisRights)
  End Sub

  Public Sub New(ByVal obj As PremisObject)
    Me.New()

    Objects.Add(obj)

  End Sub

  Public Sub New(filename As String)
    'TODO: This needs some serious testing
    Me.New()

    Dim xml As New XmlDocument()
    xml.Load(filename)
    Dim xmlns As New XmlNamespaceManager(xml.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = xml.SelectNodes("/premis:premis/premis:object", xmlns)
    For Each nd As XmlElement In nds
      Objects.Add(New PremisObject(nd))
    Next

    nds = xml.SelectNodes("/premis:premis/premis:event", xmlns)
    For Each nd As XmlElement In nds
      Events.Add(New PremisEvent(nd))
    Next

    nds = xml.SelectNodes("/premis:premis/premis:agent", xmlns)
    For Each nd As XmlElement In nds
      Agents.Add(New PremisAgent(nd))
    Next

    nds = xml.SelectNodes("/premis:premis/premis:rights", xmlns)
    For Each nd As XmlElement In nds
      Rights.Add(New PremisRights(nd))
    Next

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'After instantiating all the entities create the relationships between them
    'NOTE: If a linked to entioty does not exist in this xml then create a stub entity for purposes of filling out this model
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    For Each obj As PremisObject In Objects
      Dim nd As XmlElement = xml.SelectSingleNode(String.Format("/premis:premis/premis:object[premis:objectIdentifier/premis:objectIdentifierType='LOCAL' and premis:objectIdentifier/premis:objectIdentifierValue='{0}']", obj.LocalIdentifierValue), xmlns)

      Dim relatNds As XmlNodeList = nd.SelectNodes("premis:relationship", xmlns)
      For Each relatNd As XmlElement In relatNds
        Dim typ = relatNd.Item("relationshipType", PremisContainer.PremisNamespace)
        Dim styp = relatNd.Item("relationshipSubType", PremisContainer.PremisNamespace)

        Dim relat As New PremisRelationship(typ.InnerText, styp.InnerText)

        For Each relatObjId As XmlElement In relatNd.SelectNodes("premis:relatedObjectIdentification", xmlns)
          Dim idtyp = relatObjId.Item("relatedObjectIdentifierType", PremisContainer.PremisNamespace)
          Dim idval = relatObjId.Item("relatedObjectIdentifierValue", PremisContainer.PremisNamespace)

          Dim obj2 = Me.FindSingleObject(idtyp.InnerText, idval.InnerText)

          If obj2 IsNot Nothing Then
            relat.RelatedObjects.Add(obj2)
          Else
            relat.RelatedObjects.Add(New PremisObject(idtyp.InnerText, idval.InnerText, PremisObjectCategory.ObjectStub))
          End If

        Next

        For Each relatEvtId As XmlElement In relatNd.SelectNodes("premis:relatedEventIdentification", xmlns)
          Dim idtyp = relatEvtId.Item("relatedEventIdentifierType", PremisContainer.PremisNamespace)
          Dim idval = relatEvtId.Item("relatedEventIdentifierValue", PremisContainer.PremisNamespace)

          Dim evt2 = Me.FindSingleEvent(idtyp.InnerText, idval.InnerText)

          If evt2 IsNot Nothing Then
            relat.RelatedEvents.Add(evt2)
          Else
            relat.RelatedEvents.Add(New PremisEvent(idtyp.InnerText, idval.InnerText, "STUB"))
          End If

        Next

        obj.Relationships.Add(relat)
      Next

      Dim lnkEvtNds As XmlNodeList = nd.SelectNodes("premis:linkingEventIdentifier", xmlns)
      For Each evtNd As XmlElement In lnkEvtNds
        Dim idtyp = evtNd.Item("linkingEventIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = evtNd.Item("linkingEventIdentifierValue", PremisContainer.PremisNamespace)

        Dim evt2 = Me.FindSingleEvent(idtyp.InnerText, idval.InnerText)

        If evt2 IsNot Nothing Then
          obj.LinkedEvents.Add(evt2)
        Else
          obj.LinkedEvents.Add(New PremisEvent(idtyp.InnerText, idval.InnerText, "STUB"))
        End If

      Next

      Dim lnkRtNds As XmlNodeList = nd.SelectNodes("premis:linkingRightsStatementIdentifier", xmlns)
      For Each rtNd As XmlElement In lnkRtNds
        Dim idtyp = rtNd.Item("linkingRightsStatementIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = rtNd.Item("linkingRightsStatementIdentifierValue", PremisContainer.PremisNamespace)

        Dim rts2 = Me.FindSingleRightsStatement(idtyp.InnerText, idval.InnerText)

        If rts2 IsNot Nothing Then
          obj.LinkedRightsStatements.Add(rts2)
        Else
          obj.LinkedRightsStatements.Add(New PremisRightsStatement(idtyp.InnerText, idval.InnerText, "STUB"))
        End If

      Next

    Next

    For Each evt As PremisEvent In Events
      Dim nd As XmlElement = xml.SelectSingleNode(String.Format("/premis:premis/premis:event[premis:eventIdentifier/premis:eventIdentifierType='{0}' and premis:eventIdentifier/premis:eventIdentifierValue='{1}']", evt.EventIdentifier.IdentifierType, evt.EventIdentifier.IdentifierValue), xmlns)

      Dim lnkAgtNds As XmlNodeList = nd.SelectNodes("premis:linkingAgentIdentifier", xmlns)
      For Each agtNd As XmlElement In lnkAgtNds
        Dim idtyp = agtNd.Item("linkingAgentIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = agtNd.Item("linkingAgentIdentifierValue", PremisContainer.PremisNamespace)
        Dim roles = agtNd.SelectNodes("premis:linkingAgentRole", xmlns)
        Dim roleLst As New List(Of String)
        For Each role As XmlElement In roles
          roleLst.Add(role.InnerText)
        Next

        Dim agt2 = Me.FindSingleAgent(idtyp.InnerText, idval.InnerText)

        If agt2 IsNot Nothing Then
          evt.LinkedAgents.Add(agt2, roleLst)
        Else
          evt.LinkedAgents.Add(New PremisAgent(idtyp.InnerText, idval.InnerText), roleLst)
        End If

      Next

      Dim lnkObjNds As XmlNodeList = nd.SelectNodes("premis:linkingObjectIdentifier", xmlns)
      For Each objNd As XmlElement In lnkObjNds
        Dim idtyp = objNd.Item("linkingObjectIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = objNd.Item("linkingObjectIdentifierValue", PremisContainer.PremisNamespace)
        Dim roles = objNd.SelectNodes("premis:linkingObjectRole", xmlns)
        Dim roleLst As New List(Of String)
        For Each role As XmlElement In roles
          roleLst.Add(role.InnerText)
        Next

        Dim obj2 = Me.FindSingleObject(idtyp.InnerText, idval.InnerText)

        If obj2 IsNot Nothing Then
          evt.LinkedObjects.Add(obj2, roleLst)
        Else
          evt.LinkedObjects.Add(New PremisObject(idtyp.InnerText, idval.InnerText, PremisObjectCategory.ObjectStub), roleLst)
        End If

      Next

    Next

    For Each agt As PremisAgent In Agents
      Dim nd As XmlElement = xml.SelectSingleNode(String.Format("/premis:premis/premis:agent[premis:agentIdentifier/premis:agentIdentifierType='{0}' and premis:agentIdentifier/premis:agentIdentifierValue='{1}']", "LOCAL", agt.LocalIdentifierValue), xmlns)

      Dim lnkEvtNds As XmlNodeList = nd.SelectNodes("premis:linkingEventIdentifier", xmlns)
      For Each evtNd As XmlElement In lnkEvtNds
        Dim idtyp = evtNd.Item("linkingEventIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = evtNd.Item("linkingEventIdentifierValue", PremisContainer.PremisNamespace)

        Dim evt2 = Me.FindSingleEvent(idtyp.InnerText, idval.InnerText)

        If evt2 IsNot Nothing Then
          agt.LinkedEvents.Add(evt2)
        Else
          agt.LinkedEvents.Add(New PremisEvent(idtyp.InnerText, idval.InnerText, "STUB"))
        End If

      Next

      Dim lnkRtNds As XmlNodeList = nd.SelectNodes("premis:linkingRightsStatementIdentifier", xmlns)
      For Each rtNd As XmlElement In lnkRtNds
        Dim idtyp = rtNd.Item("linkingRightsStatementIdentifierType", PremisContainer.PremisNamespace)
        Dim idval = rtNd.Item("linkingRightsStatementIdentifierValue", PremisContainer.PremisNamespace)

        Dim rt2 = Me.FindSingleRightsStatement(idtyp.InnerText, idval.InnerText)

        If rt2 IsNot Nothing Then
          agt.LinkedRightsStatements.Add(rt2)
        Else
          agt.LinkedRightsStatements.Add(New PremisRightsStatement(idtyp.InnerText, idval.InnerText, "STUB"))
        End If

      Next

    Next

    For Each rt As PremisRights In Rights
      For Each rts As PremisRightsStatement In rt.RightsStatements

        Dim nd As XmlElement = xml.SelectSingleNode(String.Format("/premis:premis/premis:rights/premis:rightsStatement[premis:rightsStatementIdentifier/premis:rightsStatementIdentifierType='{0}' and premis:rightsStatementIdentifier/premis:rightsStatementIdentifierValue='{1}']", rts.RightsStatementIdentifier.IdentifierType, rts.RightsStatementIdentifier.IdentifierValue), xmlns)

        Dim lnkAgtNds As XmlNodeList = nd.SelectNodes("premis:linkingAgentIdentifier", xmlns)
        For Each agtNd As XmlElement In lnkAgtNds
          Dim idtyp = agtNd.Item("linkingAgentIdentifierType", PremisContainer.PremisNamespace)
          Dim idval = agtNd.Item("linkingAgentIdentifierValue", PremisContainer.PremisNamespace)
          Dim roles = agtNd.SelectNodes("premis:linkingAgentRole", xmlns)
          Dim roleLst As New List(Of String)
          For Each role As XmlElement In roles
            roleLst.Add(role.InnerText)
          Next

          Dim agt2 = Me.FindSingleAgent(idtyp.InnerText, idval.InnerText)

          If agt2 IsNot Nothing Then
            rts.LinkedAgents.Add(agt2, roleLst)
          Else
            rts.LinkedAgents.Add(New PremisAgent(idtyp.InnerText, idval.InnerText), roleLst)
          End If

        Next

        Dim lnkObjNds As XmlNodeList = nd.SelectNodes("premis:linkingObjectIdentifier", xmlns)
        For Each objNd As XmlElement In lnkObjNds
          Dim idtyp = objNd.Item("linkingObjectIdentifierType", PremisContainer.PremisNamespace)
          Dim idval = objNd.Item("linkingObjectIdentifierValue", PremisContainer.PremisNamespace)
          Dim roles = objNd.SelectNodes("premis:linkingObjectRole", xmlns)
          Dim roleLst As New List(Of String)
          For Each role As XmlElement In roles
            roleLst.Add(role.InnerText)
          Next

          Dim obj2 = Me.FindSingleObject(idtyp.InnerText, idval.InnerText)

          If obj2 IsNot Nothing Then
            rts.LinkedObjects.Add(obj2, roleLst)
          Else
            rts.LinkedObjects.Add(New PremisObject(idtyp.InnerText, idval.InnerText, PremisObjectCategory.ObjectStub), roleLst)
          End If

        Next

      Next
    Next
  End Sub

  Public Sub GetXMLRoot(xmlwr As XmlWriter)
    xmlwr.WriteStartElement("premis", PremisContainer.PremisNamespace)
    xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "info:lc/xmlns/premis-v2 http://www.loc.gov/standards/premis/v2/premis-v2-1.xsd")
    xmlwr.WriteAttributeString("version", "2.1")
    xmlwr.WriteAttributeString("xmlns", "xlink", Nothing, "http://www.w3.org/1999/xlink")
  End Sub

  Public Function GetXML() As String
    Dim sb As New StringBuilder()
    Dim xmlwr As XmlWriter = XmlWriter.Create(sb, New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})

    Me.GetXMLRoot(xmlwr)

    For Each pr As PremisObject In Objects
      pr.GetXML(xmlwr, Me)
    Next

    For Each pr As PremisEvent In Events
      pr.GetXML(xmlwr, Me)
    Next

    For Each pr As PremisAgent In Agents
      pr.GetXML(xmlwr, Me)
    Next

    For Each pr As PremisRights In Rights
      pr.GetXML(xmlwr, Me)
    Next

    xmlwr.WriteEndElement()
    xmlwr.Close()

    Dim xmlStr As String = sb.ToString

    Validate(xmlStr)

    Return xmlStr

  End Function

  Private Sub Validate(ByVal xmlStr As String)

    If Not ValidateXML Then Exit Sub

    If _schemas Is Nothing Then
      _schemas = New XmlSchemaSet
      _schemas.Add(PremisContainer.PremisNamespace, "http://www.loc.gov/standards/premis/v2/premis-v2-1.xsd")
    End If

    Dim document As XmlDocument = New XmlDocument()
    document.Schemas.Add(_schemas)
    document.LoadXml(xmlStr)
    Dim validation As ValidationEventHandler = New ValidationEventHandler(AddressOf SchemaValidationHandler)
    document.Validate(validation)

  End Sub

  Private Sub SchemaValidationHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)

    Select Case e.Severity
      Case XmlSeverityType.Error
        Throw New XmlSchemaException(e.Message)
        Exit Sub
      Case XmlSeverityType.Warning
        Throw New XmlSchemaException(e.Message)
        Exit Sub
    End Select

  End Sub

  Public Sub SaveXML(ByVal fileName As String)
    Dim xmlStr As String = Me.GetXML

    Dim txtWr As New StreamWriter(fileName, False, Encoding.UTF8)
    txtWr.Write(xmlStr)
    txtWr.Close()

  End Sub

  ''' <summary>
  ''' This function creates a separate XML file for each grouping of files that map to a fedora representation object
  ''' </summary>
  ''' <param name="folder"></param>
  ''' <param name="createSubDirs"></param>
  ''' <remarks></remarks>
  Public Sub SavePartitionedXML(folder As String, createSubDirs As Boolean)
    Dim conLst As List(Of PremisContainer) = Me.PartitionContainer()

    If createSubDirs Then
      For Each c As PremisContainer In conLst
        Dim folder2 As String = c.Objects.First.GetDefaultFileName("", "")
        Directory.CreateDirectory(Path.Combine(folder, folder2))
        For Each obj In c.Objects.Where(Function(o) o.ObjectCategory = PremisObjectCategory.File)
          File.Move(Path.Combine(folder, obj.GetFilenameIdentifier.IdentifierValue), Path.Combine(folder, folder2, obj.GetFilenameIdentifier.IdentifierValue))
          'obj.GetFilenameIdentifier.IdentifierValue = Path.Combine(folder, folder2, obj.GetFilenameIdentifier.IdentifierValue)
        Next
        c.SaveXML(Path.Combine(folder, folder2, c.Objects.First.GetDefaultFileName("premis_", "xml")))
      Next
    Else
      For Each c As PremisContainer In conLst
        c.SaveXML(Path.Combine(folder, c.Objects.First.GetDefaultFileName("premis_", "xml")))
      Next
    End If

  End Sub

  Public Sub SaveEachXML(folder As String)


    For Each pr As PremisObject In Objects
      pr.SaveXML(Path.Combine(folder, pr.GetDefaultFileName("premis_object_", "xml")))
    Next

    For Each pr As PremisEvent In Events
      pr.SaveXML(Path.Combine(folder, pr.GetDefaultFileName("premis_event_", "xml")))
    Next

    For Each pr As PremisAgent In Agents
      pr.SaveXML(Path.Combine(folder, pr.GetDefaultFileName("premis_agent_", "xml")))
    Next

    For Each pr As PremisRights In Rights
      pr.SaveXML(Path.Combine(folder, pr.GetDefaultFileName("premis_rights_", "xml")))
    Next


  End Sub

  Public ReadOnly Property NextID As String
    Get
      Dim ret As String = MakeID(_nextId)
      _nextId = _nextId + 1
      Return ret
    End Get
  End Property

  Public ReadOnly Property NextLocalIdentifier As PremisIdentifier
    Get
      Return New PremisIdentifier(Me.NextID)
    End Get
  End Property

  Public Function MakeID(index As Integer) As String
    Dim ret As String = String.Format("{0}{1:00000}", IDPrefix, index)
    Return ret
  End Function

  Public Function MakeID(index As String) As String
    Dim ret As String = String.Format("{0}{1}", IDPrefix, index)
    Return ret
  End Function

  Public Property IDPrefix As String

  ''' <summary>
  ''' Partition this container into multiple other containers one for each premis representation object
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function PartitionContainer() As List(Of PremisContainer)
    Dim containerList As New List(Of PremisContainer)

    For Each pObj As PremisObject In Me.Objects.Where(Function(o) o.ObjectCategory = PremisObjectCategory.Representation)
      Dim cont As New PremisContainer()

      containerList.Add(cont)

      Me.AddObjectsAndChildren(pObj, cont)


    Next

    Return containerList
  End Function

  Private Sub AddEventAndChildren(evt As PremisEvent, cont As PremisContainer)
    If Not cont.Events.Contains(evt) Then
      cont.Events.Add(evt)

      For Each agt As PremisAgent In evt.LinkedAgents.Keys
        Me.AddAgentAndChildren(agt, cont)
      Next

      For Each obj As PremisObject In evt.LinkedObjects.Keys.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
        Me.AddObjectsAndChildren(obj, cont)
      Next

    End If
  End Sub

  Private Sub AddAgentAndChildren(agt As PremisAgent, cont As PremisContainer)
    If Not cont.Agents.Contains(agt) Then
      cont.Agents.Add(agt)

      'NOTE: linked events and rights should end up in the contained via other means, so we do not enumerate htem here

    End If
  End Sub

  Private Sub AddRightsStatementAndChildren(rgtS As PremisRightsStatement, cont As PremisContainer)
    'make sure not already added
    For Each r As PremisRights In cont.Rights
      If r.RightsStatements.Contains(rgtS) Then Exit Sub
    Next

    Dim rgt As New PremisRights(rgtS)
    If Not cont.Rights.Contains(rgt) Then
      cont.Rights.Add(rgt)

      For Each agt As PremisAgent In rgtS.LinkedAgents.Keys
        Me.AddAgentAndChildren(agt, cont)
      Next

    End If
  End Sub

  Private Sub AddObjectsAndChildren(obj As PremisObject, cont As PremisContainer)
    If Not cont.Objects.Contains(obj) Then
      cont.Objects.Add(obj)

      For Each pEvt As PremisEvent In obj.LinkedEvents
        Me.AddEventAndChildren(pEvt, cont)
      Next

      For Each pRgtS As PremisRightsStatement In obj.LinkedRightsStatements
        Me.AddRightsStatementAndChildren(pRgtS, cont)
      Next

      For Each relat As PremisRelationship In obj.Relationships
        Dim i As Integer = 0
        For Each obj2 As PremisObject In relat.RelatedObjects.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
          Me.AddObjectsAndChildren(obj2, cont)
          i = i + 1
        Next
        If i > 0 Then
          For Each evt As PremisEvent In relat.RelatedEvents
            Me.AddEventAndChildren(evt, cont)
            i = i + 1
          Next
        End If
      Next
    End If
  End Sub

End Class

