Imports System.Xml.Xsl
Imports System.IO
Imports System.Configuration
Imports System.Xml
Imports System.Text.RegularExpressions

Public MustInherit Class Plugin
  Implements IPlugin

  Private Shared xsl As XslCompiledTransform

  Public Overridable Function GetArchivalMasterInfo(record As System.Xml.XmlNode) As System.Collections.Generic.List(Of ImageInfo) Implements IPlugin.GetArchivalMasterInfo
    Dim ret As New List(Of ImageInfo)

    Dim elemUrlName As String = ConfigurationManager.AppSettings.Item("ArchivalMasterXPath")
    Dim nds1 As XmlNodeList = record.SelectNodes(elemUrlName)

    Dim elemFormatName As String = ConfigurationManager.AppSettings.Item("ArchivalMasterFormatXPath")
    Dim nds2 As XmlNodeList = record.SelectNodes(elemFormatName)

    For i As Integer = 0 To nds1.Count - 1
      ret.Add(New ImageInfo(nds1.Item(i).InnerText, nds2.Item(i).InnerText))
    Next

    Return ret

  End Function

  Public Overridable Function GetCatalogInfo(record As System.Xml.XmlNode) As CatalogInfo Implements IPlugin.GetCatalogInfo
    Dim elemName = ConfigurationManager.AppSettings.Item("CatalogIDXPath")
    Dim nd As XmlElement = record.SelectSingleNode(elemName)
    Dim catID As String = nd.InnerText

    ' fix call numbers, so they can be searched
    ' remove Quarto or Folio prefixes
    If catID.StartsWith("Q_") Or catID.StartsWith("Q.") Or catID.StartsWith("F_") Or catID.StartsWith("F.") Then
      catID = catID.Substring(2)
    End If
    catID = catID.Trim(";")
    catID = catID.Replace("_", ".")

    Dim url As String = String.Format(ConfigurationManager.AppSettings.Item("GetMarcUrl"), catID)

    url = Regex.Replace(url, "\.\.opac$", ".opac")

    Dim ret As New CatalogInfo(url, catID)

    Return ret

  End Function

  Public MustOverride ReadOnly Property PluginName As String Implements IPlugin.PluginName

  Public Overridable Function TestRecord(record As System.Xml.XmlNode) As Boolean Implements IPlugin.TestRecord
    Return True
  End Function

  Public Overridable Sub TransFormToMods(record As System.Xml.XmlNode, xslp As System.Xml.Xsl.XsltArgumentList, xwr As System.Xml.XmlWriter) Implements IPlugin.TransFormToMods
    If xsl Is Nothing Then
      xsl = New XslCompiledTransform
      xsl.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings.Item("RecordToModsXSLT")))
    End If

    xsl.Transform(record, xslp, xwr)

  End Sub
End Class
