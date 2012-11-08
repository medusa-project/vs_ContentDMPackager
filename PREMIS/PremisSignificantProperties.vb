Imports System.Xml

Public Class PremisSignificantProperties
  Public Property SignificantPropertiesType As String

  Public Property SignificantPropertiesValue As String

  Public Property SignificantPropertiesExtensions As List(Of XmlDocument)

  Public Sub New()
    SignificantPropertiesExtensions = New List(Of XmlDocument)
  End Sub

  Public Sub New(type As String, value As String)
    Me.new()
    Me.SignificantPropertiesType = type
    Me.SignificantPropertiesValue = value
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("significantProperties")
    If Not String.IsNullOrWhiteSpace(SignificantPropertiesType) Then
      xmlwr.WriteElementString("significantPropertiesType", SignificantPropertiesType)
    End If
    If Not String.IsNullOrWhiteSpace(SignificantPropertiesValue) Then
      xmlwr.WriteElementString("significantPropertiesValue", SignificantPropertiesValue)
    End If
    For Each ext As XmlDocument In SignificantPropertiesExtensions
      xmlwr.WriteStartElement("significantPropertiesExtension")
      xmlwr.WriteNode(New XmlNodeReader(ext.DocumentElement), False)
      xmlwr.WriteEndElement()
    Next
    xmlwr.WriteEndElement()
  End Sub


End Class
