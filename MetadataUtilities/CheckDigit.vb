''' <summary>
''' Class for calculating and verifying check digits
''' </summary>
''' <remarks>Uses the Luhn Mod N algoithm, see http://en.wikipedia.org/wiki/Luhn_mod_N_algorithm </remarks>
Public Class CheckDigit
  Private Shared chars() As Char = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"}

  Private Shared Function CodePointFromCharacter(ByVal c As Char) As Integer
    Return Array.IndexOf(chars, Char.ToLower(c))
  End Function

  Private Shared Function CharacterFromCodePoint(ByVal codePoint As Integer) As Char
    Return chars(codePoint)
  End Function

  Private Shared Function NumberOfValidInputCharacters() As Integer
    Return chars.Length
  End Function

  Public Shared Function GenerateCheckCharacter(ByVal input As String) As Char
    Dim factor As Integer = 2
    Dim sum As Integer = 0
    Dim n As Integer = NumberOfValidInputCharacters()

    'Starting from the right and working leftwards is easier since 
    'the initial "factor" will always be "2" 
    For i As Integer = input.Length - 1 To 0 Step -1
      Dim codePoint As Integer = CodePointFromCharacter(input.Chars(i))
      Dim addend As Integer = factor * codePoint

      'Alternate the "factor" that each "codePoint" is multiplied by
      factor = If(factor = 2, 1, 2)

      'Sum the digits of the "addend" as expressed in base "n"
      addend = (addend \ n) + (addend Mod n)
      sum = sum + addend
    Next

    'Calculate the number that must be added to the "sum" 
    'to make it divisible by "n"
    Dim remainder As Integer = sum Mod n
    Dim checkCodePoint As Integer = n - remainder
    checkCodePoint = checkCodePoint Mod n

    Return CharacterFromCodePoint(checkCodePoint)

  End Function

  Public Shared Function ValidateCheckCharacter(ByVal input As String) As Boolean

    Dim factor As Integer = 1
    Dim sum As Integer = 0
    Dim n As Integer = NumberOfValidInputCharacters()

    'Starting from the right, work leftwards
    'Now, the initial "factor" will always be "1" 
    'since the last character is the check character
    For i As Integer = input.Length - 1 To 0 Step -1
      Dim codePoint As Integer = CodePointFromCharacter(input.Chars(i))
      Dim addend As Integer = factor * codePoint

      'Alternate the "factor" that each "codePoint" is multiplied by
      factor = If(factor = 2, 1, 2)

      'Sum the digits of the "addend" as expressed in base "n"
      addend = (addend \ n) + (addend Mod n)
      sum = sum + addend
    Next

    Dim remainder As Integer = sum Mod n

    Return (remainder = 0)
  End Function

End Class
