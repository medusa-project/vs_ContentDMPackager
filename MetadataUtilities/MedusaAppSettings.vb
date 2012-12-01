Imports System.Configuration
Imports System.IO
Imports System.Collections.Specialized

Public Class MedusaAppSettings

  Private Shared _thisInst As MedusaAppSettings

  Protected Sub New()
    'this is a singleton
  End Sub

  Public Shared Function Settings() As MedusaAppSettings
    If _thisInst Is Nothing Then
      _thisInst = New MedusaAppSettings
    End If
    Return _thisInst
  End Function

  Private _idMapFile As String = Nothing
  Public Property IdMapFile As String
    Get
      If _idMapFile Is Nothing Then
        _idMapFile = ConfigurationManager.AppSettings.Item("IdMapFile")
      End If
      Return _idMapFile
    End Get
    Set(value As String)
      _idMapFile = value
    End Set
  End Property

  Public ReadOnly Property IdMapFilePath As String
    Get
      Return Path.Combine(WorkingFolder, IdMapFile)
    End Get
  End Property

  Private _handleMapFile As String = Nothing
  Public Property HandleMapFile As String
    Get
      If _handleMapFile Is Nothing Then
        _handleMapFile = ConfigurationManager.AppSettings.Item("HandleMapFile")
      End If
      Return _handleMapFile
    End Get
    Set(value As String)
      _handleMapFile = value
    End Set
  End Property

  Public ReadOnly Property HandleMapFilePath As String
    Get
      Return Path.Combine(WorkingFolder, HandleMapFile)
    End Get
  End Property

  Private _handlePrefix As String = Nothing
  Public Property HandlePrefix As String
    Get
      If _handlePrefix Is Nothing Then
        _handlePrefix = ConfigurationManager.AppSettings.Item("Handle.Prefix")
      End If
      Return _handlePrefix
    End Get
    Set(value As String)
      _handlePrefix = value
    End Set
  End Property

  Private _handleProject As String = Nothing
  Public Property HandleProject As String
    Get
      If _handleProject Is Nothing Then
        _handleProject = ConfigurationManager.AppSettings.Item("Handle.Project")
      End If
      Return _handleProject
    End Get
    Set(value As String)
      _handleProject = value
    End Set
  End Property

  Private _handleLocalIdSeparator As String = Nothing
  Public Property HandleLocalIdSeparator As String
    Get
      If _handleLocalIdSeparator Is Nothing Then
        _handleLocalIdSeparator = ConfigurationManager.AppSettings.Item("Handle.LocalIdSeparator")
      End If
      Return _handleLocalIdSeparator
    End Get
    Set(value As String)
      _handleLocalIdSeparator = value
    End Set
  End Property

  Private _handleServiceURL As String = Nothing
  Public Property HandleServiceURL As String
    Get
      If _handleServiceURL Is Nothing Then
        _handleServiceURL = ConfigurationManager.AppSettings.Item("Handle.ServiceURL")
      End If
      Return _handleServiceURL
    End Get
    Set(value As String)
      _handleServiceURL = value
    End Set
  End Property

  Private _handleResourceType As String = Nothing
  Public Property HandleResourceType As String
    Get
      If _handleResourceType Is Nothing Then
        _handleResourceType = ConfigurationManager.AppSettings.Item("Handle.ResourceType")
      End If
      Return _handleResourceType
    End Get
    Set(value As String)
      _handleResourceType = value
    End Set
  End Property

  Private _handleResolverBaseURL As String = Nothing
  Public Property HandleResolverBaseURL As String
    Get
      If _handleResolverBaseURL Is Nothing Then
        _handleResolverBaseURL = ConfigurationManager.AppSettings.Item("Handle.ResolverBaseURL")
      End If
      Return _handleResolverBaseURL
    End Get
    Set(value As String)
      _handleResolverBaseURL = value
    End Set
  End Property

  Private _handleGeneration As HandleGenerationType? = Nothing
  Public Property HandleGeneration As HandleGenerationType
    Get
      If _handleGeneration Is Nothing Then
        _handleGeneration = [Enum].Parse(GetType(HandleGenerationType), ConfigurationManager.AppSettings.Item("Handle.Generation"), True)
      End If
      Return _handleGeneration
    End Get
    Set(value As HandleGenerationType)
      _handleGeneration = value
    End Set
  End Property

  Private _getCollectionModsUrl As String = Nothing
  Public Property GetCollectionModsUrl As String
    Get
      If _getCollectionModsUrl Is Nothing Then
        _getCollectionModsUrl = ConfigurationManager.AppSettings.Item("GetCollectionModsUrl")
      End If
      Return _getCollectionModsUrl
    End Get
    Set(value As String)
      _getCollectionModsUrl = value
    End Set
  End Property

  Private _getMarcUrl As String = Nothing
  Public Property GetMarcUrl As String
    Get
      If _getMarcUrl Is Nothing Then
        _getMarcUrl = ConfigurationManager.AppSettings.Item("GetMarcUrl")
      End If
      Return _getMarcUrl
    End Get
    Set(value As String)
      _getMarcUrl = value
    End Set
  End Property

  Private _checksumAlgorithm As String = Nothing
  Public Property ChecksumAlgorithm As String
    Get
      If _checksumAlgorithm Is Nothing Then
        _checksumAlgorithm = ConfigurationManager.AppSettings.Item("ChecksumAlgorithm")
      End If
      Return _checksumAlgorithm
    End Get
    Set(value As String)
      _checksumAlgorithm = value
    End Set
  End Property

  Private _ignoreBadCert As Boolean? = Nothing
  Public Property IgnoreBadCert As Boolean
    Get
      If _ignoreBadCert Is Nothing Then
        If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("IgnoreBadCert")) Then
          _ignoreBadCert = False
        Else
          _ignoreBadCert = Boolean.Parse(ConfigurationManager.AppSettings.Item("IgnoreBadCert"))
        End If
      End If
      Return _ignoreBadCert
    End Get
    Set(value As Boolean)
      _ignoreBadCert = value
    End Set
  End Property

  Private _saveFilesAs As SaveFileAsType? = Nothing
  Public Property SaveFilesAs As SaveFileAsType
    Get
      If _saveFilesAs Is Nothing Then
        _saveFilesAs = [Enum].Parse(GetType(SaveFileAsType), ConfigurationManager.AppSettings.Item("SaveFilesAs"), True)
      End If
      Return _saveFilesAs
    End Get
    Set(value As SaveFileAsType)
      _saveFilesAs = value
    End Set
  End Property

  Private _doFits As Boolean? = Nothing
  Public Property DoFits As Boolean
    Get
      If _doFits Is Nothing Then
        If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("DoFits")) Then
          _doFits = True
        Else
          _doFits = Boolean.Parse(ConfigurationManager.AppSettings.Item("DoFits"))
        End If
      End If
      Return _doFits
    End Get
    Set(value As Boolean)
      _doFits = value
    End Set
  End Property

  Private _fitsHome As String = Nothing
  Public Property FitsHome As String
    Get
      If _fitsHome Is Nothing Then
        _fitsHome = ConfigurationManager.AppSettings.Item("FitsHome")
      End If
      Return _fitsHome
    End Get
    Set(value As String)
      _fitsHome = value
    End Set
  End Property

  Private _fitsScript As String = Nothing
  Public Property FitsScript As String
    Get
      If _fitsScript Is Nothing Then
        _fitsScript = ConfigurationManager.AppSettings.Item("FitsScript")
      End If
      Return _fitsScript
    End Get
    Set(value As String)
      _fitsScript = value
    End Set
  End Property

  Private _overwriteObjects As Boolean? = Nothing
  Public Property OverwriteObjects As Boolean
    Get
      If _overwriteObjects Is Nothing Then
        If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("OverwriteObjects")) Then
          _overwriteObjects = False
        End If
        _overwriteObjects = Boolean.Parse(ConfigurationManager.AppSettings.Item("OverwriteObjects"))
      End If
      Return _overwriteObjects
    End Get
    Set(value As Boolean)
      _overwriteObjects = value
    End Set
  End Property

  Private _workingFolder As String = Nothing
  Public Property WorkingFolder As String
    Get
      If _workingFolder Is Nothing Then
        _workingFolder = ConfigurationManager.AppSettings.Item("WorkingFolder")
      End If
      Return _workingFolder
    End Get
    Set(value As String)
      _workingFolder = value
    End Set
  End Property

  Private _logFile As String = Nothing
  Public Property LogFile As String
    Get
      If _logFile Is Nothing Then
        _logFile = ConfigurationManager.AppSettings.Item("LogFile")
        If String.IsNullOrWhiteSpace(_logFile) Then
          _logFile = "FolderPackager.log"
        End If
      End If
      Return _logFile
    End Get
    Set(value As String)
      _logFile = value
    End Set
  End Property

  Public ReadOnly Property LogFilePath As String
    Get
      Return Path.Combine(WorkingFolder, LogFile)
    End Get
  End Property


  Private _sourceFolder As String = Nothing
  Public Property SourceFolder As String
    Get
      If _sourceFolder Is Nothing Then
        _sourceFolder = ConfigurationManager.AppSettings.Item("SourceFolder")
      End If
      Return _sourceFolder
    End Get
    Set(value As String)
      _sourceFolder = value
    End Set
  End Property

  Private _collectionId As String = Nothing
  Public Property CollectionId As String
    Get
      If _collectionId Is Nothing Then
        _collectionId = ConfigurationManager.AppSettings.Item("CollectionId")
      End If
      Return _collectionId
    End Get
    Set(value As String)
      _collectionId = value
    End Set
  End Property

  Private _collectionHandle As String = Nothing
  Public Property CollectionHandle As String
    Get
      If _collectionHandle Is Nothing Then
        _collectionHandle = ConfigurationManager.AppSettings.Item("CollectionHandle")
      End If
      Return _collectionHandle
    End Get
    Set(value As String)
      _collectionHandle = value
    End Set
  End Property

  Private _collectionName As String = Nothing
  Public Property CollectionName As String
    Get
      If _collectionName Is Nothing Then
        _collectionName = ConfigurationManager.AppSettings.Item("CollectionName")
      End If
      Return _collectionName
    End Get
    Set(value As String)
      _collectionName = value
    End Set
  End Property

  Private _collectionURL As String = Nothing
  Public Property CollectionURL As String
    Get
      If _collectionURL Is Nothing Then
        _collectionURL = ConfigurationManager.AppSettings.Item("CollectionURL")
      End If
      Return _collectionURL
    End Get
    Set(value As String)
      _collectionURL = value
    End Set
  End Property

  Private _collectionDescriptionPath As String = Nothing
  Public Property CollectionDescriptionPath As String
    Get
      If _collectionDescriptionPath Is Nothing Then
        _collectionDescriptionPath = ConfigurationManager.AppSettings.Item("CollectionDescriptionPath")
      End If
      Return _collectionDescriptionPath
    End Get
    Set(value As String)
      _collectionDescriptionPath = value
    End Set
  End Property

  Private _objectFolderLevel As Integer? = Nothing
  Public Property ObjectFolderLevel As Integer
    Get
      If _objectFolderLevel Is Nothing Then
        _objectFolderLevel = Integer.Parse(ConfigurationManager.AppSettings.Item("ObjectFolderLevel"))
      End If
      Return _objectFolderLevel
    End Get
    Set(value As Integer)
      _objectFolderLevel = value
    End Set
  End Property

  Private _premisDisseminationRightsBasis As String = Nothing
  Public Property PremisDisseminationRightsBasis As String
    Get
      If _premisDisseminationRightsBasis Is Nothing Then
        _premisDisseminationRightsBasis = ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis")
      End If
      Return _premisDisseminationRightsBasis
    End Get
    Set(value As String)
      _premisDisseminationRightsBasis = value
    End Set
  End Property

  Private _premisDisseminationCopyrightStatus As String = Nothing
  Public Property PremisDisseminationCopyrightStatus As String
    Get
      If _premisDisseminationCopyrightStatus Is Nothing Then
        _premisDisseminationCopyrightStatus = ConfigurationManager.AppSettings.Item("PremisDisseminationCopyrightStatus")
      End If
      Return _premisDisseminationCopyrightStatus
    End Get
    Set(value As String)
      _premisDisseminationCopyrightStatus = value
    End Set
  End Property

  Private _premisDisseminationRights As String = Nothing
  Public Property PremisDisseminationRights As String
    Get
      If _premisDisseminationRights Is Nothing Then
        _premisDisseminationRights = ConfigurationManager.AppSettings.Item("PremisDisseminationRights")
      End If
      Return _premisDisseminationRights
    End Get
    Set(value As String)
      _premisDisseminationRights = value
    End Set
  End Property

  Private _premisDisseminationRightsRestrictions As String = Nothing
  Public Property PremisDisseminationRightsRestrictions As String
    Get
      If _premisDisseminationRightsRestrictions Is Nothing Then
        _premisDisseminationRightsRestrictions = ConfigurationManager.AppSettings.Item("PremisDisseminationRightsRestrictions")
      End If
      Return _premisDisseminationRightsRestrictions
    End Get
    Set(value As String)
      _premisDisseminationRightsRestrictions = value
    End Set
  End Property

  Private _metadataMarcRegex As String = Nothing
  Public Property MetadataMarcRegex As String
    Get
      If _metadataMarcRegex Is Nothing Then
        _metadataMarcRegex = ConfigurationManager.AppSettings.Item("MetadataMarcRegex")
      End If
      Return _metadataMarcRegex
    End Get
    Set(value As String)
      _metadataMarcRegex = value
    End Set
  End Property

  Private _metadataDcRdfRegex As String = Nothing
  Public Property MetadataDcRdfRegex As String
    Get
      If _metadataDcRdfRegex Is Nothing Then
        _metadataDcRdfRegex = ConfigurationManager.AppSettings.Item("MetadataDcRdfRegex")
      End If
      Return _metadataDcRdfRegex
    End Get
    Set(value As String)
      _metadataDcRdfRegex = value
    End Set
  End Property

  Private _metadataSpreadsheetRegex As String = Nothing
  Public Property MetadataSpreadsheetRegex As String
    Get
      If _metadataSpreadsheetRegex Is Nothing Then
        _metadataSpreadsheetRegex = ConfigurationManager.AppSettings.Item("MetadataSpreadsheetRegex")
      End If
      Return _metadataSpreadsheetRegex
    End Get
    Set(value As String)
      _metadataSpreadsheetRegex = value
    End Set
  End Property

  Private _omitFoldersRegex As String = Nothing
  Public Property OmitFoldersRegex As String
    Get
      If _omitFoldersRegex Is Nothing Then
        _omitFoldersRegex = ConfigurationManager.AppSettings.Item("OmitFoldersRegex")
      End If
      Return _omitFoldersRegex
    End Get
    Set(value As String)
      _omitFoldersRegex = value
    End Set
  End Property

  Private _omitFilesRegex As String = Nothing
  Public Property OmitFilesRegex As String
    Get
      If _omitFilesRegex Is Nothing Then
        _omitFilesRegex = ConfigurationManager.AppSettings.Item("OmitFilesRegex")
      End If
      Return _omitFilesRegex
    End Get
    Set(value As String)
      _omitFilesRegex = value
    End Set
  End Property

  Private _derivativeContentFileRegex As String = Nothing
  Public Property DerivativeContentFileRegex As String
    Get
      If _derivativeContentFileRegex Is Nothing Then
        _derivativeContentFileRegex = ConfigurationManager.AppSettings.Item("DerivativeContentFileRegex")
      End If
      Return _derivativeContentFileRegex
    End Get
    Set(value As String)
      _derivativeContentFileRegex = value
    End Set
  End Property

  Private _originalContentFileRegex As String = Nothing
  Public Property OriginalContentFileRegex As String
    Get
      If _originalContentFileRegex Is Nothing Then
        _originalContentFileRegex = ConfigurationManager.AppSettings.Item("OriginalContentFileRegex")
      End If
      Return _originalContentFileRegex
    End Get
    Set(value As String)
      _originalContentFileRegex = value
    End Set
  End Property

  Private _significantFileIdentiferRegex As String = Nothing
  Public Property SignificantFileIdentiferRegex As String
    Get
      If _significantFileIdentiferRegex Is Nothing Then
        _significantFileIdentiferRegex = ConfigurationManager.AppSettings.Item("SignificantFileIdentiferRegex")
      End If
      Return _significantFileIdentiferRegex
    End Get
    Set(value As String)
      _significantFileIdentiferRegex = value
    End Set
  End Property

  Private _packageMode As PackageModeType? = Nothing
  Public Property PackageMode As PackageModeType
    Get
      If _packageMode Is Nothing Then
        _packageMode = [Enum].Parse(GetType(PackageModeType), ConfigurationManager.AppSettings.Item("PackageMode"), True)
      End If
      Return _packageMode
    End Get
    Set(value As PackageModeType)
      _packageMode = value
    End Set
  End Property


  Public ReadOnly Property FitsScriptPath As String
    Get
      Return Path.Combine(FitsHome, FitsScript)
    End Get
  End Property

  Private _handlePassword As String = Nothing
  Public Property HandlePassword As String
    Get
      If _handlePassword Is Nothing Then
        Dim confSet As NameValueCollection = ConfigurationManager.GetSection("secretAppSettings")
        _handlePassword = confSet.Item("Handle.Password")
      End If
      Return _handlePassword
    End Get
    Set(value As String)
      _handlePassword = value
    End Set
  End Property



  Private _marcToModsXslt As String = Nothing
  Public Property MarcToModsXslt As String
    Get
      If _marcToModsXslt Is Nothing Then
        _marcToModsXslt = ConfigurationManager.AppSettings.Item("MarcToModsXslt")
      End If
      Return _marcToModsXslt
    End Get
    Set(value As String)
      _marcToModsXslt = value
    End Set
  End Property

  Private _dcRdfToModsXslt As String = Nothing
  Public Property DcRdfToModsXslt As String
    Get
      If _dcRdfToModsXslt Is Nothing Then
        _dcRdfToModsXslt = ConfigurationManager.AppSettings.Item("DcRdfToModsXslt")
      End If
      Return _dcRdfToModsXslt
    End Get
    Set(value As String)
      _dcRdfToModsXslt = value
    End Set
  End Property

  Private _restartAtPath As String = Nothing
  Public Property RestartAtPath As String
    Get
      If _restartAtPath Is Nothing Then
        _restartAtPath = ConfigurationManager.AppSettings.Item("RestartAtPath")
      End If
      Return _restartAtPath
    End Get
    Set(value As String)
      _restartAtPath = value
    End Set
  End Property

  Public Const COPYRIGHT As String = "COPYRIGHT"
End Class

Public Enum HandleGenerationType
  ROOT_OBJECT_AND_FILES
  ROOT_OBJECT_ONLY
  FILES_ONLY
  NONE
End Enum

Public Enum SaveFileAsType
  ONE
  MULTIPLE
  REPRESENTATIONS
  MEDUSA
  MEDUSA_MULTIPLE
End Enum

Public Enum PackageModeType
  MOVE
  COPY
  HARDLINK
End Enum