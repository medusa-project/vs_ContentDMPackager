Imports System.Xml
Imports System.Xml.Xsl

Public Interface IPlugin

  ''' <summary>
  ''' Return the name of this Plugin
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  ReadOnly Property PluginName As String

  ''' <summary>
  ''' Return a list of URIs and Formats from which the Archival Master Images can be fetched
  ''' </summary>
  ''' <param name="record"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Function GetArchivalMasterInfo(record As XmlNode) As List(Of ImageInfo)

  ''' <summary>
  ''' Return the URI from which the MODS XML can be fetched from the library OPAC
  ''' And the Call Number or BibId used to fetch that record
  ''' </summary>
  ''' <param name="record"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Function GetCatalogInfo(record As XmlNode) As CatalogInfo

  ''' <summary>
  ''' Transform the ContentDM record into MODS and return the resulting XML
  ''' </summary>
  ''' <param name="record"></param>
  ''' <param name="xwr"></param>
  ''' <param name="xslp"></param>
  ''' <remarks></remarks>
  Sub TransFormToMods(record As XmlNode, xslp As XsltArgumentList, xwr As XmlWriter)

  ''' <summary>
  ''' Test whether the record is valid and retrievable from ContentDM w/o errors
  ''' </summary>
  ''' <param name="record"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Function TestRecord(record As XmlNode) As Boolean

End Interface

Public Class CatalogInfo
  Public Property Uri As String
  Public Property CatalogId As String

  Public Sub New(url As String, id As String)
    Uri = url
    CatalogId = id
  End Sub

End Class

Public Class ImageInfo
  Public Property Uri As String
  Public Property Format As String

  Public Sub New(url As String, form As String)
    Uri = url
    Format = form
  End Sub

  Public ReadOnly Property BaseFormat As String
    Get
      Dim parts() As String = Format.Split("/")
      Return parts(0)
    End Get
  End Property

End Class
