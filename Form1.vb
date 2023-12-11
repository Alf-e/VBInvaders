Imports System.IO

Public Class Form1

    Public Game As GameController
    Private MainMenu As MainMenuController
    Private SettingsPage As SettingsMenuController
    Public Shared FilePath As String = Application.StartupPath.ToString() & "SourceImages\"
    Public Shared FilePathAudio As String = Application.StartupPath.ToString() & "SourceSounds\"
    Public Shared AudioVolume As Double = 0.1
    Public Shared BossStatus As Boolean = True
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MenuLoad()

    End Sub
    Public Sub MenuLoad()
        SettingsPage = Nothing
        Game = Nothing
        MainMenu = New MainMenuController(Me)
    End Sub

    Public Sub GameStart()
        SettingsPage = Nothing
        MainMenu = Nothing
        Game = New GameController(Me)
    End Sub

    Public Sub SettingsLoad()
        Game = Nothing
        MainMenu = Nothing
        SettingsPage = New SettingsMenuController(Me)
    End Sub

End Class
