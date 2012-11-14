Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisCreatingApplication

  Public Property CreatingApplicationName As String

  Public Property CreatingApplicationVersion As String

  Public Property DateCreatedByApplication As String

  Public Property CreatingApplicationExtensions As List(Of XmlDocument)

  Public Sub New(elem As XmlElement)
    CreatingApplicationExtensions = New List(Of XmlDocument)

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("premis", PremisContainer.PremisNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("premis:creatingApplicationName", xmlns)
    For Each nd As XmlElement In nds
      CreatingApplicationName = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:creatingApplicationVersion", xmlns)
    For Each nd As XmlElement In nds
      CreatingApplicationVersion = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:dateCreatedByApplication", xmlns)
    For Each nd As XmlElement In nds
      DateCreatedByApplication = nd.InnerText
    Next

    nds = elem.SelectNodes("premis:creatingApplicationExtension", xmlns)
    For Each nd As XmlElement In nds
      CreatingApplicationExtensions.Add(nd.Clone)
    Next

  End Sub

  Public Sub New(ByVal application As String)
    CreatingApplicationName = application
    CreatingApplicationExtensions = New List(Of XmlDocument)
  End Sub

  Public Sub New(ByVal application As String, ByVal version As String)
    CreatingApplicationName = application
    CreatingApplicationVersion = version
    CreatingApplicationExtensions = New List(Of XmlDocument)
  End Sub

  Public Sub New(ByVal application As String, ByVal version As String, ByVal dt As Date)
    CreatingApplicationName = application
    CreatingApplicationVersion = version
    DateCreatedByApplication = dt.ToString("s")
    CreatingApplicationExtensions = New List(Of XmlDocument)
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("creatingApplication")
    If Not String.IsNullOrWhiteSpace(CreatingApplicationName) Then
      xmlwr.WriteElementString("creatingApplicationName", CreatingApplicationName)
    End If
    If Not String.IsNullOrWhiteSpace(CreatingApplicationVersion) Then
      xmlwr.WriteElementString("creatingApplicationVersion", CreatingApplicationVersion)
    End If
    If Not String.IsNullOrWhiteSpace(DateCreatedByApplication) Then
      Dim dts As DateTime
      If DateTime.TryParse(DateCreatedByApplication, dts) Then
        'if it can be interpreted as a date then format appropriately
        xmlwr.WriteElementString("dateCreatedByApplication", dts.ToString("yyyy-MM-ddTHH:mm:ssK"))
      Else
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'If its not a valid date it should be one of these patterns from the XML Schema, and will generate schema error if not
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '\d{2}(\d{2}|\?\?|\d(\d|\?))(-(\d{2}|\?\?))?~?\??
        '\d{6}(\d{2}|\?\?)~?\??
        '\d{8}T\d{6}
        '((\d{4}(-\d{2})?)|UNKNOWN)/((\d{4}(-\d{2})?)|UNKNOWN|OPEN)
        '\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}((Z|(\+|-)\d{2}:\d{2}))?/\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}((Z|(\+|-)\d{2}:\d{2}))?
        'OPEN
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        xmlwr.WriteElementString("dateCreatedByApplication", DateCreatedByApplication)
      End If
    End If
    For Each nd As XmlDocument In CreatingApplicationExtensions
      xmlwr.WriteStartElement("creatingApplicationExtension")
      xmlwr.WriteNode(nd.CreateNavigator(), False)
      xmlwr.WriteEndElement()
    Next
    xmlwr.WriteEndElement()
  End Sub


End Class
