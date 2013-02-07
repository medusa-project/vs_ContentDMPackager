Imports System.Xml
Imports System.IO
Imports System.Text


Public Class PremisRightsStatement

  Public Property RightsStatementIdentifier As PremisIdentifier

  Public Property RightsBasis As String

  Public Property CopyrightInformation As PremisCopyrightInformation

  Public Property RightsGranted As List(Of PremisRightsGranted)

  Public Property LinkedAgents As Dictionary(Of PremisAgent, List(Of String))

  Public Property LinkedObjects As Dictionary(Of PremisObject, List(Of String))

  Public Sub New(elem As XmlElement)
    LinkedAgents = New Dictionary(Of PremisAgent, List(Of String))
    LinkedObjects = New Dictionary(Of PremisObject, List(Of String))
    RightsGranted = New List(Of PremisRightsGranted)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:rightsStatementIdentifier", xmlns)
    For Each nd As XmlElement In nds
      RightsStatementIdentifier = New PremisIdentifier(nd.Item("rightsStatementIdentifierType", PremisContainer.PremisNamespace).InnerText, nd.Item("rightsStatementIdentifierValue", PremisContainer.PremisNamespace).InnerText)
    Next

    nds = elem.SelectNodes("premis:rightsBasis", xmlns)
    For Each nd As XmlElement In nds
      RightsBasis = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:copyrightInformation", xmlns)
    For Each nd As XmlElement In nds
      CopyrightInformation = New PremisCopyrightInformation(nd)
    Next

    nds = elem.SelectNodes("premis:rightsGranted", xmlns)
    For Each nd As XmlElement In nds
      RightsGranted.Add(New PremisRightsGranted(nd))
    Next


  End Sub

  Public Sub LinkToAgent(ByVal agt As PremisAgent, Optional twoWay As Boolean = False)
    LinkedAgents.Add(agt, New List(Of String))
    If twoWay = True Then
      'Reverse Link
      If Not agt.LinkedRightsStatements.Contains(Me) Then
        agt.LinkToRightsStatement(Me)
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
      If Not obj.LinkedRightsStatements.Contains(Me) Then
        obj.LinkToRightsStatement(Me)
      End If
    End If
  End Sub

  Public Sub LinkToObject(ByVal obj As PremisObject, ByVal role As String, Optional twoWay As Boolean = True)
    Me.LinkToObject(obj, twoWay)
    LinkedObjects.Item(obj).Add(role)
  End Sub

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal idType As String, ByVal idValue As String, ByVal basis As String)
    RightsStatementIdentifier = New PremisIdentifier(idType, idValue)
    RightsBasis = basis
    LinkedAgents = New Dictionary(Of PremisAgent, List(Of String))
    LinkedObjects = New Dictionary(Of PremisObject, List(Of String))
    RightsGranted = New List(Of PremisRightsGranted)
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("rightsStatement")

    xmlwr.WriteStartElement("rightsStatementIdentifier")
    xmlwr.WriteElementString("rightsStatementIdentifierType", RightsStatementIdentifier.IdentifierType)
    xmlwr.WriteElementString("rightsStatementIdentifierValue", RightsStatementIdentifier.IdentifierValue)
    xmlwr.WriteEndElement()

    xmlwr.WriteElementString("rightsBasis", RightsBasis)

    If CopyrightInformation IsNot Nothing Then
      CopyrightInformation.GetXML(xmlwr)
    End If

    For Each grant As PremisRightsGranted In RightsGranted
      grant.GetXML(xmlwr)
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
End Class

