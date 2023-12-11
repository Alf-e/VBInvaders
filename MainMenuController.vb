Public Class MainMenuController
    Private MenuWindow As Form1
    Private TableLayout As TableLayoutPanel
    Private WithEvents StartButton, SettingsButton, ExitButton As Button
    Private TitleLabel As Label
    Public Sub New(form As Form1)
        MenuWindow = form
        InitialiseDisplay()
    End Sub

    Private Sub InitialiseDisplay()
        MenuWindow.FormBorderStyle = FormBorderStyle.Fixed3D
        MenuWindow.BackColor = ColorTranslator.FromHtml("#FF344C")
        MenuWindow.Text = "VBInvaders"

        MenuWindow.Width = 1000
        MenuWindow.Height = 600
        MenuWindow.MaximizeBox = False

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
        TitleLabel.Text = "VB Invaders"
        TitleLabel.Margin = New Padding(0, 10, 0, 0)
        TitleLabel.Font = New Font("Arial", 50, FontStyle.Bold)
        TitleLabel.TextAlign = ContentAlignment.MiddleCenter
        TitleLabel.AutoSize = True
        TitleLabel.Anchor = AnchorStyles.None
        TitleLabel.ForeColor = Color.White
        TableLayout.SetColumn(TitleLabel, 1)
        TableLayout.SetRow(TitleLabel, 1)



        StartButton = New Button()
        StartButton.Text = "Start Game"
        StartButton.Font = New Font("Arial", 16, FontStyle.Bold)
        StartButton.Margin = New Padding(0, 50, 0, 10)
        StartButton.ForeColor = Color.White
        StartButton.FlatStyle = FlatStyle.Popup
        StartButton.Width = 200
        StartButton.Height = 75
        StartButton.Anchor = AnchorStyles.None
        StartButton.TextAlign = ContentAlignment.MiddleCenter
        StartButton.BackColor = ColorTranslator.FromHtml("#56A637")
        TableLayout.Controls.Add(StartButton)
        TableLayout.SetColumn(StartButton, 1)
        TableLayout.SetRow(StartButton, 2)

        SettingsButton = New Button()
        SettingsButton.Text = "Settings"
        SettingsButton.ForeColor = Color.White
        SettingsButton.Font = New Font("Arial", 16, FontStyle.Bold)
        SettingsButton.Margin = New Padding(0, 10, 0, 10)
        SettingsButton.FlatStyle = FlatStyle.Popup
        SettingsButton.Width = 200
        SettingsButton.Height = 75
        SettingsButton.Anchor = AnchorStyles.None
        SettingsButton.TextAlign = ContentAlignment.MiddleCenter
        SettingsButton.BackColor = ColorTranslator.FromHtml("#FD7A00")
        TableLayout.Controls.Add(SettingsButton)
        TableLayout.SetColumn(SettingsButton, 1)
        TableLayout.SetRow(SettingsButton, 3)

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

        StartButton.Focus()
    End Sub

    Private Sub GameStartPressed() Handles StartButton.Click
        For Each control As Control In Form1.Controls
            control.Dispose()
        Next
        MenuWindow.Controls.Clear()
        MenuWindow.GameStart()

    End Sub

    Private Sub SettingsPressed() Handles SettingsButton.Click
        For Each control As Control In Form1.Controls
            control.Dispose()
        Next
        MenuWindow.Controls.Clear()
        MenuWindow.SettingsLoad()
    End Sub

    Private Sub ExitPressed() Handles ExitButton.Click
        MenuWindow.Close()
    End Sub
End Class
