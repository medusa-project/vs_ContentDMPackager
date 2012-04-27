Imports System.Xml
Imports System.Xml.Xsl
Imports System.Xml.Schema
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Net
Imports System.Text
Imports System.Security
Imports System.Security.Cryptography
Imports System.Reflection
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.ContentDMPluginAPI

''' <summary>
''' Process a single ContentDM record to create a Medusa Submission Package
''' </summary>
''' <remarks></remarks>
Public Class ContentDMRecordProcessor

  Const MAX_RETRY_COUNT = 5


  Private pContainer As PremisContainer
  Private pCurrentAgent As PremisAgent
  Private pRepresentation As PremisObject
  Private pMODS As PremisObject
  Private pMODSEvt As PremisEvent
  Private recNum As Integer
  Private recPath As String
  Private contentDmRecord As XmlElement
  Private compoundObject As CpdNode
  Private collHandle As String

  Private IdMap As Dictionary(Of Integer, String)

  Dim okFormats() As String = {"image/jp2", "image/tiff"}

  Private Const SOURCE_METADATA_FILE = "contentdm_record_{0}.xml"
  Private Const MODS_METADATA_FILE = "mods_{0}.xml"

  Private plugin As IPlugin


  Protected Sub New()
    'no public empty constructor is allowed
  End Sub

  Sub New(ByVal collectionHandle As String, im As Dictionary(Of Integer, String))
    collHandle = collectionHandle
    IdMap = im
    InitPlugin()
  End Sub


  ''' <summary>
  ''' Initialize the plugin used for the specific ContentDM collection.
  ''' The Plugin DLL file is specified in the AppSetting ContentDMPlugin field, and it must be named like "*Plugin.dll"
  ''' There must be a class in this DLL called Plugin and it must implement the IPlugin interface.
  ''' </summary>
  ''' <remarks></remarks>
  Private Sub InitPlugin()

    Dim appDir As String = My.Application.Info.DirectoryPath

    Dim availablePlugins As String() = Directory.GetFiles(appDir, "*Plugin.DLL")

    If availablePlugins.Count = 0 Then
      Trace.TraceError("No Plugin DLL files were found")
      Throw New Exception("No Plugin DLL files were found")
    End If

    Dim asmName As String = ConfigurationManager.AppSettings.Item("ContentDMPlugin")
    For Each possiblePlugin As String In availablePlugins
      If Path.GetFileNameWithoutExtension(possiblePlugin).ToLower = asmName.ToLower Then
        Dim asm As Assembly = Assembly.LoadFrom(possiblePlugin)
        Dim myType As System.Type = asm.GetTypes.SingleOrDefault(Function(t) t.Name = "Plugin")
        Dim implementsIPlugin As Boolean = GetType(IPlugin).IsAssignableFrom(myType)
        If implementsIPlugin Then
          plugin = CType(Activator.CreateInstance(myType), IPlugin)
        End If
      End If
    Next

    If plugin Is Nothing Then
      Trace.TraceError("No ContentDMPlugin was found in the plugin DLLs")
      Throw New Exception("No ContentDMPlugin was found in the plugin DLLs")
    End If

  End Sub

  ''' <summary>
  ''' Process the non-compound ContentDM record contained in the given XmlNode
  ''' </summary>
  ''' <param name="recNd">XmlNode containing ContentDM export record</param>
  ''' <param name="cpdNd">CpdNode containing Compound Object hierarchy, must be Nothing for non-compound objects</param>
  ''' <remarks>All folders and files will be relative to whatever the current directory is</remarks>
  Public Sub ProcessRecord(ByVal recNd As XmlNode, ByVal cpdNd As CpdNode)

    contentDmRecord = recNd
    compoundObject = cpdNd

    Dim recNumstr As String = Me.GetContentDmRecordProperty("contentdm_number")
    recNum = Integer.Parse(recNumstr)

    If plugin.TestRecord(recNd) = False Then
      Console.Out.WriteLine(String.Format("Record Number: {0}  Skipped: It is not accessible via ContentDM.", recNum))
      Trace.TraceError(String.Format("Record Number {0} is being skipped.  It is not accessible via ContentDM.", recNum))
      Exit Sub
    End If

    'Create subdirectories -- named for the ContentDM record number (max 1000 files per directory)
    Dim pathNum As String = Int(recNum / 1000).ToString 'maximum of 1000 records per subdirectory
    Directory.CreateDirectory(Path.Combine(pathNum, recNum.ToString))

    Console.Out.WriteLine(String.Format("Record Number: {0}", recNum))

    recPath = Path.Combine(pathNum, recNum.ToString)

    Dim handle As String
    If IdMap.ContainsKey(recNum) Then
      'use the already minted handle 
      handle = IdMap.Item(recNum)
      If Not MetadataFunctions.ValidateHandle(handle) Then Throw New Exception("Invalid Handle: " & handle)
    Else
      'Create a PREMIS metadata for the record
      handle = MetadataFunctions.GenerateHandle
      IdMap.Add(recNum, handle)
    End If

    Dim idType As String = "LOCAL"
    If handle.StartsWith(ConfigurationManager.AppSettings.Item("HandlePrefix") & "/") Then
      idType = "HANDLE"
    End If

    pRepresentation = New PremisObject(idType, handle, PremisObjectCategory.Representation)
    pRepresentation.XmlId = String.Format("pageptr_{0}", recNum)

    Dim pId As New PremisIdentifier("CONTENTDM_NUMBER", recNum)
    pRepresentation.ObjectIdentifiers.Add(pId)
    pContainer = New PremisContainer(pRepresentation)
    pContainer.IDPrefix = handle & ConfigurationManager.AppSettings.Item("LocalIdSeparator")

    If compoundObject IsNot Nothing Then
      'Cycle thru all the compoundObject pages and assign them LOCAL Identifier values that will be used later
      compoundObject.AddPremisLocalIdentifiersToAllPages(pContainer)

      'also add mods filename to the xml
      Dim pgLst As XmlNodeList = compoundObject.RootNodeWithXml.Xml.SelectNodes("//page")
      For Each pg As XmlElement In pgLst
        Dim num As Integer = pg.SelectSingleNode("pageptr").InnerText
        Dim mx As XmlElement = compoundObject.RootNodeWithXml.Xml.CreateElement("modsfile")
        Dim cPg As CpdPage = compoundObject.FindPage(num)
        mx.InnerXml = PremisObject.GetFileName(New PremisIdentifier(cPg.LocalIdentifier), "file", "mods_", "xml")
        pg.AppendChild(mx)
      Next
    End If

    If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")) Then
      Dim pRgtStmt As New PremisRightsStatement("LOCAL", pContainer.NextID, ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis"))
      pRgtStmt.RightsGranted.Add(New PremisRightsGranted(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")))
      pRgtStmt.LinkToObject(pRepresentation)
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

    idType = "LOCAL"
    If collHandle.StartsWith(ConfigurationManager.AppSettings.Item("HandlePrefix") & "/") Then
      idType = "HANDLE"
    End If

    pRepresentation.RelateToObject("COLLECTION", "IS_MEMBER_OF", idType, collHandle)

    pCurrentAgent = UIUCLDAPUser.GetPremisAgent
    pCurrentAgent.AgentIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Agents.Add(pCurrentAgent)

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
    'include details about packager program here
    pEvt.EventDetail = String.Format("SIP created from a ContentDM export record.  Program: {0} {1} [{2}], Computer: {3}, {4} V{5}, {6}",
                                     My.Application.Info.Title, My.Application.Info.Version, My.Application.Info.CompanyName, My.Computer.Name, My.Computer.Info.OSFullName.Trim,
                                     My.Computer.Info.OSVersion, My.Application.UICulture.EnglishName)
    pEvt.LinkToAgent(pCurrentAgent)
    pEvt.LinkToObject(pRepresentation)
    pContainer.Events.Add(pEvt)

    Dim recFile As String = SaveContentDmRecord(recNum, recNd)

    Dim catFile As String = CaptureCatalogRecord(recFile)

    Dim modsFile As String = MigrateRecordToMods(catFile, recFile)

    Dim masters As List(Of String) = CaptureArchivalMaster()

    'Dim accessFiles As List(Of String) = CaptureThumbImage(masters(0)) 'assume only one archival master
    'Dim thumbFiles As List(Of String) = CaptureAccessImage(masters(0)) 'assume only one archival master

    Select Case ConfigurationManager.AppSettings.Item("SaveFilesAs").ToLower
      Case "one"
        'save one big premis file for the whole object
        pContainer.SaveXML(Path.Combine(recPath, pContainer.Objects.First.GetDefaultFileName("premis_", "xml")))
      Case "multiple"
        'save a separate file for each premis entity
        pContainer.SaveEachXML(recPath)
      Case "representations"
        'create directory structure and save a separate premis file for each premis representation object and associated entities, 
        'also save a premis file for each file object (not metadata)
        pContainer.SavePartitionedXML(recPath, True)
      Case "medusa"
        'create directory structure and save a separate premis file for each fedora object as defined for our medusa content model
        'pContainer.SaveXML(Path.Combine(recPath, pContainer.Objects.First.GetDefaultFileName("test_premis_", "xml")))
        MedusaHelpers.SavePartitionedXML(pContainer, recPath, True)
      Case Else
        'save one big premis file for the whole object
        pContainer.SaveXML(Path.Combine(recPath, pContainer.Objects.First.GetDefaultFileName("premis_", "xml")))
    End Select

  End Sub

  Private Function SaveContentDmRecord(num As Integer, nd As XmlElement) As String
    'write the record out to an xml file
    Dim outFName As String = Path.Combine(recPath, String.Format(SOURCE_METADATA_FILE, num))
    Dim strwr As StreamWriter = File.CreateText(outFName)
    strwr.Write(nd.OuterXml)
    strwr.Close()

    Dim pChar As PremisObjectCharacteristics = PremisObjectCharacteristics.CharacterizeFile(outFName, "text/xml")

    Dim pObj1 As New PremisObject("FILENAME", String.Format(SOURCE_METADATA_FILE, num), PremisObjectCategory.File, pChar)
    pObj1.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_METADATA_FILE", Now))
    pObj1.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Objects.Add(pObj1)

    'rename the contentdm file to use the uuid
    Dim newFName As String = pObj1.GetDefaultFileName("contentdm_", ".xml")
    Rename(outFName, Path.Combine(recPath, newFName))
    pObj1.GetFilenameIdentifier.IdentifierValue = newFName

    Dim pEvt1 As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt1.EventDetail = "The original ContentDM records was extracted from an export file."
    pEvt1.LinkToAgent(pCurrentAgent)
    pEvt1.LinkToObject(pObj1)
    pContainer.Events.Add(pEvt1)


    Return newFName

  End Function

  ''' <summary>
  ''' If the ContentDM record has a call number or bib id field, it will be used to capture a snapshot of the library catalog for the record.
  ''' The catalog MARC record is captured including the holdings and circ data.  The captured catalog record is assumed to be the source, at least partially, of the 
  ''' ContentDM metadata for the item, so a DERIVATION HAS_SOURCE relationship is established between the ContentDM metadata and the catalog
  ''' metadata.
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function CaptureCatalogRecord(recFile As String) As String

    Dim catInfo As CatalogInfo = plugin.GetCatalogInfo(contentDmRecord)

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt.LinkToAgent(pCurrentAgent)
    pEvt.EventDetail = String.Format("Fetching bibliographic record, including MARC XML and holdings data, from the University of Illinois at Urbana-Champaign Library Catalog using web service URL: {0}", catInfo.Uri)
    pContainer.Events.Add(pEvt)

    Dim pObjChars As New PremisObjectCharacteristics("text/xml")
    Dim fn As String = Regex.Replace(catInfo.CatalogId.Replace(" ", ""), String.Format("[{0}]", New String(Path.GetInvalidFileNameChars)), "_") 'TODO: This could result in filename collisions, but it is renamed later so no problem
    Dim outFile As String = Path.Combine(recPath, fn & ".xml")

    Dim opacFile As String = FetchURL(catInfo.Uri, pEvt, pObjChars, outFile)

    If Not String.IsNullOrWhiteSpace(opacFile) Then
      Dim pObj As New PremisObject("FILENAME", opacFile, PremisObjectCategory.File, pObjChars)
      pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_METADATA_FILE", Now))
      pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", catInfo.Uri))
      pObj.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
      pContainer.Objects.Add(pObj)
      pObj.LinkToEvent(pEvt)

      'rename the catalog file to use the uuid
      Dim newFName As String = pObj.GetDefaultFileName("opac_", ".xml")
      Rename(outFile, Path.Combine(recPath, newFName))
      pObj.GetFilenameIdentifier.IdentifierValue = newFName
      opacFile = newFName

      Dim pObj2 As PremisObject = pContainer.FindSingleObject("FILENAME", recFile)
      pObj2.RelateToObject("DERIVATION", "HAS_SOURCE", pObj)

    End If



    Return opacFile
  End Function

  Private Function MigrateRecordToMods(ByVal catFile As String, recfile As String) As String
    Return MigrateRecordToMods(catFile, recfile, recNum, contentDmRecord)
  End Function


  Private Function MigrateRecordToMods(ByVal catFile As String, recFile As String, num As Integer, rec As XmlElement) As String
    Return MigrateRecordToMods(catFile, recFile, Nothing, num, rec)
  End Function


  Private Function MigrateRecordToMods(ByVal catFile As String, recFile As String, modsFile As String, num As Integer, rec As XmlElement) As String

    Dim saveFile As String
    If String.IsNullOrWhiteSpace(modsFile) Then
      saveFile = String.Format("mods_{0}.xml", num)
    Else
      saveFile = modsFile
    End If

    Dim xslp As New XsltArgumentList
    xslp.AddParam("current-date", "", Now.ToString("s"))
    xslp.AddParam("class-auth", "", MetadataFunctions.GetCallNumberAuthority(Me.GetContentDmRecordProperty(ConfigurationManager.AppSettings.Item("CatalogIDXPath"))))
    xslp.AddParam("related-source-file", "", catFile)
    xslp.AddParam("contentdm-collection", "", ConfigurationManager.AppSettings.Item("ContentDMCollectionName"))
    xslp.AddParam("contentdm-number", "", num)
    xslp.AddParam("source-file", "", recFile)

    If compoundObject IsNot Nothing Then
      xslp.AddParam("compound-object", "", compoundObject.Xml)
      If ConfigurationManager.AppSettings.Item("CreateSingleMODSFile").ToUpper = "FALSE" Then
        xslp.AddParam("link-compound-object", "", "TRUE")
      Else
        xslp.AddParam("link-compound-object", "", "FALSE")
      End If
    End If

    Dim xwr As XmlWriter = XmlWriter.Create(Path.Combine(recPath, saveFile), New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .ConformanceLevel = ConformanceLevel.Auto})

    plugin.TransFormToMods(rec, xslp, xwr)

    xwr.Close()

    Dim pChar As PremisObjectCharacteristics = PremisObjectCharacteristics.CharacterizeFile(Path.Combine(recPath, saveFile), "text/xml")

    'CAUTION: Some ugly filenaming code below

    'create premis object for mods record
    Dim pObj As New PremisObject("FILENAME", saveFile, PremisObjectCategory.File, pChar)
    pObj.PreservationLevels.Add(New PremisPreservationLevel("DERIVATIVE_METADATA_FILE", Now))
    If String.IsNullOrWhiteSpace(modsFile) Then
      'use the next available local identifier
      pObj.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    Else
      'base the local identifier off of the filename which was pre-assigned
      pObj.ObjectIdentifiers.Insert(0, New PremisIdentifier("LOCAL", pContainer.MakeID(pObj.GetDefaultFileNameIndex)))
    End If
    pMODS = pObj
    pContainer.Objects.Add(pObj)

    Dim newFName As String
    If String.IsNullOrWhiteSpace(modsFile) Then
      'rename the mods file to use the uuid
      newFName = pObj.GetDefaultFileName("mods_", ".xml")
      Rename(Path.Combine(recPath, saveFile), Path.Combine(recPath, newFName))
      pObj.GetFilenameIdentifier.IdentifierValue = newFName
    Else
      newFName = modsFile
    End If

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "MIGRATION")
    pMODSEvt = pEvt
    pEvt.EventDetail = String.Format("The {0} file was transformed into the {1} file using XSLT.", recFile, newFName)
    pEvt.LinkToAgent(pCurrentAgent)
    pContainer.Events.Add(pEvt)


    Dim pobj2 As PremisObject = pContainer.FindSingleObject("FILENAME", recFile)

    If num = recNum Then
      pRepresentation.RelateToObject("METADATA", "HAS_ROOT", pObj)
    End If

    pObj.RelateToObject("DERIVATION", "HAS_SOURCE", pobj2, pEvt)

    Return newFName

  End Function

  Private Function CaptureArchivalMaster(record As XmlElement, parent As PremisObject, relat As String, subRelat As String) As List(Of String)
    Dim arcs As List(Of ImageInfo) = plugin.GetArchivalMasterInfo(record)

    Dim arcFiles As New List(Of String)
    Dim i As Integer = 0
    For Each nd As ImageInfo In arcs
      Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
      pEvt.LinkToAgent(pCurrentAgent)
      If nd.Uri.EndsWith(".cpd") Then
        pEvt.EventDetail = String.Format("Fetching Compound Object File from URL: {0}", nd.Uri)
      Else
        pEvt.EventDetail = String.Format("Fetching Production Master Image from URL: {0}", nd.Uri)
      End If

      pContainer.Events.Add(pEvt)

      Dim pObjChars As New PremisObjectCharacteristics(nd.Format)

      Dim arcFile As String = FetchURL(nd.Uri, pEvt, pObjChars)
      arcFiles.Add(arcFile)

      If (Not okFormats.Contains(nd.Format)) And nd.Format.StartsWith("image/") Then
        'add event detail for less than ideal image formats
        Dim pEvtDet As New PremisEventOutcomeDetail(String.Format("The captured image is not in a preferred archival format.  Preferred formats are {0}.", Join(okFormats, " or ")))
        pEvt.EventOutcomeInformation.Item(0).EventOutcomeDetails.Add(pEvtDet)

      End If

      If Not String.IsNullOrWhiteSpace(arcFile) Then

        If nd.Uri.EndsWith(".cpd") Then

          Dim pObj2 As New PremisObject("FILENAME", arcFile, PremisObjectCategory.File, pObjChars)
          pObj2.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_METADATA_FILE", Now))
          pObj2.ObjectIdentifiers.Add(New PremisIdentifier("URL", nd.Uri))
          pObj2.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
          pObj2.OriginalName = arcFile

          'rename the file to use the uuid
          Dim newFName As String = pObj2.GetDefaultFileName(nd.BaseFormat & "_", Path.GetExtension(arcFile))
          Rename(Path.Combine(recPath, arcFile), Path.Combine(recPath, newFName))
          pObj2.GetFilenameIdentifier.IdentifierValue = newFName

          pObj2.LinkToEvent(pEvt)
          pContainer.Objects.Add(pObj2)


          pMODS.RelateToObject("DERIVATION", "HAS_SOURCE", pObj2, pMODSEvt)
          pMODSEvt.EventDetail = String.Format("The {0} file was derived from all of the files linked to this event by using XSLT.", pMODS.GetFilenameIdentifier.IdentifierValue)

          Dim lst As List(Of String) = ProcessCpdPages(compoundObject.Pages, parent)
          arcFiles.AddRange(lst)

          Dim lst2 As List(Of String) = ProcessCpdNodes(compoundObject.Nodes, parent)
          arcFiles.AddRange(lst2)

        Else
          Dim pObj As New PremisObject("FILENAME", arcFile, PremisObjectCategory.File, pObjChars)
          pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_CONTENT_FILE", Now))
          pObj.OriginalName = arcFile
          pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", nd.Uri))
          pObj.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)

          'rename the file to use the uuid
          Dim newFName As String = pObj.GetDefaultFileName(nd.BaseFormat & "_", Path.GetExtension(arcFile))
          Rename(Path.Combine(recPath, arcFile), Path.Combine(recPath, newFName))
          pObj.GetFilenameIdentifier.IdentifierValue = newFName

          pObj.LinkToEvent(pEvt)
          pContainer.Objects.Add(pObj)
          parent.RelateToObject(relat, subRelat, pObj)
          pObj.RelateToObject(relat, "PARENT", parent)
        End If

      End If
      i = i + 1
    Next

    Return arcFiles
  End Function

  Private Function CaptureArchivalMaster() As List(Of String)

    Dim arcFiles As List(Of String) = CaptureArchivalMaster(contentDmRecord, pRepresentation, "BASIC_IMAGE_ASSET", "PRODUCTION_MASTER")

    Return arcFiles
  End Function

  Private Function ProcessCpdPages(pgs As List(Of CpdPage), parent As PremisObject) As List(Of String)
    Dim ret As New List(Of String)
    Dim prevObj As PremisObject = Nothing
    For Each pg As CpdPage In pgs
      Dim nd As XmlElement = contentDmRecord.SelectSingleNode(String.Format("//record[contentdm_number={0}]", pg.PagePtr))

      Dim recFile As String = SaveContentDmRecord(pg.PagePtr, nd)

      'TODO: Use the LocalIdentifier in the CpdPage object for the identifier of the MODS Premis file
      Dim modsFile As String = ""
      If ConfigurationManager.AppSettings.Item("CreateSingleMODSFile").ToUpper = "FALSE" Then
        modsFile = MigrateRecordToMods("", recFile, PremisObject.GetFileName(New PremisIdentifier(pg.LocalIdentifier), "file", "mods_", "xml"), pg.PagePtr, nd)
      End If

      Dim pFile As PremisObject = pContainer.FindSingleObject("FILENAME", recFile)

      Dim pObj As New PremisObject("LOCAL", pContainer.NextID, PremisObjectCategory.Representation)
      pObj.XmlId = String.Format("pageptr_{0}", pg.PagePtr)
      pObj.RelateToObject("BASIC_COMPOUND_ASSET", "PARENT", parent)
      Dim pId As New PremisIdentifier("CONTENTDM_NUMBER", pg.PagePtr)
      pObj.ObjectIdentifiers.Add(pId)

      If ConfigurationManager.AppSettings.Item("CreateSingleMODSFile") = "TRUE" Then
        pMODS.RelateToObject("DERIVATION", "HAS_SOURCE", pFile, pMODSEvt)
      Else
        Dim pMODS2 As PremisObject = pContainer.FindSingleObject("FILENAME", modsFile)
        pObj.RelateToObject("METADATA", "HAS_ROOT", pMODS2)
      End If

      If prevObj Is Nothing Then
        parent.RelateToObject("BASIC_COMPOUND_ASSET", "FIRST_CHILD", pObj)
        pContainer.Objects.Add(pObj)
      Else
        prevObj.RelateToObject("BASIC_COMPOUND_ASSET", "NEXT_SIBLING", pObj)
        pObj.RelateToObject("BASIC_COMPOUND_ASSET", "PREVIOUS_SIBLING", prevObj)
        pContainer.Objects.Add(pObj)
      End If

      ret = CaptureArchivalMaster(nd, pObj, "BASIC_IMAGE_ASSET", "PRODUCTION_MASTER")

      prevObj = pObj
    Next

    Return ret
  End Function

  Private Function ProcessCpdNodes(nds As List(Of CpdNode), parent As PremisObject) As List(Of String)
    Dim ret As New List(Of String)
    Dim prevObj As PremisObject = Nothing

    For Each nd As CpdNode In nds
      Dim pObj As New PremisObject("LOCAL", pContainer.NextID, PremisObjectCategory.Representation)
      pObj.RelateToObject("BASIC_COMPOUND_ASSET", "PARENT", parent)

      If prevObj Is Nothing Then
        parent.RelateToObject("BASIC_COMPOUND_ASSET", "FIRST_CHILD", pObj)
      Else
        prevObj.RelateToObject("BASIC_COMPOUND_ASSET", "NEXT_SIBLING", pObj)
        pObj.RelateToObject("BASIC_COMPOUND_ASSET", "PREVIOUS_SIBLING", prevObj)
      End If

      pContainer.Objects.Add(pObj)

      Dim pLst As List(Of String) = ProcessCpdPages(nd.Pages, pObj)
      ret.AddRange(pLst)

      Dim nLst As List(Of String) = ProcessCpdNodes(nd.Nodes, pObj)
      ret.AddRange(nLst)

      prevObj = pObj

    Next


    Return ret
  End Function

  Private Function CaptureAccessImage(ByVal masterFile As String) As List(Of String)
    Dim rootPath As String = Path.GetPathRoot(ConfigurationManager.AppSettings.Item("ContentDMDataDir"))
    Dim retFiles As New List(Of String)

    Dim nd As XmlElement = contentDmRecord.SelectSingleNode("contentdm_file_path")

    Dim imgPath As String = Path.Combine(rootPath, nd.InnerText.Trim("/"))

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt.LinkToAgent(pCurrentAgent)
    pEvt.EventDetail = String.Format("Copying ContentDM Access image from '{0}'", imgPath)
    pContainer.Events.Add(pEvt)

    If File.Exists(Path.Combine(recPath, Path.GetFileName(imgPath))) Then
      File.Delete(Path.Combine(recPath, Path.GetFileName(imgPath)))
    End If

    Try
      File.Copy(imgPath, Path.Combine(recPath, Path.GetFileName(imgPath)))
    Catch ex As Exception
      Trace.TraceError("Error in Folder: {0}", recPath)
      Trace.TraceError("Copying file: {1} -- {0}", ex.Message, imgPath)

      Dim evtInfo As New PremisEventOutcomeInformation("ERROR")
      Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
      evtInfo.EventOutcomeDetails.Add(evtDet)
      pEvt.EventOutcomeInformation.Add(evtInfo)
      Return retFiles
      Exit Function
    End Try

    retFiles.Add(Path.GetFileName(imgPath))

    Dim pObjChars As PremisObjectCharacteristics = PremisObjectCharacteristics.CharacterizeFile(Path.Combine(recPath, Path.GetFileName(imgPath)), "image/jpeg")

    Dim pObj As New PremisObject("FILENAME", Path.GetFileName(imgPath), PremisObjectCategory.File, pObjChars)
    pObj.PreservationLevels.Add(New PremisPreservationLevel("DERIVATIVE_CONTENT_FILE", Now))
    pObj.OriginalName = imgPath
    pObj.LinkToEvent(pEvt)
    pContainer.Objects.Add(pObj)

    'pContainer.FindSingleObject("FILENAME", masterFile).RelateToObject("DERIVATION", "IS_SOURCE_OF", pObj)

    pRepresentation.RelateToObject("BASIC_IMAGE_ASSET", "SCREEN_SIZE", pObj)

    Return retFiles
  End Function

  Private Function CaptureThumbImage(ByVal masterFile As String) As List(Of String)
    Dim rootPath As String = Path.GetPathRoot(ConfigurationManager.AppSettings.Item("ContentDMDataDir"))
    Dim retFiles As New List(Of String)

    Dim nd As XmlElement = contentDmRecord.SelectSingleNode("contentdm_file_path")

    Dim imgPath As String = Path.Combine(rootPath, nd.InnerText.Trim("/"))
    imgPath = Path.Combine(Path.GetDirectoryName(imgPath), "icon" & Path.GetFileName(imgPath))

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt.LinkToAgent(pCurrentAgent)
    pEvt.EventDetail = String.Format("Copying ContentDM Thumbnail image from '{0}'", imgPath)
    pContainer.Events.Add(pEvt)

    If File.Exists(Path.Combine(recPath, Path.GetFileName(imgPath))) Then
      File.Delete(Path.Combine(recPath, Path.GetFileName(imgPath)))
    End If

    Try
      File.Copy(imgPath, Path.Combine(recPath, Path.GetFileName(imgPath)))
    Catch ex As Exception
      Trace.TraceError("Error in Folder: {0}", recPath)
      Trace.TraceError("Copying file: {1} -- {0}", ex.Message, imgPath)

      Dim evtInfo As New PremisEventOutcomeInformation("ERROR")
      Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
      evtInfo.EventOutcomeDetails.Add(evtDet)
      pEvt.EventOutcomeInformation.Add(evtInfo)
      Return retFiles
      Exit Function
    End Try

    retFiles.Add(Path.GetFileName(imgPath))

    Dim pObjChars As PremisObjectCharacteristics = PremisObjectCharacteristics.CharacterizeFile(Path.Combine(recPath, Path.GetFileName(imgPath)), "image/jpeg")


    Dim pObj As New PremisObject("FILENAME", Path.GetFileName(imgPath), PremisObjectCategory.File, pObjChars)
    pObj.PreservationLevels.Add(New PremisPreservationLevel("DERIVATIVE_CONTENT_FILE", Now))
    pObj.OriginalName = imgPath
    pObj.LinkToEvent(pEvt)
    pContainer.Objects.Add(pObj)


    'pContainer.FindSingleObject("FILENAME", masterFile).RelateToObject("DERIVATION", "IS_SOURCE_OF", pObj)

    pRepresentation.RelateToObject("BASIC_IMAGE_ASSET", "THUMBNAIL", pObj)

    Return retFiles
  End Function

  Private Function FetchURL(ByVal url As String, ByVal pEvt As PremisEvent, ByVal pObjChar As PremisObjectCharacteristics, Optional ByVal saveFile As String = "") As String
    Dim uri As New Uri(url)
    Dim size As Long = 0

    Dim httpReq As WebRequest = WebRequest.Create(uri)
    Dim httpRsp As WebResponse = Nothing

    Dim tries As Integer = 0
    Do Until tries > MAX_RETRY_COUNT
      Try
        httpRsp = httpReq.GetResponse
        Exit Do
      Catch ex As WebException
        If tries >= MAX_RETRY_COUNT Then
          Trace.TraceError("Error in Folder: {0}", recPath)
          Trace.TraceError("Fetching URL: {1} -- {0}", ex.Message, url)

          Dim evtInfo As New PremisEventOutcomeInformation([Enum].GetName(GetType(WebExceptionStatus), ex.Status))
          Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
          evtInfo.EventOutcomeDetails.Add(evtDet)
          pEvt.EventOutcomeInformation.Add(evtInfo)
          Return ""
          Exit Function
        Else
          Trace.TraceInformation(String.Format("{2} Retrying FetchURL: {0}.  Try: {1}", uri, tries, recPath))
        End If
      Catch ex As Exception
        Trace.TraceError("Error in Folder: {0}", recPath)
        Trace.TraceError("Fetching URL: {1} -- {0}", ex.Message, url)

        Dim evtInfo As New PremisEventOutcomeInformation(ex.GetType.Name)
        Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
        evtInfo.EventOutcomeDetails.Add(evtDet)
        pEvt.EventOutcomeInformation.Add(evtInfo)
        Return ""
        Exit Function
      End Try
      Threading.Thread.Sleep(((3 ^ tries) - 1) * 1000)
      tries = tries + 1
      httpReq = WebRequest.Create(uri)
    Loop

    Dim http_mime As String = httpRsp.ContentType

    Dim outFileName As String
    If String.IsNullOrWhiteSpace(saveFile) Then
      Dim fn As String = Path.GetFileName(uri.LocalPath)
      'replace invalid chars in name
      fn = Regex.Replace(fn, String.Format("[{0}]", New String(Path.GetInvalidFileNameChars)), "_") 'TODO: This could result in filename collisions
      outFileName = Path.Combine(recPath, fn)
    Else
      outFileName = saveFile
    End If

    Dim alg As String = ConfigurationManager.AppSettings.Item("ChecksumAlgorithm")
    Dim strm As Stream = httpRsp.GetResponseStream
    Dim sha1 As HashAlgorithm = HashAlgorithm.Create(alg)

    Dim outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)

    Dim byt(8 * 1024) As Byte
    Dim len As Integer = strm.Read(byt, 0, byt.Length)
    Dim len2 As Integer = 0
    While len > 0
      size = size + len
      outStrm.Write(byt, 0, len)
      len2 = sha1.TransformBlock(byt, 0, len, byt, 0)
      len = strm.Read(byt, 0, byt.Length)
    End While
    sha1.TransformFinalBlock(byt, 0, 0)

    Dim pFix As New PremisFixity(alg, BytesToStr(sha1.Hash))
    pObjChar.Fixities.Add(pFix)
    pObjChar.Size = size


    outStrm.Close()
    strm.Close()

    If TypeOf httpRsp Is HttpWebResponse Then
      Dim evtInfoOK As New PremisEventOutcomeInformation([Enum].GetName(GetType(HttpStatusCode), CType(httpRsp, HttpWebResponse).StatusCode))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    Else
      Dim evtInfoOK As New PremisEventOutcomeInformation(If(httpRsp.ContentLength > 0, "OK", "InternalServerError"))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    End If

    Dim mime As String = MetadataFunctions.GetMimeFromFile(outFileName, http_mime)

    If mime <> pObjChar.Formats.Item(0).FormatName Then
      Dim pForm2 As New PremisFormat(mime)
      pForm2.FormatNotes.Add("This is the MIME type as determined by URL Moniker Library.")
      pObjChar.Formats.Add(pForm2)

      Dim pEvtDet As New PremisEventOutcomeDetail("The MIME type returned by the HTTP Request or as determined by the URL Moniker Library does not match the expected MIME type.")
      pEvt.EventOutcomeInformation.Item(0).EventOutcomeDetails.Add(pEvtDet)

    End If

    Return Path.GetFileName(outFileName)

  End Function

  Private Function GetContentDmRecordProperties(ByVal propName As String) As List(Of String)
    Dim valNodes As XmlNodeList = contentDmRecord.SelectNodes(propName)
    Dim ret As New List(Of String)
    For Each valNode In valNodes
      ret.Add(valNode.innertext)
    Next
    Return ret
  End Function

  Private Function GetContentDmRecordProperty(ByVal propName As String) As String
    Dim valNode As XmlNode = contentDmRecord.SelectSingleNode(propName)
    If valNode IsNot Nothing Then
      Return valNode.InnerText
    Else
      Return ""
    End If
  End Function

  'TODO: Move this to a common library
  Private Shared Function BytesToStr(ByVal bytes() As Byte) As String
    Dim str As StringBuilder = New StringBuilder
    Dim i As Integer = 0
    Do While (i < bytes.Length)
      str.AppendFormat("{0:X2}", bytes(i))
      i = (i + 1)
    Loop
    Return str.ToString
  End Function


End Class
