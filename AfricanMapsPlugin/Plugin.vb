Imports Uiuc.Library.ContentDMPluginAPI
Imports System.Configuration
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Net
Imports System.IO

Public Class Plugin
  Inherits Uiuc.Library.ContentDMPluginAPI.Plugin
  Implements IPlugin


  Public Overrides ReadOnly Property PluginName As String
    Get
      Return "AfricanMaps"
    End Get
  End Property

End Class
