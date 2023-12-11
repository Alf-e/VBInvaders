Imports System.Diagnostics.Tracing
Imports System.IO
Imports Microsoft.VisualBasic.Devices

Public Class SettingsMenuController
    Private MenuWindow As Form1
    Private TableLayout, AudioSection As TableLayoutPanel
    Private WithEvents ExitButton As Button
    Private WithEvents HitboxCheckBox As CheckBox
    Private TitleLabel, AudioLabel As Label
    Private AudioBar As TrackBar

    Public Sub New(form As Form1)
        MenuWindow = form
        InitialiseDisplay()
        HitboxCheckBox.Focus()
    End Sub

    Private Function CheckHitboxSetting() As Boolean
        Dim filepath As String = "hitboxsetting.txt"
        If File.Exists(filepath) Then
            Using reader As StreamReader = File.OpenText(filepath)
                If reader.ReadLine() = "true" Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Else
            Using fs As FileStream = File.Create(filepath)
                fs.Close()
            End Using
            Return False
        End If

    End Function

    Private Function CheckSoundSetting() As Double
        Dim filepath As String = "audiosetting.txt"
        If File.Exists(filepath) Then
            Using reader As StreamReader = File.OpenText(filepath)
                Return reader.ReadLine()
            End Using
        Else
            Using fs As FileStream = File.Create(filepath)
                fs.Close()
            End Using
            Return 1
        End If
    End Function

    Private Sub WriteHitboxOutcomeToSetting()
        Using writer As StreamWriter = File.CreateText("hitboxsetting.txt")
            If HitboxCheckBox.Checked Then
                writer.WriteLine("true")
            Else
                writer.WriteLine("false")
            End If

        End Using
    End Sub
    Private Sub WriteAudioOutcomeToSetting()
        Using writer As StreamWriter = File.CreateText("audiosetting.txt")
            writer.WriteLine(AudioBar.Value)
        End Using
        Form1.AudioVolume = AudioBar.Value / 10
    End Sub

    Private Sub InitialiseDisplay()
        MenuWindow.BackColor = ColorTranslator.FromHtml("#FF344C")
        MenuWindow.Text = "VBInvaders"

        MenuWindow.Width = 1000
        MenuWindow.Height = 600

        TableLayout = New TableLayoutPanel()
        MenuWindow.Controls.Add(TableLayout)
        TableLayout.ColumnCount = 3
        TableLayout.RowCount = 6
        TableLayout.Dock = DockStyle.Fill
        TableLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        TableLayout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        TableLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        TableLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        'TableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single

        TitleLabel = New Label()
        TableLayout.Controls.Add(TitleLabel)
        TitleLabel.Text = "Settings"
        TitleLabel.Margin = New Padding(0, 10, 0, 0)
        TitleLabel.Font = New Font("Arial", 50, FontStyle.Bold)
        TitleLabel.TextAlign = ContentAlignment.MiddleCenter
        TitleLabel.AutoSize = True
        TitleLabel.Anchor = AnchorStyles.None
        TitleLabel.ForeColor = Color.White
        TableLayout.SetColumn(TitleLabel, 1)
        TableLayout.SetRow(TitleLabel, 1)

        HitboxCheckBox = New CheckBox()
        HitboxCheckBox.Checked = CheckHitboxSetting()
        HitboxCheckBox.Text = "     Hitbox Display"
        HitboxCheckBox.ForeColor = Color.White
        HitboxCheckBox.Font = New Font("Arial", 14, FontStyle.Bold)
        HitboxCheckBox.Margin = New Padding(0, 10, 0, 10)
        HitboxCheckBox.FlatStyle = FlatStyle.Popup
        HitboxCheckBox.Width = 200
        HitboxCheckBox.Height = 50
        HitboxCheckBox.Anchor = AnchorStyles.None
        TableLayout.Controls.Add(HitboxCheckBox)
        TableLayout.SetColumn(HitboxCheckBox, 1)
        TableLayout.SetRow(HitboxCheckBox, 2)


        LoadAudioSection()


        ExitButton = New Button()
        ExitButton.Text = "Exit"
        ExitButton.ForeColor = Color.White
        ExitButton.Font = New Font("Arial", 16, FontStyle.Bold)
        ExitButton.Margin = New Padding(0, 10, 0, 10)
        ExitButton.FlatStyle = FlatStyle.Popup
        ExitButton.Width = 200
        ExitButton.Height = 75
        ExitButton.Anchor = AnchorStyles.None
        ExitButton.TextAlign = ContentAlignment.MiddleCenter
        ExitButton.BackColor = ColorTranslator.FromHtml("#EA0000")
        TableLayout.Controls.Add(ExitButton)
        TableLayout.SetColumn(ExitButton, 1)
        TableLayout.SetRow(ExitButton, 4)
    End Sub

    Private Sub CheckBoxFocus() Handles HitboxCheckBox.GotFocus
        HitboxCheckBox.ForeColor = Color.Black
    End Sub

    Private Sub CheckBoxUnfocus() Handles HitboxCheckBox.LostFocus
        HitboxCheckBox.ForeColor = Color.White
    End Sub

    Private Sub ExitPressed() Handles ExitButton.Click
        WriteHitboxOutcomeToSetting()
        WriteAudioOutcomeToSetting()
        For Each control As Control In Form1.Controls
            control.Dispose()
        Next
        MenuWindow.Controls.Clear()
        MenuWindow.MenuLoad()
    End Sub

    Private Sub LoadAudioSection()
        AudioSection = New TableLayoutPanel()

        AudioSection.ColumnCount = 2
        AudioSection.RowCount = 1
        AudioSection.Dock = DockStyle.Fill
        AudioSection.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        AudioSection.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        AudioSection.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        'AudioSection.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        TableLayout.Controls.Add(AudioSection)
        TableLayout.SetRow(AudioSection, 3)
        TableLayout.SetColumn(AudioSection, 1)

        AudioBar = New TrackBar()
        AudioBar.Width = 200
        AudioBar.Maximum = 10
        AudioBar.Minimum = 0
        AudioBar.Value = CheckSoundSetting()
        AudioBar.TickStyle = TickStyle.Both
        AudioBar.Anchor = AnchorStyles.None

        AudioSection.Controls.Add(AudioBar)
        AudioSection.SetColumn(AudioBar, 0)
        AudioSection.SetRow(AudioBar, 0)

        AudioLabel = New Label()
        AudioLabel.Text = "Volume"
        AudioLabel.Width = 200
        AudioLabel.Margin = New Padding(0, 10, 0, 0)
        AudioLabel.Font = New Font("Arial", 14, FontStyle.Bold)
        AudioLabel.TextAlign = ContentAlignment.MiddleCenter
        AudioLabel.AutoSize = True
        AudioLabel.Anchor = AnchorStyles.None

        AudioLabel.ForeColor = Color.Black
        AudioSection.Controls.Add(AudioLabel)
        AudioSection.SetColumn(AudioLabel, 1)
        AudioSection.SetRow(AudioLabel, 0)

    End Sub

End Class
