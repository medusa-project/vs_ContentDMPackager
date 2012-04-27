Imports System.Xml

Public Class PremisPreservationLevel
  Public Property PreservationLevelValue As String
  Public Property PreservationLevelRole As String
  Public Property PreservationLevelRationale As List(Of String)
  Public Property PreservationLevelDateAssigned As Date?

  Protected Sub New()
    'no empty constuctors allowed
  End Sub

  Public Sub New(ByVal value As String)
    PreservationLevelValue = value
    PreservationLevelRationale = New List(Of String)
  End Sub

  Public Sub New(ByVal value As String, dt As Date)
    PreservationLevelValue = value
    PreservationLevelDateAssigned = dt
    PreservationLevelRationale = New List(Of String)
  End Sub

  Public Sub New(ByVal value As String, role As String, dt As Date)
    PreservationLevelValue = value
    PreservationLevelRole = role
    PreservationLevelDateAssigned = dt
    PreservationLevelRationale = New List(Of String)
  End Sub

  Public Sub New(ByVal value As String, role As String)
    PreservationLevelValue = value
    PreservationLevelRole = role
    PreservationLevelRationale = New List(Of String)
  End Sub

  Public Sub GetXML(ByVal xmlwr As XmlWriter)
    xmlwr.WriteStartElement("preservationLevel")

    If Not String.IsNullOrWhiteSpace(PreservationLevelValue) Then
      xmlwr.WriteElementString("preservationLevelValue", PreservationLevelValue)
    End If

    If Not String.IsNullOrWhiteSpace(PreservationLevelRole) Then
      xmlwr.WriteElementString("preservationLevelRole", PreservationLevelRole)
    End If

    For Each r As String In PreservationLevelRationale
      xmlwr.WriteElementString("preservationLevelRationale", r)
    Next

    If PreservationLevelDateAssigned.HasValue Then
      xmlwr.WriteElementString("preservationLevelDateAssigned", PreservationLevelDateAssigned.Value.ToString("s"))
    End If

    xmlwr.WriteEndElement()

  End Sub
End Class
