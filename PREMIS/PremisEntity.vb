Imports System.Xml
Imports System.IO
Imports System.Text

Public MustInherit Class PremisEntity

  Public Property XmlId As String

  Public MustOverride Sub GetXML(xmlwr As XmlWriter, pCont As PremisContainer, Optional IncludeSchemaLocation As Boolean = False)

  Public MustOverride Function GetDefaultFileName(prefix As String, ext As String) As String


  ''' <summary>
  ''' Return just the index suffix part of a local identifier, i.e. for the identifier "MEDUSA:XX-XXX-XX-XXX=X.00005" this function
  ''' would return "00005"
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetDefaultFileNameIndex() As String
    Return Path.GetExtension(Me.GetDefaultFileName("", "")).TrimStart(".")
  End Function

  Public Sub SaveXML(ByVal fileName As String, pCont As PremisContainer)

    Using txtwr As New StreamWriter(fileName, False, Encoding.UTF8)
      Using xmlwr As XmlWriter = XmlWriter.Create(txtwr, New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})
        Me.GetXML(xmlwr, pCont, True)
        xmlwr.Close()
      End Using
    End Using
  End Sub

End Class
