Imports System.Xml
Imports System.IO
Imports System.Configuration
Imports Uiuc.Library.Premis

Public Enum CpdType
  Branch = 0
  Root = 1
  Document = 2
  Monograph = 3
  Postcard = 4
  PictureCube = 5
End Enum

Public Class CpdNode
  Public Property Type As CpdType
  Public Property NodeTitle As String
  Public Property Pages As List(Of CpdPage)
  Public Property Nodes As List(Of CpdNode)
  Public Property ParentNode As CpdNode
  Public Property ContentDmNumber As Integer
  Public Property Xml As XmlDocument

  'go up tree until find first xml
  Public ReadOnly Property RootNodeWithXml As CpdNode
    Get
      Dim ret As CpdNode = Me
      Do
        If ret IsNot Nothing AndAlso ret.Xml Is Nothing Then ret = ret.ParentNode
      Loop Until ret Is Nothing OrElse ret.Xml IsNot Nothing
      Return ret
    End Get
  End Property


  ''' <summary>
  ''' Given either a contentdm export record or a contentdm compound object file
  ''' create a hierarchy of cpdnodes returning the root node
  ''' </summary>
  ''' <param name="Xml"></param>
  ''' <returns></returns>
  ''' <remarks>If there are no compound objects, a flat list of all pages (contentDM records) will be contained in the root node Pages property.</remarks>
  Public Shared Function Create(Xml As XmlDocument) As CpdNode
    Dim rootPath As String = Path.GetPathRoot(ConfigurationManager.AppSettings.Item("ContentDMDataDir"))

    Dim rootNode As New CpdNode(CpdType.Root)

    If Xml.DocumentElement.Name = "metadata" Then
      'first look for compound objects
      Dim nds As XmlNodeList = Xml.SelectNodes("/metadata/record")
      For Each nd As XmlNode In nds
        Dim fpElem As XmlElement = nd.SelectSingleNode("contentdm_file_path")
        Dim numElem As XmlElement = nd.SelectSingleNode("contentdm_number")
        If fpElem.InnerText.EndsWith(".cpd") Then
          'some characters can be bad so load it into string before loading into xml
          Dim cpdPath As String = Path.Combine(rootPath, fpElem.InnerText.Trim("/"))
          Dim strRdr As New StreamReader(cpdPath, True)
          Dim str As String = strRdr.ReadToEnd
          strRdr.Close()
          Dim cpdXml As New XmlDocument
          cpdXml.LoadXml(str)

          Dim cpdNd As CpdNode = CpdNode.Create(cpdXml)
          cpdNd.ContentDmNumber = numElem.InnerText
          cpdNd.Xml = cpdXml
          cpdNd.NodeTitle = Path.Combine(rootPath, fpElem.InnerText.Trim("/"))

          rootNode.AddNode(cpdNd)
        End If
      Next

      'next do another pass through the export xml document and add any records that do not belong to a compound object
      For Each nd As XmlNode In nds
        Dim numElem As XmlElement = nd.SelectSingleNode("contentdm_number")
        Dim cpdNd As CpdNode = rootNode.FindNode(numElem.InnerText)
        If cpdNd Is Nothing Then
          Dim ttl As String = nd.SelectSingleNode("title").InnerText
          Dim file As String = nd.SelectSingleNode("contentdm_file_name").InnerText
          Dim ptr As String = nd.SelectSingleNode("contentdm_number").InnerText
          Dim pg As New CpdPage(ttl, file, ptr)
          rootNode.AddPage(pg)
        End If
      Next

    ElseIf Xml.DocumentElement.Name = "cpd" Then
      Dim typElem As XmlElement = Xml.SelectSingleNode("/cpd/type")
      rootNode = New CpdNode(CpdNode.GetCpdType(typElem.InnerText))
      'Parse pages and nodes inside of cpd
      Dim nds As XmlNodeList = Xml.SelectNodes("/cpd/page|/cpd/node")
      For Each nd As XmlElement In nds
        Dim cpdObj As Object = CpdNode.Create(nd)
        If TypeOf cpdObj Is CpdNode Then
          rootNode.AddNode(cpdObj)
        ElseIf TypeOf cpdObj Is CpdPage Then
          rootNode.AddPage(cpdObj)
        Else
          Throw New Exception(String.Format("Unexpected object '{0}'", TypeName(cpdObj)))
        End If
      Next

    End If

    Return rootNode
  End Function

  ''' <summary>
  ''' Return either a CpdPage or CpdNode object for the given cpd element
  ''' </summary>
  ''' <param name="elem"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Shared Function Create(elem As XmlElement) As Object
    Dim ret As Object
    If elem.Name = "page" Then
      Dim ttl As String = elem.SelectSingleNode("pagetitle").InnerText
      Dim file As String = elem.SelectSingleNode("pagefile").InnerText
      Dim ptr As String = elem.SelectSingleNode("pageptr").InnerText
      ret = New CpdPage(ttl, file, ptr)
    ElseIf elem.Name = "node" Then
      Dim ttl As String = elem.SelectSingleNode("nodetitle").InnerText
      ret = New CpdNode(ttl)
      Dim nds As XmlNodeList = elem.SelectNodes("page|node")
      For Each nd As XmlElement In nds
        Dim cpdObj As Object = CpdNode.Create(nd)
        If TypeOf cpdObj Is CpdNode Then
          ret.AddNode(cpdObj)
        ElseIf TypeOf cpdObj Is CpdPage Then
          ret.AddPage(cpdObj)
        Else
          Throw New Exception(String.Format("Unexpected object '{0}'", TypeName(cpdObj)))
        End If
      Next
    Else
      Throw New Exception(String.Format("Unexpected element '{0}'", elem.Name))
    End If
    Return ret
  End Function

  ''' <summary>
  ''' Create new empty node
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub New()
    Type = CpdType.Branch
    Pages = New List(Of CpdPage)
    Nodes = New List(Of CpdNode)
  End Sub

  ''' <summary>
  ''' Create new empty node of the given type
  ''' </summary>
  ''' <param name="typ"></param>
  ''' <remarks></remarks>
  Public Sub New(typ As CpdType)
    Me.New()
    Type = typ
  End Sub

  ''' <summary>
  ''' Create new empty node with given title
  ''' </summary>
  ''' <param name="title"></param>
  ''' <remarks></remarks>
  Public Sub New(title As String)
    Me.New()
    NodeTitle = title
  End Sub

  ''' <summary>
  ''' Add a new page to the node
  ''' </summary>
  ''' <param name="title"></param>
  ''' <param name="file"></param>
  ''' <param name="ptr"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function AddPage(title As String, file As String, ptr As String) As CpdPage
    Dim page As New CpdPage(title, file, ptr)
    page.ParentNode = Me
    Me.Pages.Add(page)
    Return page
  End Function

  ''' <summary>
  ''' Add a new child node to the node
  ''' </summary>
  ''' <param name="title"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function AddNode(title As String) As CpdNode
    Dim node As New CpdNode(title)
    node.ParentNode = Me
    Nodes.Add(node)
    Return node
  End Function

  ''' <summary>
  ''' Add the given node to the node list
  ''' </summary>
  ''' <param name="node"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function AddNode(node As CpdNode) As CpdNode
    node.ParentNode = Me
    Nodes.Add(node)
    Return node
  End Function

  ''' <summary>
  ''' Add the given page to the page list
  ''' </summary>
  ''' <param name="pg"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function AddPage(pg As CpdPage) As CpdPage
    pg.ParentNode = Me
    Pages.Add(pg)
    Return pg
  End Function

  Public Shared Function GetCpdType(typeName As String) As CpdType
    Select Case typeName.ToLower
      Case "document"
        Return CpdType.Document
      Case "monograph"
        Return CpdType.Monograph
      Case "picture cube"
        Return CpdType.PictureCube
      Case "postcard"
        Return CpdType.Postcard
      Case Else
        Return CpdType.Branch
    End Select
  End Function

  ''' <summary>
  ''' Given a ContentDM Number return either the node that represent that number, or that contains the page for that number
  ''' </summary>
  ''' <param name="contentDmNumber"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function FindNode(contentDmNumber As Integer) As CpdNode
    Return CpdNode.FindNode(Me, contentDmNumber)
  End Function

  ''' <summary>
  ''' Given a ContentDM Number return the for that number
  ''' </summary>
  ''' <param name="contentDmNumber"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function FindPage(contentDmNumber As Integer) As CpdPage
    Return CpdNode.FindPage(Me, contentDmNumber)
  End Function

  ''' <summary>
  ''' Given a ContentDM Number return the page for that number
  ''' </summary>
  ''' <param name="node"></param>
  ''' <param name="num"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Shared Function FindPage(node As CpdNode, num As Integer) As CpdPage

    For Each pg As CpdPage In node.Pages
      If pg.PagePtr = num Then
        Return pg
      End If
    Next

    For Each nd As CpdNode In node.Nodes
      Dim fndPg As CpdPage = CpdNode.FindPage(nd, num)
      If fndPg IsNot Nothing Then
        Return fndPg
      End If
    Next

    Return Nothing
  End Function

  ''' <summary>
  ''' Given a ContentDM Number return either the node that represent that number, or that contains the page for that number
  ''' </summary>
  ''' <param name="node"></param>
  ''' <param name="num"></param>
  ''' <returns></returns>
  ''' <remarks>This use a brute-force linear search through the hierarchy.  Might want to add an index to the structure if this
  ''' becomes a bottle neck, but probably not critical since this is back office process anyway</remarks>
  Private Shared Function FindNode(node As CpdNode, num As Integer) As CpdNode
    If node.ContentDmNumber = num Then
      Return node
    Else
      For Each pg As CpdPage In node.Pages
        If pg.PagePtr = num Then
          Return node
        End If
      Next
      For Each nd As CpdNode In node.Nodes
        Dim fndNode As CpdNode = CpdNode.FindNode(nd, num)
        If fndNode IsNot Nothing Then
          Return fndNode
        End If
      Next
    End If

    Return Nothing
  End Function

  ''' <summary>
  ''' Using the given PremisContainer's NextID function assign local identifiers to all pages
  ''' </summary>
  ''' <param name="pCont"></param>
  ''' <remarks></remarks>
  Sub AddPremisLocalIdentifiersToAllPages(pCont As PremisContainer)
    For Each pg As CpdPage In Me.Pages
      pg.LocalIdentifier = pCont.NextID
    Next
    For Each nd As CpdNode In Me.Nodes
      nd.AddPremisLocalIdentifiersToAllPages(pCont)
    Next
  End Sub

End Class
