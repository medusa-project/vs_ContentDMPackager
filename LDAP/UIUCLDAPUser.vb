Imports System.DirectoryServices
Imports System.Runtime.InteropServices
Imports Uiuc.Library.Premis
Imports System.Security

''' <summary>
''' This class uses the campus LDAP.  it provides access to LDAP attributes for a given user account keyed by netid.
''' </summary>
''' <remarks></remarks>
Public Class UIUCLDAPUser
  Private _netid As String

  Private _result As SearchResult = Nothing

  Private _errnum As Integer = 0
  Private _errdes As String = ""

  ''' <summary>
  ''' Parse a qualified id (domain\id) and return just the id part
  ''' </summary>
  ''' <param name="s"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetNetIDFromQualifiedID(ByVal s As String) As String
    If s.Contains("\") Then
      Return s.Substring(s.IndexOf("\") + 1)
    Else
      Return s
    End If
  End Function

  ''' <summary>
  ''' Parse a qualified id (domain\id) and return just the domain part
  ''' </summary>
  ''' <param name="s"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetDomainFromQualifiedID(ByVal s As String) As String
    If s.Contains("\") Then
      Return s.Substring(0, s.IndexOf("\"))
    Else
      Return ""
    End If
  End Function

  ''' <summary>
  ''' Return a PREMIS Agent object for the given NetID
  ''' </summary>
  ''' <param name="netid"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetPremisAgent(ByVal netid As String) As PremisAgent
    Dim ldap As New UIUCLDAPUser(netid)
    Dim agent As PremisAgent = New PremisAgent("UIUC_NETID", netid)
    agent.AgentIdentifiers.Add(New PremisIdentifier("EMAIL", ldap.EMail))
    agent.AgentNames.Add(ldap.DisplayName)
    agent.AgentType = "PERSON"
    agent.AgentNotes.Add(ldap.Title & ", " & ldap.HomeDepartment)

    Return agent
  End Function

  ''' <summary>
  ''' Return a PREMIS Agent object for the currently logged in user
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetPremisAgent() As PremisAgent
    Return UIUCLDAPUser.GetPremisAgent(Principal.WindowsIdentity.GetCurrent.Name)
  End Function


  ''' <summary>
  ''' I don't want instantiated classes without a specified netid
  ''' </summary>
  ''' <remarks></remarks>
  Protected Sub New()
    'do not want outsiders creating empty instances
  End Sub

  ''' <summary>
  ''' Initialize the class for a given netid
  ''' </summary>
  ''' <param name="netid"></param>
  ''' <remarks></remarks>
  Public Sub New(ByVal netid As String)

    _netid = UIUCLDAPUser.GetNetIDFromQualifiedID(netid)

    Dim entry As DirectoryEntry = New DirectoryEntry("LDAP://ldap-campus.uiuc.edu/ou=people,dc=uiuc,dc=edu")

    entry.AuthenticationType = AuthenticationTypes.None
    Dim search As DirectorySearcher = New DirectorySearcher(entry)

    search.Filter = String.Format("(uid={0})", _netid)
    search.SearchScope = SearchScope.Subtree
    Dim result As SearchResult
    Dim scoll As SearchResultCollection = Nothing
    Try
      scoll = search.FindAll
      If scoll.Count = 0 Then
        _result = Nothing
        _errnum = 404
        _errdes = "No matching record was found."
      End If
      If scoll.Count > 1 Then
        _errnum = 200
        _errdes = "Unexpected number of returns"
      End If
      For Each result In scoll
        _result = result
      Next
    Catch ex1 As DirectoryServicesCOMException
      _errnum = ex1.ExtendedError
      _errdes = ex1.ExtendedErrorMessage
    Catch ex2 As COMException
      _errnum = ex2.ErrorCode
      _errdes = ex2.Message
    Catch ex3 As SystemException
      _errnum = 100
      _errdes = ex3.Message
    End Try
    If Not scoll Is Nothing Then
      scoll.Dispose()
    End If

  End Sub

  'uiucEduUserEmailAddr
  ''' <summary>
  ''' Return the email address of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property EMail As String
    Get
      If _result.Properties.Contains("mail") Then
        Return _result.Properties.Item("mail").Item(0)
      Else
        Return ""
      End If
    End Get
  End Property


  ''' <summary>
  ''' Return the job title of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Title As String
    Get
      If _result.Properties.Contains("title") Then
        Return StrConv(_result.Properties.Item("title").Item(0), VbStrConv.ProperCase)
      Else
        Return ""
      End If
    End Get
  End Property

  ''' <summary>
  ''' Return the uid of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property UID() As String
    Get
      Return _result.Properties.Item("uid").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return the display name of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property DisplayName() As String
    Get
      Return _result.Properties.Item("displayname").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return just the first name of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property FirstName() As String
    Get
      Return _result.Properties.Item("uiucedufirstname").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return just the last name of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property LastName() As String
    Get
      Return _result.Properties.Item("uiucedulastname").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return the concat of the firstname lastname
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property FirstLastName() As String
    Get
      Return String.Format("{0} {1}", FirstName, LastName)
    End Get
  End Property

  ''' <summary>
  ''' Return the concat the lastname, firstname
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property LastFirstName() As String
    Get
      Return String.Format("{1}, {0}", FirstName, LastName)
    End Get
  End Property

  ''' <summary>
  ''' Return an array of all affiliations for the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Affiliations() As String()
    Get
      Dim ret() As String
      ReDim ret(_result.Properties.Item("eduPersonAffiliation").Count - 1)
      _result.Properties.Item("eduPersonAffiliation").CopyTo(ret, 0)
      Return ret
    End Get
  End Property

  ''' <summary>
  ''' Return the name of the home department for the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property HomeDepartment() As String
    Get
      Return _result.Properties.Item("uiucEduHomeDeptName").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return the office address of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property OfficeAddress() As String
    Get
      Return _result.Properties.Item("uiucEduOfficeAddress").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return the employee type code of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks>
  ''' NOTE:  The following are the meanings for the EmpTypes
  ''' "A" = faculty/academic (regular faculty)
  ''' "B" = academic professional (research faculty)
  ''' "C,D,E" = civil service, including hourly
  ''' "H" = academic/grad hourly
  ''' "P" = post doc, res assoc and intern 
  ''' "T" = designating working-retirees (emeritis faculty)
  ''' "U" = "unpaid" by the University (adjunct faculty)
  ''' </remarks>
  Public ReadOnly Property EmployeeType As String
    Get
      Return _result.Properties.Item("uiucEduEmployeeType").Item(0)
    End Get
  End Property

  ''' <summary>
  ''' Return the employee type name of the user
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property EmployeeTypeName As String
    Get
      Return UIUCLDAPUser.ConvertEmployeeTypesToNames(Me.EmployeeType)
    End Get
  End Property

  ''' <summary>
  ''' Convert a list of employee type codes into a list of employee type names
  ''' </summary>
  ''' <param name="typstr"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function ConvertEmployeeTypesToNames(ByVal typstr As String) As String
    Dim delims() As Char = {",", ";"}
    Dim typs() As String = typstr.Split(delims, StringSplitOptions.RemoveEmptyEntries)

    Dim ret As String = ""

    For Each code In typs
      Select Case code.ToUpper
        Case "A"
          ret = ret & ", Faculty"
        Case "B"
          ret = ret & ", Acad. Prof."
        Case "C", "D"
          ret = ret & ", Civil Service"
        Case "E"
          ret = ret & ", Extra Help"
        Case "G"
          ret = ret & ", Grad. Assisant"
        Case "H"
          ret = ret & ", Acad./Grad. Hourly"
        Case "L"
          ret = ret & ", Lump Sum"
        Case "M"
          ret = ret & ", Summer Help"
        Case "P"
          ret = ret & ", Post Doc."
        Case "R"
          ret = ret & ", Medical Resident"
        Case "S"
          ret = ret & ", Student"
        Case "T"
          ret = ret & ", Retiree"
        Case "U"
          ret = ret & ", Unpaid"
        Case "V"
          ret = ret & ", Virtual"
        Case "W"
          ret = ret & ", One Time Pay"
        Case Else
          ret = ret & String.Format(", Code: {0}", code)
      End Select
    Next

    Return Mid(ret, 3)
  End Function


  ''' <summary>
  ''' Return the ldap error number; 0 for no error
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property ErrorNumber() As Integer
    Get
      Return _errnum
    End Get
  End Property

  ''' <summary>
  ''' Return the LDAP error message; empty string for no error
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property ErrorMessage() As String
    Get
      Return _errdes
    End Get
  End Property

  ''' <summary>
  ''' Make sure the LDAP directory is closed
  ''' </summary>
  ''' <remarks></remarks>
  Protected Overrides Sub finalize()
    If _result IsNot Nothing Then
      _result.GetDirectoryEntry.Close()
    End If
  End Sub

  Public Shared Function Create(ByVal netid As String) As UIUCLDAPUser
    Return New UIUCLDAPUser(netid)
  End Function

End Class
