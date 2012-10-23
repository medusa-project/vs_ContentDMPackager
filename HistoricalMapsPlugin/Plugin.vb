Imports Uiuc.Library.ContentDMPluginAPI
Imports System.Configuration
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Net

Public Class Plugin
  Inherits Uiuc.Library.ContentDMPluginAPI.Plugin
  Implements IPlugin


  ''' <summary>
  ''' For the Historical Maps collection, the image could come from several different places as
  ''' specified in the appSettings ArchivalMasterURLElement Xpath expression.  The format is never 
  ''' specified, so it must be guessed.
  ''' </summary>
  ''' <param name="record"></param>
  ''' <returns></returns>
  ''' <remarks>This is kind of kludgy since overlapping data is obtained from the app.config file and is also
  ''' hardcoded into this function, but that's why we have a plugin architecture, so we can do funky stuff in the Plugin.</remarks>
  Public Overrides Function GetArchivalMasterInfo(record As System.Xml.XmlNode) As List(Of ImageInfo)
    Dim ret As New List(Of ImageInfo)

    Dim elemUrlName As String = ConfigurationManager.AppSettings.Item("ArchivalMasterXPath")
    Dim jp2url As String = ConfigurationManager.AppSettings.Item("Jpeg2000BaseUrl")

    Dim nds1 As XmlNodeList = record.SelectNodes(elemUrlName)

    For i As Integer = 0 To nds1.Count - 1
      Dim iElem As XmlElement = nds1.Item(i)

      Select Case iElem.Name
        Case "jpeg2000_url"
          ret.Add(New ImageInfo(nds1.Item(i).InnerText, "image/jp2"))
        Case "view_mrsid_image_zoom"
          Dim url As String = GetUrlToSid(nds1.Item(i).InnerText)
          If Not String.IsNullOrWhiteSpace(url) Then
            ret.Add(New ImageInfo(url, "image/x-mrsid-image"))
          End If
          Dim url2 As String = GetUrlToJp2FromSid(nds1.Item(i).InnerText)
          If Not String.IsNullOrWhiteSpace(url2) Then
            ret.Add(New ImageInfo(url2, "image/jp2"))
          End If
        Case "contentdm_file_path"
          Dim url As String = GetUrlToJpeg(nds1.Item(i).InnerText)
          If Not String.IsNullOrWhiteSpace(url) Then
            If url.EndsWith(".cpd") Then
              ret.Add(New ImageInfo(url, "text/xml"))
            Else
              ret.Add(New ImageInfo(url, "image/jpeg"))
            End If
          End If
        Case "archival_file_name"
          'this element points to a *.tif, but the tif filename may be converted to a jp2 url in some cases
          Dim url As String = GetUrlToJp2FromTif(nds1.Item(i).InnerText)
          If Not String.IsNullOrWhiteSpace(url) Then
            ret.Add(New ImageInfo(url, "image/jp2"))
          End If
        Case Else
          ret.Add(New ImageInfo(nds1.Item(i).InnerText, "application/octet-stream"))
      End Select
    Next

    ret = GetBestImages(ret)

    Return ret
  End Function

  ''' <summary>
  ''' Out of the possible alternate images in the list, only return the best one. JP2 is best, JPEG is next, MrSID is next
  ''' finally just return whatever is in list
  ''' </summary>
  ''' <param name="lst"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function GetBestImages(lst As List(Of ImageInfo)) As List(Of ImageInfo)
    Dim ret As New List(Of ImageInfo)
    If lst.Count = 1 Then
      ret.AddRange(lst)
    Else
      Dim jp2 = lst.FirstOrDefault(Function(i) i.Format = "image/jp2")
      If jp2 IsNot Nothing Then
        ret.Add(jp2)
        Return ret
      End If
      Dim jpeg = lst.FirstOrDefault(Function(i) i.Format = "image/jpeg")
      If jpeg IsNot Nothing Then
        ret.Add(jpeg)
        Return ret
      End If
      Dim sid = lst.FirstOrDefault(Function(i) i.Format = "image/x-mrsid-image")
      If sid IsNot Nothing Then
        ret.Add(sid)
        Return ret
      End If
      ret.AddRange(lst)
    End If

    Return ret
  End Function

  Private Function GetUrlToJp2FromSid(n As String) As String

    Dim jp2url As String = ConfigurationManager.AppSettings.Item("Jpeg2000BaseUrl")

    Dim re As New Regex("^http://(.+)/cgi/sid/bin/show_gif\.plx\?client=(.+)&image=(.+)$")

    Dim ms As Match = re.Match(n)

    Dim ret As String = ""

    If ms.Success Then
      Dim image As String = ms.Groups.Item(3).Value
      ret = String.Format(jp2url, Path.GetFileNameWithoutExtension(image))
    End If

    Return ret
  End Function

  Private Function GetUrlToJp2FromTif(n As String) As String

    Dim jp2url As String = ConfigurationManager.AppSettings.Item("Jpeg2000BaseUrl")
    Dim nm As String = Path.GetFileNameWithoutExtension(n)
    Return String.Format(jp2url, nm)

  End Function

  Public Overrides ReadOnly Property PluginName As String
    Get
      Return "HistoricalMaps"
    End Get
  End Property


  Private Function GetUrlToSid(url As String) As String
    Dim re As New Regex("^http://(.+)/cgi/sid/bin/show_gif\.plx\?client=(.+)&image=(.+)$")

    Dim ms As Match = re.Match(url)

    Dim ret As String = ""

    If ms.Success Then
      Dim host As String = ms.Groups.Item(1).Value
      Dim client As String = ms.Groups.Item(2).Value
      Dim image As String = ms.Groups.Item(3).Value
      ret = String.Format("http://{0}/sid/{1}/{2}", host, client, image)
    End If

    Return ret
  End Function

  Private Function GetUrlToJpeg(url As String) As String
    Dim rootPath As String = Path.GetPathRoot(ConfigurationManager.AppSettings.Item("ContentDMDataDir"))

    Dim imgPath As String = Path.Combine(rootPath, url.Trim("/"))

    Dim u As New Uri(imgPath)

    Dim ret As String = u.AbsoluteUri

    Return ret
  End Function

  Public Overrides Function TestRecord(record As System.Xml.XmlNode) As Boolean
    Dim nd As XmlElement = record.SelectSingleNode("item_url|reference_url")

    Dim req As HttpWebRequest = WebRequest.Create(nd.InnerText)
    Dim rsp As HttpWebResponse = Nothing
    Try
      rsp = req.GetResponse
    Catch ex As Exception
      Return False
    End Try

    Dim strm As Stream = rsp.GetResponseStream
    Dim rdr As New StreamReader(strm)
    Dim str As String = rdr.ReadToEnd
    rdr.Close()

    'look for a javascript redirect 
    Dim re As New Regex("location\.replace\(""(.+)""\);")
    Dim m As Match = re.Match(str)

    If m.Success Then
      Dim pth As String = m.Groups.Item(1).Value

      Dim newurl As String = String.Format("{0}{1}", req.RequestUri.GetLeftPart(UriPartial.Authority), pth)

      req = WebRequest.Create(newurl)
      Try
        rsp = req.GetResponse
      Catch ex As Exception
        Return False
      End Try

      Dim strm2 As Stream = rsp.GetResponseStream
      Dim rdr2 As New StreamReader(strm2)
      Dim str2 As String = rdr2.ReadToEnd
      rdr2.Close()

      If str2.Contains("This is not a compound object") Then
        Return False
      Else
        Return True
      End If
    End If

    Return False
  End Function
End Class
