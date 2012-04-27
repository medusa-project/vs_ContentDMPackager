
Public Class PremisIdentifier
  Implements IEquatable(Of PremisIdentifier)


  Public Property IdentifierType As String

  Public Property IdentifierValue As String

  Sub New(ByVal type As String, ByVal value As String)
    IdentifierType = type
    IdentifierValue = value
  End Sub

  Sub New(value As String)
    IdentifierType = "LOCAL"
    IdentifierValue = value
  End Sub

  Public Overloads Function Equals(ByVal other As PremisIdentifier) As Boolean Implements System.IEquatable(Of PremisIdentifier).Equals
    Return Me.IdentifierType = other.IdentifierType AndAlso Me.IdentifierValue = other.IdentifierValue
  End Function

  Public Overrides Function Equals(ByVal other As Object) As Boolean
    If other Is Nothing Then Return MyBase.Equals(other)

    If Not TypeOf other Is PremisIdentifier Then
      Throw New InvalidCastException("The 'obj' argument is not a PremisIdentifier object.")
    Else
      Return Equals(DirectCast(other, PremisIdentifier))
    End If
  End Function

  Public Overrides Function GetHashCode() As Integer
    Return String.Format("{0}:{1}", Me.IdentifierType, Me.IdentifierValue).GetHashCode()
  End Function

  Public Shared Operator =(ByVal id1 As PremisIdentifier, ByVal id2 As PremisIdentifier) As Boolean
    Return id1.Equals(id2)
  End Operator

  Public Shared Operator <>(ByVal id1 As PremisIdentifier, ByVal id2 As PremisIdentifier) As Boolean
    Return Not id1.Equals(id2)
  End Operator

End Class

