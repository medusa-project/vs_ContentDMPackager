Imports System.Xml
Imports System.IO
Imports System.Text

Public Class PremisCreatingApplication

  Public Property CreatingApplicationName As String

  Public Property CreatingApplicationVersion As String

  Public Property DateCreatedByApplication As String

  Public Sub New(ByVal application As String)
    CreatingApplicationName = application
  End Sub

  Public Sub New(ByVal application As String, ByVal version As String)
    CreatingApplicationName = application
    CreatingApplicationVersion = version
  End Sub

  Public Sub New(ByVal application As String, ByVal version As String, ByVal dt As Date)
    CreatingApplicationName = application
    CreatingApplicationVersion = version
    DateCreatedByApplication = dt.ToString("s")
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
      xmlwr.WriteElementString("dateCreatedByApplication", DateCreatedByApplication)
    End If
    xmlwr.WriteEndElement()
  End Sub


End Class
