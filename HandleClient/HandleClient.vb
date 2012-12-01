Imports System.Net
Imports System.Xml
Imports System.ComponentModel
Imports System.IO
Imports System.Configuration
Imports System.Text
Imports System.Collections.Specialized
Imports System.Security
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities

Public Class HandleClient

  Public Property local_id As String

  Public Property target As String

  Public Property target_type As TargetType

  Public Property email As String

  Public Property desc As String

  Public Property status As HttpStatusCode = HttpStatusCode.InternalServerError

  Public Property error_message As String = "Unexpected Error"

  Public Property last_action As String = ""

  Public Property updated_fields As String = ""

  Public Property dotrace As Boolean = True

  Public Shared Function DeleteHandle(local_id As String) As HandleClient
    Dim hc As New HandleClient()
    hc.local_id = local_id
    Dim ret As HttpStatusCode = hc.Delete
    hc.updated_fields = "Deleted"
    Return hc
  End Function

  Public Shared Function CreateUpdateHandle(local_id As String, target As String, email As String, desc As String) As HandleClient
    Dim hc As New HandleClient()
    Dim ret As HttpStatusCode
    Dim update As Boolean = False

    Dim target_type As TargetType
    Dim u As New Uri(target)
    If u.Scheme.ToLower = "file" Then
      target_type = TargetType.FILE
    Else
      target_type = TargetType.URL
    End If

    hc.local_id = local_id

    'try to get the handle, if it doesn't exist create it, if it does exist update it - if needed
    hc.dotrace = False
    ret = hc.Retrieve()
    hc.dotrace = True
    If ret = HttpStatusCode.OK Then
      If hc.target_type <> target_type Then
        hc.updated_fields = String.Format("target_type: '{1}'=>'{2}', ", hc.updated_fields, hc.target_type, target_type)
        hc.target_type = target_type
        update = True
      End If
      If target IsNot Nothing AndAlso hc.target <> target Then
        hc.updated_fields = String.Format("{0}target: '{1}'=>'{2}', ", hc.updated_fields, hc.target, target)
        hc.target = target
        update = True
      End If
      If email IsNot Nothing AndAlso hc.email <> email Then
        hc.updated_fields = String.Format("{0}email: '{1}'=>'{2}', ", hc.updated_fields, hc.email, email)
        hc.email = email
        update = True
      End If
      If desc IsNot Nothing AndAlso hc.desc <> desc Then
        hc.updated_fields = String.Format("{0}desc: '{1}'=>'{2}', ", hc.updated_fields, hc.desc, desc)
        hc.desc = desc
        update = True
      End If
      If update = True Then
        'delete old record and add new one (hc.update doesn't always seem to operate as expected)
        ret = hc.Delete
        hc.target_type = target_type
        hc.target = target
        hc.email = email
        hc.desc = desc
        ret = hc.Create
      End If
    Else
      hc.target_type = target_type
      hc.target = target
      hc.email = email
      hc.desc = desc
      ret = hc.Create
      hc.updated_fields = String.Format("target_type: '{0}', target: '{1}', email: '{2}', desc: '{3}'", target_type, target, email, desc)
    End If

    Return hc
  End Function

  Private Function Create() As HttpStatusCode
    Dim httpres As HttpWebResponse = Me.ExecuteHTTPRequest("POST")

    If httpres IsNot Nothing Then
      status = httpres.StatusCode
      last_action = "HANDLE_CREATION"
      Me.UpdateDatabase("Create")
    Else
      status = HttpStatusCode.InternalServerError
    End If
    Return status

  End Function

  Private Function Delete() As HttpStatusCode
    Dim temp As Boolean = dotrace
    dotrace = False
    Dim s As HttpStatusCode = Me.Retrieve()
    dotrace = temp

    If s = HttpStatusCode.OK Then
      Dim httpres As HttpWebResponse = Me.ExecuteHTTPRequest("DELETE")
      If httpres IsNot Nothing Then
        status = httpres.StatusCode
        last_action = "HANDLE_DELETION"
        Me.UpdateDatabase("Delete")
      Else
        status = HttpStatusCode.InternalServerError
      End If
      Return status
    Else
      Return s
    End If
  End Function

  Private Function Retrieve() As HttpStatusCode
    last_action = "HANDLE_RETRIEVAL"

    Dim ret As HttpStatusCode
    Dim httpres As HttpWebResponse = Me.ExecuteHTTPRequest("GET")

    If httpres IsNot Nothing Then
      ret = httpres.StatusCode

      If ret = HttpStatusCode.OK Then
        Dim receiveStream As Stream = httpres.GetResponseStream()

        Dim xml As New XmlDocument

        Dim readStream As New StreamReader(receiveStream, Encoding.UTF8)
        Dim xmlString As String = readStream.ReadToEnd()
        readStream.Close()

        xml.LoadXml(xmlString)

        Dim nds As XmlNodeList = xml.SelectNodes("/response/handle-value")

        For Each elem As XmlElement In nds
          If elem.GetAttribute("type") = "URL" Then
            target_type = TargetType.URL
            target = elem.GetAttribute("data")
          End If
          If elem.GetAttribute("type") = "FILE" Then
            target_type = TargetType.FILE
            target = elem.GetAttribute("data")
          End If
          If elem.GetAttribute("type") = "EMAIL" Then
            email = elem.GetAttribute("data")
          End If
          If elem.GetAttribute("type") = "DESC" Then
            desc = elem.GetAttribute("data")
          End If
        Next

      End If
    Else
      ret = HttpStatusCode.InternalServerError
    End If
    status = ret
    Return ret
  End Function

  Private Function Update() As HttpStatusCode
    last_action = "HANDLE_UPDATE"

    Dim httpres As HttpWebResponse = Me.ExecuteHTTPRequest("PUT")
    If httpres IsNot Nothing Then
      status = httpres.StatusCode
      last_action = "HANDLE_UPDATE"
      Me.UpdateDatabase("Update")
    Else
      status = HttpStatusCode.InternalServerError
    End If

    Return status

  End Function

  Private Function ExecuteHTTPRequest(ByVal method As String) As HttpWebResponse
    error_message = ""
    Dim valid_methods() As String = {"GET", "POST", "PUT", "DELETE"}
    If Not valid_methods.Contains(method) Then
      Throw New Exception("Unexpected HTTP Method: " & method)
    End If


    Dim ret As HttpStatusCode
    Dim uristr As String = Me.service_url
    If method = "POST" Or method = "PUT" Then
      uristr = uristr & Me.query_string
    End If
    Dim url As New Uri(uristr)
    Dim httpreq As HttpWebRequest = HttpWebRequest.Create(url)
    Dim credentialCache As New CredentialCache()
    credentialCache.Add(New Uri(uristr), "Basic", New NetworkCredential("no-op", HandleClient.password))
    httpreq.Method = method
    httpreq.ContentType = "application/x-www-form-urlencoded"
    httpreq.Credentials = credentialCache

    Dim httpres As HttpWebResponse = Nothing
    Try
      httpres = httpreq.GetResponse
    Catch wex As WebException
      If wex.Response IsNot Nothing Then
        Me.error_message = wex.Message
        httpres = wex.Response
        Me.error_message = String.Format("{0} ({1})", Me.error_message, httpres.StatusDescription)
        If dotrace Then Trace.TraceWarning("Handle Service Error: {2} '{1}' {0}", Me.error_message, uristr, method)
      Else
        Me.error_message = wex.Message
        Trace.TraceWarning("Handle Service Error: {2} '{1}' {0}", Me.error_message, uristr, method)
        Return Nothing
      End If
    Catch oex As Exception
      Me.error_message = oex.Message
      Trace.TraceWarning("Handle Service Error: {2} '{1}' {0}", Me.error_message, uristr, method)
      Return Nothing
    End Try

    ret = httpres.StatusCode
    If method <> "GET" Then httpres.Close()

    status = ret

    Return httpres

  End Function

  Public Shared ReadOnly Property prefix As String
    Get
      Return MedusaAppSettings.Settings.HandlePrefix
    End Get
  End Property

  Private Shared ReadOnly Property service_base_url As String
    Get
      Return MedusaAppSettings.Settings.HandleServiceURL
    End Get
  End Property

  Private Shared ReadOnly Property resource_type As String
    Get
      Return MedusaAppSettings.Settings.HandleResourceType
    End Get
  End Property

  Private Shared ReadOnly Property password As String
    Get
      Return MedusaAppSettings.Settings.HandlePassword
    End Get
  End Property

  Public Shared ReadOnly Property resolver_base_url As String
    Get
      Return MedusaAppSettings.Settings.HandleResolverBaseURL
    End Get
  End Property

  Private ReadOnly Property service_url As String
    Get
      Return String.Format("{0}/{1}/{2}", HandleClient.service_base_url, HandleClient.resource_type, Me.handle_value)
    End Get
  End Property

  Public ReadOnly Property handle_value As String
    Get
      Return String.Format("{0}/{1}", HandleClient.prefix, Me.local_id)
    End Get
  End Property

  Public ReadOnly Property handle_resolver As String
    Get
      Return (String.Format("{0}/{1}", HandleClient.resolver_base_url, Me.handle_value))
    End Get
  End Property

  Private ReadOnly Property query_string() As String
    Get
      Dim ret As String = ""
      If Me.target_type = TargetType.URL Then
        ret = String.Format("url={0}", Uri.EscapeDataString(Me.target))
      ElseIf Me.target_type = TargetType.FILE Then
        ret = String.Format("file={0}", Uri.EscapeDataString(Me.target))
      Else
        Throw New Exception("Unexpected target_type: " & Me.target_type)
      End If

      If Not String.IsNullOrWhiteSpace(Me.email) Then
        ret = String.Format("{0}&email={1}", ret, Uri.EscapeDataString(Me.email))
      End If

      If Not String.IsNullOrWhiteSpace(Me.desc) Then
        ret = String.Format("{0}&desc={1}", ret, Uri.EscapeDataString(Me.desc))
      End If

      Return String.Format("?{0}", ret)
    End Get
  End Property

  Private Sub UpdateDatabase(action As String)
    If status = HttpStatusCode.Created Or status = HttpStatusCode.NoContent Then

      Using db As New HandleManagerDataContext
        Dim ha As New HandleAction
        ha.action = action
        ha.date = Now
        ha.desc = Me.desc
        ha.email = Me.email
        ha.handle = Me.handle_value
        ha.netid = Principal.WindowsIdentity.GetCurrent.Name
        ha.target = Me.target
        ha.target_type = [Enum].GetName(GetType(TargetType), Me.target_type)

        db.HandleActions.InsertOnSubmit(ha)

        db.SubmitChanges()
      End Using

    End If
  End Sub

  ''' <summary>
  ''' Return a PremisEvent that represents the last action taken by the handle client
  ''' </summary>
  ''' <param name="id"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetPremisEvent(id As PremisIdentifier) As PremisEvent
    Dim pEvt As New PremisEvent(id.IdentifierType, id.IdentifierValue, last_action)

    pEvt.EventOutcomeInformation.Add(New PremisEventOutcomeInformation([Enum].GetName(GetType(HttpStatusCode), Me.status)))
    If Not String.IsNullOrWhiteSpace(Me.error_message) Then
      pEvt.EventOutcomeInformation.First.EventOutcomeDetails.Add(New PremisEventOutcomeDetail(Me.error_message))
    End If
    If Not String.IsNullOrWhiteSpace(Me.updated_fields) Then
      pEvt.EventOutcomeInformation.First.EventOutcomeDetails.Add(New PremisEventOutcomeDetail(String.Format("Handle: {0}, {1}", Me.handle_value, Me.updated_fields.Trim(", "))))
    End If

    Return pEvt
  End Function

End Class

Public Enum TargetType
  URL = 0
  FILE = 1
End Enum
