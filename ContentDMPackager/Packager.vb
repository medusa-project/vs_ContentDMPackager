Imports System.Xml
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Text
Imports System.Security
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.Ldap
Imports System.Net

Module Packager
  Const DEBUG_MAX_COUNT = 20 'Integer.MaxValue
  Private IdMap As New Dictionary(Of Integer, String)

  ''' <summary>
  ''' "Remember the Maine!"
  ''' </summary>
  ''' <remarks></remarks>
  Sub Main()

    'need to deal with SSL
    ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateCert


    'set working folder
    Directory.SetCurrentDirectory(ConfigurationManager.AppSettings.Item("WorkingFolder"))

    'init tracing/logging
    Trace.Listeners.Clear()
    Trace.Listeners.Add(New TextWriterTraceListener(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), "ContentDMPackager.log")))

    Trace.TraceInformation("Started: {0} By: {1} ", Now.ToString("s"), Principal.WindowsIdentity.GetCurrent.Name)
    Trace.TraceInformation("Working Folder: {0}", Directory.GetCurrentDirectory)
    Trace.TraceInformation("Export File: {0}", ConfigurationManager.AppSettings.Item("ContentDMExportFile"))

    Dim collHandle As String = ProcessCollection()

    ProcessExport(collHandle)

    Trace.TraceInformation("Finished: {0}", Now.ToString("s"))
    Trace.Flush()

    Console.Out.WriteLine("Done.  Press Enter to finish.")
    Console.In.ReadLine()

  End Sub

  ''' <summary>
  ''' Process the export records, all belonging to the given collection
  ''' </summary>
  ''' <param name="collHandle"></param>
  ''' <remarks></remarks>
  Private Sub ProcessExport(ByVal collHandle As String)

    Dim fileName As String = Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("ContentDMExportFile"))

    If File.Exists(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile"))) Then
      LoadIdMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile")))
    End If

    Dim xmlStr As String = FixExport(fileName)

    Dim xml As New XmlDocument
    xml.PreserveWhitespace = True

    xml.LoadXml(xmlStr)

    Dim cpd As CpdNode = CpdNode.Create(xml)

    Dim cnt As Integer = 0
    'first process non-compound objects
    Console.Out.WriteLine("Processing simple objects...")
    For Each pg As CpdPage In cpd.Pages
      Dim processor As New ContentDMRecordProcessor(collHandle, IdMap)
      processor.ProcessRecord(xml.SelectSingleNode(String.Format("/metadata/record[contentdm_number='{0}']", pg.PagePtr)), Nothing)
      cnt = cnt + 1
      If cnt >= DEBUG_MAX_COUNT Then Exit For 'for debugging
    Next
    cnt = 0
    'next process compound objects
    Console.Out.WriteLine("Processing compound objects...")
    For Each nd As CpdNode In cpd.Nodes
      Dim processor As New ContentDMRecordProcessor(collHandle, IdMap)
      processor.ProcessRecord(xml.SelectSingleNode(String.Format("/metadata/record[contentdm_number='{0}']", nd.ContentDmNumber)), nd)
      cnt = cnt + 1
      If cnt >= DEBUG_MAX_COUNT Then Exit For 'for debugging
    Next

    SaveIdMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile")))

  End Sub

  Private Sub SaveIdMap(fileName As String)
    'If Not File.Exists(fileName) Then
    Dim fs As New StreamWriter(fileName)
    For Each k In IdMap
      fs.WriteLine(String.Format("{0},{1}", k.Key, k.Value))
    Next
    fs.Close()
    'End If
  End Sub

  Private Sub LoadIdMap(filename As String)
    Dim fs As New StreamReader(filename)
    Do Until fs.EndOfStream
      Dim ln As String = fs.ReadLine
      Dim parts() As String = ln.Split(",", 2, StringSplitOptions.RemoveEmptyEntries)
      IdMap.Add(parts(0), parts(1))
    Loop
    fs.Close()
  End Sub

  ''' <summary>
  ''' Create a collection record package for the exported ContentDM collection.
  ''' If there is a value for the CollectionHandle AppSetting that CollectionHandle AppSetting will be used for all the records.  
  ''' If the CollectionHandle AppSetting
  ''' does not have a value, a new CollectionHandle will be created and used for the new collection record.  The 
  ''' CollectionHandle AppSetting should be set to this new Handle value for any subsequent runs of this script.
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks>This will just be a stub record and should be manually enhanced, if applicable.</remarks>
  Private Function ProcessCollection() As String
    Dim collHandle As String = ConfigurationManager.AppSettings.Item("CollectionHandle")

    'need to create collection record
    Dim pth As String = Path.Combine(ConfigurationManager.AppSettings.Item("ContentDMDataDir"), "index/etc/about.txt")
    Dim collDescr As String = File.ReadAllText(pth)

    Directory.CreateDirectory("collection")


    'create mods record for the collection
    Dim xmlwr As XmlWriter = XmlWriter.Create(Path.Combine("collection", "mods.xml"), New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})
    xmlwr.WriteStartElement("mods", "http://www.loc.gov/mods/v3")
    xmlwr.WriteAttributeString("version", "3.4")
    xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/mods.xsd")
    xmlwr.WriteComment("This collection mods record should be edited by hand to most accurately reflect the collection")
    xmlwr.WriteStartElement("typeOfResource")
    xmlwr.WriteAttributeString("collection", "yes")
    xmlwr.WriteString("mixed material")
    xmlwr.WriteEndElement()
    xmlwr.WriteStartElement("titleInfo")
    xmlwr.WriteElementString("title", ConfigurationManager.AppSettings.Item("ContentDMCollectionName"))
    xmlwr.WriteEndElement()
    xmlwr.WriteElementString("abstract", collDescr)
    xmlwr.WriteStartElement("location")
    xmlwr.WriteStartElement("url")
    xmlwr.WriteAttributeString("access", "object in context")
    xmlwr.WriteAttributeString("usage", "primary")
    xmlwr.WriteString(ConfigurationManager.AppSettings.Item("ContentDMCollectionURL"))
    xmlwr.WriteEndElement()
    xmlwr.WriteEndElement()
    xmlwr.Close()

    'Create a PREMIS metadata for the collection
    If String.IsNullOrWhiteSpace(collHandle) Then
      collHandle = MetadataFunctions.GenerateHandle
    End If
    Dim idType As String = "LOCAL"
    If collHandle.StartsWith(ConfigurationManager.AppSettings.Item("HandlePrefix") & "/") Then
      idType = "HANDLE"
    End If
    Dim pObj As New PremisObject(idType, collHandle, PremisObjectCategory.Representation)
    pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", ConfigurationManager.AppSettings.Item("ContentDMCollectionURL")))
    Dim pContainer As PremisContainer = New PremisContainer(pObj)
    pContainer.IDPrefix = collHandle & ConfigurationManager.AppSettings.Item("LocalIdSeparator")

    Dim currentAgent As PremisAgent = UIUCLDAPUser.GetPremisAgent
    currentAgent.AgentIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Agents.Add(currentAgent)

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
    pEvt.EventDetail = "SIP created from a ContentDM export record."
    pEvt.LinkToAgent(currentAgent)
    pEvt.LinkToObject(pObj)
    pContainer.Events.Add(pEvt)

    Dim pObj2 As New PremisObject("FILENAME", "mods.xml", PremisObjectCategory.File, "text/xml")
    pObj2.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Objects.Add(pObj2)

    'rename mods file to use uuid
    Dim newFName As String = pObj2.GetDefaultFileName("mods_", ".xml")
    If File.Exists(Path.Combine("collection", newFName)) Then
      File.Delete(Path.Combine("collection", newFName))
    End If
    Rename(Path.Combine("collection", "mods.xml"), Path.Combine("collection", newFName))
    pObj2.GetFilenameIdentifier.IdentifierValue = newFName

    Dim pEvt2 As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
    pEvt2.EventDetail = String.Format("The {0} file was derived from ContentDM collection data.  It is expected to be manually edited to add data.", newFName)
    pEvt2.LinkToAgent(currentAgent)
    pEvt2.LinkToObject(pObj2)
    pContainer.Events.Add(pEvt2)

    pObj.RelateToObject("METADATA", "HAS_ROOT", pObj2)

    If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")) Then
      Dim pRgtStmt As New PremisRightsStatement("LOCAL", pContainer.NextID, ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis"))
      pRgtStmt.RightsGranted.Add(New PremisRightsGranted(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")))
      pRgtStmt.LinkToObject(pObj)
      If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRightsRestrictions")) Then
        pRgtStmt.RightsGranted.FirstOrDefault.Restrictions.Add(ConfigurationManager.AppSettings.Item("PremisDisseminationRightsRestrictions"))
      End If
      If ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis") = "COPYRIGHT" And
        Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationCopyrightStatus")) Then
        Dim cpyRt As New PremisCopyrightInformation(ConfigurationManager.AppSettings.Item("PremisDisseminationCopyrightStatus"), "United States")
        pRgtStmt.CopyrightInformation = cpyRt
      End If
      Dim pRt As New PremisRights(pRgtStmt)
      pContainer.Rights.Add(pRt)
    End If

    pContainer.SaveXML(Path.Combine("collection", pObj.GetDefaultFileName("premis_object_", "xml")))

    'pContainer.SaveEachXML(Path.Combine("collection", "_").TrimEnd("_"))


    Return collHandle
  End Function

  ''' <summary>
  ''' ContentDM has a bug which mishandles ampersand entities, incorrectly splitting values on the semicolon
  ''' which is dropped from the entity
  ''' </summary>
  ''' <param name="fname"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function FixExport(ByVal fname As String) As String
    Dim strRdr As New StreamReader(fname, True)

    Dim str As String = strRdr.ReadToEnd

    strRdr.Close()

    Dim re As New Regex("&amp</([^>]+)>\s+<\1>")

    str = re.Replace(str, "&amp; ")

    Return str
  End Function

  Function ValidateCert(ByVal sender As Object, _
    ByVal cert As System.Security.Cryptography.X509Certificates.X509Certificate, _
    ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, _
    ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
    If ConfigurationManager.AppSettings.Item("IgnoreBadCert").ToUpper = "TRUE" Then
      Return True
    Else
      Return sslPolicyErrors = Security.SslPolicyErrors.None
    End If
  End Function


End Module
