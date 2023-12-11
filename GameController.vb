
Imports System.Configuration
Imports System.Media
Imports System.Net.Mime.MediaTypeNames
Imports System.Transactions
Imports System.Drawing
Imports Image = System.Drawing.Image
Imports System.IO
Imports System.ComponentModel
Imports Microsoft.VisualBasic.Devices
Imports NAudio.Wave


Public Class GameController
    Private HitboxDisplay As Boolean = False
    Private WithEvents GameWindow As Form1
    Private PlayerShip As Defender
    Private Boss As BossAlien
    Private WithEvents BossWaitTimer As New Timer() With {.Interval = 2000}
    Private WithEvents GameOverPopup As GameOverlay

    Public Sub New(form As Form1)
        GameWindow = form
        CheckHitboxDisplay()
        Form1.BossStatus = CheckBossStateFile()
        InitialiseDisplay()
        InitialiseGamestate()

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
    Private Sub CheckHitboxDisplay()
        If CheckHitboxSetting() Then
            HitboxDisplay = True
        Else
            HitboxDisplay = False
        End If
    End Sub
    Private Sub InitialiseGamestate()
        PlayerShip = New Defender(GameWindow, HitboxDisplay)
        If Form1.BossStatus Then
            CreateBoss()
        Else
            Alien.PopulateAlienList(GameWindow, HitboxDisplay)

        End If

    End Sub
    Public Sub CreateBoss()
        BossWaitTimer.Start()
        PlayerShip.DisableInput()
    End Sub
    Public Sub KillBoss()
        Boss = Nothing
    End Sub
    Private Sub BossWaitOver() Handles BossWaitTimer.Tick
        BossWaitTimer.Stop()
        If (IsNothing(PlayerShip.Image)) = False Then
            Boss = New BossAlien(GameWindow, PlayerShip, HitboxDisplay, PlayerShip)
            PlayerShip.AddBossReference(Boss)
        End If
        Form1.BossStatus = True
        UpdateBossStateFile()

    End Sub
    Public Function GetDefenderSize() As Size
        Return PlayerShip.Size
    End Function
    Public Function GetDefenderLocation() As Point
        Return PlayerShip.Location
    End Function
    Private Sub InitialiseDisplay()

        GameWindow.Location = New Point(20, 20)
        GameWindow.Size = New Size(1200, 1000)
        GameWindow.BackColor = Color.Black
        GameWindow.FormBorderStyle = FormBorderStyle.None
        GameWindow.MaximizeBox = False
    End Sub
    Public Shared Function DetectCollision(objloc1 As Point, objsize1 As Size, objloc2 As Point, objsize2 As Size) As Boolean
        If (objloc1.X >= objloc2.X - (objsize1.Width - 1)) And (objloc1.X <= objloc2.X + (objsize2.Width - 1)) Then
            If (objloc1.Y >= objloc2.Y - (objsize1.Height - 1)) And (objloc1.Y <= objloc2.Y + (objsize2.Height - 1)) Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Sub GameOver()
        Me.PlayerShip.Kill()
        Me.PlaySoundAsync("explosion.wav")
        Me.GameOverPopup = New LossOverlay(Me.GameWindow)
    End Sub
    Public Sub GameWin()
        Form1.BossStatus = False
        UpdateBossStateFile()
        Me.GameOverPopup = New WinOverlay(Me.GameWindow)

    End Sub
    Public Sub onFocus() Handles GameWindow.GotFocus
        If IsNothing(GameOverPopup) = False Then
            GameOverPopup.Focus()
        End If
    End Sub
    Private Sub EscapeQuitGame(sender As Object, e As KeyEventArgs) Handles GameWindow.KeyDown
        'keyvalue is 27 for escape
        If e.KeyValue = 27 Then
            Form1.Close()
        End If
    End Sub

    Public Sub PlaySoundAsync(soundName As String)
        PlaySound(soundName)
    End Sub

    Private Sub PlaySound(soundName As String)
        Dim Wave1 As New NAudio.Wave.WaveOut 'Wave out device for playing the sound

        Dim audioFileReader As New NAudio.Wave.AudioFileReader(Form1.FilePathAudio + soundName)

        Wave1.Init(audioFileReader)

        Wave1.Volume = Form1.AudioVolume

        Wave1.Play()

    End Sub

    Private Sub WindowClosing() Handles GameWindow.FormClosing
        UpdateBossStateFile()
    End Sub

    Private Sub UpdateBossStateFile()
        Using writer As StreamWriter = File.CreateText("bosssetting.txt")
            writer.WriteLine(Form1.BossStatus)
        End Using

    End Sub

    Private Function CheckBossStateFile() As Boolean
        Dim filepath As String = "bosssetting.txt"
        If File.Exists(filepath) Then
            Using reader As StreamReader = File.OpenText(filepath)
                If reader.ReadLine() = "True" Then
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
#Region "GameObjects"
    Private Class Alien
        Inherits PictureBox
        Private imagev1, imagev2 As Image
        Public Shared ExplosionEffect As Image
        Public Shared MovementDirection As Direction = Direction.RIGHT
        Private Shared AlienArmy As List(Of Alien)
        Public Shared WithEvents ArmyMoveTimer As New Timer() With {.Interval = 500}
        Private WithEvents DeathTimer As New Timer() With {.Interval = 300}
        Public Shared ExplosionSize As Size
        Private Shared AlienProjectile As Projectile
        Private Shared GameWindow As Form1
        Private Shared HitboxDisplay As Boolean

        Public Sub New(form As Form1, column As Byte, row As Byte, hitboxbool As Boolean)

            Me.BackColor = Color.Transparent
            ScaleUpImage(row)
            InitialisePosition(column, row)
            Me.Visible = True
            Alien.HitboxDisplay = hitboxbool
            If Alien.HitboxDisplay Then
                Me.BorderStyle = BorderStyle.FixedSingle
            End If

            form.Controls.Add(Me)
            GameWindow = form
        End Sub
        Public Shared Function GetProjRef() As AlienProjectile
            Return Alien.AlienProjectile
        End Function
        Public Shared Sub KillProjRef()
            Alien.AlienProjectile = Nothing
        End Sub
        Private Sub InitialisePosition(column As Byte, row As Byte)
            Dim Xcoefficient As Int16 = 75
            Dim Ycoefficient As Int16 = 70


            Dim Xposition As Int16 = (Xcoefficient * column) - 25
            Dim Yposition As Int16 = (Ycoefficient * row) - 25
            If row = 1 Then
                Xposition += 7
            End If
            Me.Location = New Point(Xposition, Yposition)
            'Alien 0 start coords
            'x = 57
            'y = 45
        End Sub

        Private Sub ScaleUpImage(row As Byte)
            Dim scaleFactor As Integer = 5 ' Set the desired scaling factor
            Dim imagetype As String
            Dim loopfactor As Int16
            If IsNothing(Alien.ExplosionEffect) Then
                loopfactor = 3
            Else
                loopfactor = 2
            End If
            For index = 1 To loopfactor
                imagetype = "Alien"
                Select Case row
                    Case 1
                        imagetype &= "2"
                    Case 2, 3
                        imagetype &= "1"
                    Case 4, 5
                        imagetype &= "3"
                End Select
                Select Case index
                    Case 1
                        imagetype &= "a"
                    Case 2
                        imagetype &= "b"
                    Case 3
                        imagetype = "ExplosionF4"
                End Select
                Dim originalImage As Image = New Bitmap(Form1.FilePath & imagetype & ".png") ' Load your small image from resources
                If imagetype = "ExplosionF4" Then
                    Alien.ExplosionSize = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size
                Else
                    Me.Size = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size
                End If

                Dim scaledImage As New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor, Imaging.PixelFormat.Format32bppArgb)

                Using g As Graphics = Graphics.FromImage(scaledImage)

                    For y As Integer = 0 To originalImage.Height - 1
                        For x As Integer = 0 To originalImage.Width - 1
                            Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                            If pixelColor <> ColorTranslator.FromHtml("#000000") Then

                                pixelColor = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                                For i As Integer = 0 To scaleFactor - 1
                                    For j As Integer = 0 To scaleFactor - 1
                                        scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                    Next j
                                Next i
                            End If


                        Next x
                    Next y
                End Using
                Select Case index
                    Case 1
                        Me.imagev1 = scaledImage ' Set the scaled image to the PictureBox
                    Case 2
                        Me.imagev2 = scaledImage ' Set the scaled image to the PictureBox
                    Case 3
                        Alien.ExplosionEffect = scaledImage
                End Select

            Next
            Me.Image = Me.imagev1
        End Sub

        Private Sub MoveLocation(location As Point)
            Me.Location = location
            If Me.Image.Equals(ExplosionEffect) = False Then
                If Me.Image.Equals(imagev1) Then
                    Me.Image = imagev2
                Else
                    Me.Image = imagev1
                End If
            End If
            If GameController.DetectCollision(Me.Location, Me.Size, GameWindow.Game.GetDefenderLocation(), GameWindow.Game.GetDefenderSize()) Then
                GameWindow.Game.GameOver()
            End If


        End Sub

        Public Sub Kill()
            GameWindow.Game.PlaySoundAsync("invaderkilled.wav")
            Me.MoveLocation(New Point(Me.Location.X - ((Me.Location.X + (Alien.ExplosionSize.Width \ 2)) - (Me.Location.X + (Me.Width \ 2))), Me.Location.Y))
            Me.Size = Alien.ExplosionSize
            Me.Image = Alien.ExplosionEffect
            DeathTimer.Start()

        End Sub

        Private Sub DeathEffect() Handles DeathTimer.Tick
            Me.Dispose()
            AlienArmy.Remove(Me)
            DeathTimer.Stop()
        End Sub
        Private Shared Sub MoveAliens()
            Dim Xcoefficient As Int16
            Dim Ycoefficient As Int16
            Dim SmallestXpos As Int16 = 5000
            Dim LargestXpos As Int16 = 0
            GameWindow.Game.PlaySoundAsync("fastinvader1.wav")

            Select Case Alien.MovementDirection
                Case Direction.RIGHT
                    Xcoefficient = 10
                Case Direction.LEFT
                    Xcoefficient = -10
            End Select
            For Each A In Alien.AlienArmy
                A.MoveLocation(New Point(A.Location.X + Xcoefficient, A.Location.Y + Ycoefficient))
                If A.Location.X < SmallestXpos Then
                    SmallestXpos = A.Location.X
                End If
                If A.Location.X > LargestXpos Then
                    LargestXpos = A.Location.X
                End If
            Next


            If ((SmallestXpos <= 57) Or (LargestXpos >= 1085)) Then
                For Each A In Alien.AlienArmy
                    A.MoveLocation(New Point(A.Location.X, A.Location.Y + 30))
                Next
                If Alien.MovementDirection = Direction.RIGHT Then
                    Alien.MovementDirection = Direction.LEFT
                Else
                    Alien.MovementDirection = Direction.RIGHT
                End If
            End If
        End Sub

        Public Shared Sub PopulateAlienList(GameWindow As Form1, hitboxbool As Boolean)
            Alien.AlienArmy = New List(Of Alien)
            Alien.MovementDirection = Direction.RIGHT
            For row = 0 To 4 'default=4 any higher than 4 breaks imagetype loading logic
                For column = 0 To 10 'default=10 any higher than 10 breaks the down movement
                    Alien.AlienArmy.Add(New Alien(GameWindow, column + 1, row + 1, hitboxbool))
                Next
            Next
            Alien.ArmyMoveTimer.Start()
        End Sub

        Private Shared Sub ArmyMoveTimerTick() Handles ArmyMoveTimer.Tick
            If AlienArmy.Count <> 0 Then
                Alien.MoveAliens()
                Alien.TryFireAlienProj()
            End If
        End Sub

        Private Shared Sub TryFireAlienProj()
            Dim RandomAlien As Alien = GetRandomAlien()
            If IsNothing(Alien.AlienProjectile) Then
                Alien.AlienProjectile = New AlienProjectile(GameWindow, New Point(RandomAlien.Location.X + (RandomAlien.Width / 2), RandomAlien.Location.Y + (RandomAlien.Height + 1)), 10, RandomAlien, HitboxDisplay)
            End If

        End Sub
        Public Shared Function GetAlienArmy() As List(Of Alien)
            Return Alien.AlienArmy
        End Function
        Private Shared Function GetRandomAlien() As Alien
            Randomize()
            Return Alien.AlienArmy.Item(CInt(Int((Alien.AlienArmy.Count * Rnd()) + 1) - 1))
        End Function

    End Class
    Private Class Defender
        Inherits PictureBox

        Private MoveDirection As Direction
        Private CurrentProjectile As DefenderProjectile
        Private WithEvents InputSourceWindow As Form1
        Private Boss As BossAlien
        Private WithEvents DMoveTimer As New Timer() With {.Interval = 1}
        Private WithEvents DeathTimer As New Timer() With {.Interval = 300}
        Public CanInput, Shooting As Boolean
        Private MaxLeft, MaxRight As Int16
        Private HitboxDisplay As Boolean


        Public Sub New(form As Form1, hitboxbool As Boolean)
            CanInput = True
            Shooting = False
            MaxLeft = 85
            MaxRight = 1030
            ScaleUpImage()
            Me.Location = New Point(550, 900)
            Me.Visible = True
            form.Controls.Add(Me)
            InputSourceWindow = form
            Me.HitboxDisplay = hitboxbool
            If Me.HitboxDisplay Then
                Me.BorderStyle = BorderStyle.FixedSingle
            End If

        End Sub
        Public Sub AddBossReference(bossRef As BossAlien)
            Me.Boss = bossRef
        End Sub
        Public Sub UpdateMaxRight()
            MaxRight = 1500
        End Sub
        Public Sub DisableInput()
            CanInput = False

        End Sub

        Public Sub EnableInput()
            CanInput = True

        End Sub

        Public Sub KillDefProjReference()
            CurrentProjectile = Nothing
        End Sub

        Public Sub MoveDefender()
            If CanInput Then
                Dim MoveAmount As Int16
                If MoveDirection = Direction.LEFT And Me.Location.X > MaxLeft Then
                    MoveAmount = -5
                ElseIf MoveDirection = Direction.RIGHT And Me.Location.X < MaxRight Then
                    MoveAmount = 5
                End If
                Me.Location = New Point(Me.Location.X + MoveAmount, Me.Location.Y)
            End If

        End Sub

        Public Sub MoveDefender(loc As Point)

            Me.Location = loc
        End Sub
        Private Sub UserInputDown(sender As Object, e As KeyEventArgs) Handles InputSourceWindow.KeyDown
            If CanInput Then
                If e.KeyValue = 37 Then
                    Me.MoveDirection = Direction.LEFT
                    DMoveTimer.Start()
                ElseIf e.KeyValue = 39 Then
                    Me.MoveDirection = Direction.RIGHT
                    DMoveTimer.Start()
                End If
                If (e.KeyValue = 32) Then

                    TryShoot()
                    Shooting = True
                End If
            End If







        End Sub
        Private Sub UserInputReleased(sender As Object, e As KeyEventArgs) Handles InputSourceWindow.KeyUp

            If (e.KeyValue = 37 And Me.MoveDirection = Direction.LEFT) Or (e.KeyValue = 39 And Me.MoveDirection = Direction.RIGHT) Then
                Me.MoveDirection = Nothing
                DMoveTimer.Stop()
            End If
            If e.KeyValue = 32 Then
                Shooting = False
            End If

        End Sub
        Public Sub TryShoot()
            If IsNothing(CurrentProjectile) And CanInput Then
                CurrentProjectile = New DefenderProjectile(InputSourceWindow, New Point(Me.Location.X + 25, Me.Location.Y - 45), -25, Me, HitboxDisplay, Boss)

            End If
        End Sub
        Private Sub DefenderTimerTick(sender As Object, e As EventArgs) Handles DMoveTimer.Tick
            Me.MoveDefender()
        End Sub
        Private Sub ScaleUpImage()
            Dim scaleFactor As Integer = 5 ' Set the desired scaling factor

            Dim originalImage As Image = New Bitmap(Form1.FilePath & "Defender.png") ' Load your small image from resources
            Me.Size = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size
            Dim scaledImage As New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor)

            Using g As Graphics = Graphics.FromImage(scaledImage)
                For y As Integer = 0 To originalImage.Height - 1
                    For x As Integer = 0 To originalImage.Width - 1
                        Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                        If pixelColor <> ColorTranslator.FromHtml("#000000") Then
                            For i As Integer = 0 To scaleFactor - 1
                                For j As Integer = 0 To scaleFactor - 1
                                    scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                Next j
                            Next i
                        End If

                    Next x
                Next y
            End Using
            Me.Image = scaledImage
        End Sub

        Public Sub Kill()
            Me.DisableInput()
            Me.Size = Alien.ExplosionSize
            Me.Image = Alien.ExplosionEffect
            DeathTimer.Start()
        End Sub

        Private Sub DeathEffect() Handles DeathTimer.Tick
            Me.Image = Nothing
            Me.Location = New Point(0 - Me.Width, 0 - Me.Height)
            'InputSourceWindow.Game.DestroyPlayerReference()
            DeathTimer.Stop()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
            Me.DeathTimer.Stop()
            Me.DMoveTimer.Stop()



        End Sub
    End Class
    Private Class Projectile
        Inherits PictureBox

        Protected Shared imagev1, imagev2 As Image
        Protected WithEvents ProjectileTimer As New Timer() With {.Interval = 20}
        Protected ProjectileWindow As Form1
        Protected Shared FinalSize As Size
        Protected MoveAmount As Int16
        Private HitboxDisplay As Boolean

        Protected Sub New(form As Form1, location As Point, moveamount As Int16, hitboxbool As Boolean)
            If IsNothing(imagev1) Then
                LoadScaledUpImages()
            End If
            Me.Size = FinalSize
            Me.Image = imagev1
            Me.Location = location
            form.Controls.Add(Me)
            ProjectileWindow = form

            Me.MoveAmount = moveamount
            Me.BackColor = Color.Transparent
            Me.BringToFront()

            Me.HitboxDisplay = hitboxbool
            If Me.HitboxDisplay Then
                Me.BorderStyle = BorderStyle.FixedSingle
            End If

            ProjectileTimer.Start()


        End Sub


        Protected Overridable Sub ProjectileTimerTick(sender As Object, e As EventArgs) Handles ProjectileTimer.Tick
            If Me.Location.Y <= 0 Or Me.Location.Y >= 1200 Then
                KillProjectile()


            Else
                MoveLocation(New Point(Me.Location.X, Me.Location.Y + MoveAmount))
            End If
        End Sub
        Public Overridable Sub KillProjectile()
            ProjectileWindow.Controls.Remove(Me)
            Me.Dispose()






        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)

            Me.ProjectileTimer.Stop()


        End Sub


        Public Sub LoadScaledUpImages()
            Dim scaleFactor As Integer = 5 ' Set the desired scaling factor
            Dim imagetype As String

            For index = 1 To 2
                Select Case index
                    Case 1
                        imagetype = "ProjectileA"
                    Case 2
                        imagetype = "ProjectileB"

                End Select


                Dim originalImage As Image = New Bitmap(Form1.FilePath & imagetype & ".png") ' Load your small image from resources

                FinalSize = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size


                Dim scaledImage As New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor, Imaging.PixelFormat.Format32bppArgb)

                Using g As Graphics = Graphics.FromImage(scaledImage)

                    For y As Integer = 0 To originalImage.Height - 1
                        For x As Integer = 0 To originalImage.Width - 1
                            Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                            If pixelColor <> ColorTranslator.FromHtml("#000000") Then
                                For i As Integer = 0 To scaleFactor - 1
                                    For j As Integer = 0 To scaleFactor - 1
                                        scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                    Next j
                                Next i
                            End If

                        Next x
                    Next y
                End Using
                Select Case index
                    Case 1
                        imagev1 = scaledImage ' Set the scaled image to the PictureBox
                    Case 2
                        imagev2 = scaledImage ' Set the scaled image to the PictureBox
                End Select
            Next
        End Sub

        Public Sub MoveLocation(location As Point)
            Me.Location = location
            If Me.Image.Equals(imagev1) Then
                Me.Image = imagev2
            Else
                Me.Image = imagev1
            End If
        End Sub


    End Class
    Private Class DefenderProjectile
        Inherits Projectile

        Private WithEvents DeathTimer As New Timer() With {.Interval = 300}
        Private Shared ExplosionEffect As Image
        Private Shared ExplosionSize As Size
        Private Boss As BossAlien
        Private Host As Defender
        Public Sub New(form As Form1, location As Point, moveamount As Int16, hostdefender As Defender, hitboxbool As Boolean, boss As BossAlien)
            MyBase.New(form, location, moveamount, hitboxbool)
            form.Game.PlaySoundAsync("shoot.wav")
            Host = hostdefender
            Me.Boss = boss
            LoadExplosionEffect()
        End Sub
        Private Sub DeathTick() Handles DeathTimer.Tick
            DeathTimer.Stop()
            MyBase.KillProjectile()
            Host.KillDefProjReference()
            If Host.Shooting Then
                Host.TryShoot()
            End If
        End Sub

        Protected Overrides Sub ProjectileTimerTick(sender As Object, e As EventArgs)
            MyBase.ProjectileTimerTick(sender, e)
            If IsNothing(Boss) = False Then
                If GameController.DetectCollision(Me.Location, Me.Size, Boss.Location, Boss.Size) Then
                    Boss.ProjHitTrigger()
                    KillProjectile()
                End If
                For Each proj In Me.Boss.FetchProjectiles()
                    If GameController.DetectCollision(Me.Location, Me.Size, proj.Location, proj.Size) Then
                        proj.KillProjectile()
                        KillProjectile()
                        Exit For
                    End If

                Next

            End If

            If IsNothing(Alien.GetAlienArmy()) = False Then
                For Each A As Alien In Alien.GetAlienArmy
                    If GameController.DetectCollision(Me.Location, Me.Size, A.Location, A.Size) Then
                        If Alien.GetAlienArmy().Count = 1 Then
                            'Boss spawn start
                            ProjectileWindow.Game.CreateBoss()
                        End If
                        A.Kill()
                        KillProjectile()
                        Exit For
                    End If

                Next
                If IsNothing(Alien.GetProjRef()) = False Then
                    If GameController.DetectCollision(Me.Location, Me.Size, Alien.GetProjRef().Location, Alien.GetProjRef().Size) Then
                        Alien.GetProjRef().KillProjectile()
                        KillProjectile()
                    End If
                End If



            End If




        End Sub

        Private Sub LoadExplosionEffect()
            Dim scaleFactor As Integer = 3 ' Set the desired scaling factor


            Dim originalImage As Image = New Bitmap(Form1.FilePath & "ExplosionF4.png") ' Load your small image from resources

            ExplosionSize = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size


            Dim scaledImage As New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor, Imaging.PixelFormat.Format32bppArgb)

            Using g As Graphics = Graphics.FromImage(scaledImage)

                For y As Integer = 0 To originalImage.Height - 1
                    For x As Integer = 0 To originalImage.Width - 1
                        Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                        If pixelColor <> ColorTranslator.FromHtml("#000000") Then
                            For i As Integer = 0 To scaleFactor - 1
                                For j As Integer = 0 To scaleFactor - 1
                                    scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                Next j
                            Next i
                        End If

                    Next x
                Next y
            End Using

            ExplosionEffect = scaledImage ' Set the scaled image to the PictureBox

        End Sub

        Public Overrides Sub KillProjectile()
            MyBase.ProjectileTimer.Stop()
            MyBase.Location = New Point(Me.Location.X - ((Me.Location.X + (ExplosionSize.Width \ 2)) - (Me.Location.X + (Me.Width \ 2))), Me.Location.Y)
            MyBase.Image = ExplosionEffect
            MyBase.Size = ExplosionSize
            Me.DeathTimer.Start()
        End Sub
    End Class
    Private Class AlienProjectile
        Inherits Projectile

        Private HostAlien As Alien
        Public Sub New(form As Form1, location As Point, moveamount As Int16, host As Alien, hitboxbool As Boolean)
            MyBase.New(form, location, moveamount, hitboxbool)
            Me.HostAlien = host
        End Sub
        Protected Overrides Sub ProjectileTimerTick(sender As Object, e As EventArgs)
            MyBase.ProjectileTimerTick(sender, e)

            If GameController.DetectCollision(Me.Location, Me.Size, ProjectileWindow.Game.GetDefenderLocation(), ProjectileWindow.Game.GetDefenderSize()) Then
                KillProjectile()

                ProjectileWindow.Game.GameOver()
            End If

        End Sub

        Public Overrides Sub KillProjectile()
            MyBase.KillProjectile()
            Alien.KillProjRef()
        End Sub
        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)
            Alien.KillProjRef()
        End Sub


    End Class
    Private Class BossProjectile
        Inherits Projectile

        Private HostBoss As BossAlien
        Sub New(form As Form1, location As Point, moveamount As Int16, hitboxbool As Boolean, hostboss As BossAlien)
            MyBase.New(form, location, moveamount, hitboxbool)
            Me.HostBoss = hostboss
        End Sub

        Protected Overrides Sub ProjectileTimerTick(sender As Object, e As EventArgs)
            MyBase.ProjectileTimerTick(sender, e)
            If IsNothing(ProjectileWindow.Game) = False Then
                If GameController.DetectCollision(Me.Location, Me.Size, ProjectileWindow.Game.GetDefenderLocation(), ProjectileWindow.Game.GetDefenderSize()) Then
                    KillProjectile()

                    ProjectileWindow.Game.GameOver()
                End If
            End If


        End Sub

        Public Overrides Sub KillProjectile()
            MyBase.KillProjectile()
            HostBoss.FetchProjectiles().Remove(Me)

        End Sub
        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)
            HostBoss.FetchProjectiles().Remove(Me)
        End Sub
    End Class
    Private Class BossAlien
        Inherits PictureBox
        Private Shared GameWindow As Form1
        Private WithEvents ColourTimer As New Timer() With {.Interval = 1}
        Private WithEvents IntroMoveTimer As New Timer() With {.Interval = 1}
        Private WithEvents MoveCycleTimer As New Timer() With {.Interval = 1}
        Private WithEvents ProjFireTimer As New Timer() With {.Interval = 500}
        Private WithEvents ExplosionTimer As Timer
        Private ProjectileList As New List(Of BossProjectile)
        Private ColourAmount, CurrentExplosionFrame As Int16
        Private ColourUp, HitboxDisplay As Boolean
        Private BlackImage, WhiteImage As Image
        Private ExplosionSprites As New List(Of Image)
        Private Player As Defender
        Private BossHealthBar As Healthbar
        Private moveDirection As Direction = Direction.RIGHT
        Private ExplosionSize As Size
        Public Sub New(form As Form1, playership As Defender, hitboxbool As Boolean, player As Defender)
            ScaleUpImage()
            LoadExplosionImages()

            Me.BackColor = Color.Transparent
            GameWindow = form
            Me.Location = New Point((GameWindow.Width / 2) - (Me.Width / 2), 0 - Me.Height)
            Me.Visible = True
            Me.Player = player
            form.Controls.Add(Me)

            Me.HitboxDisplay = hitboxbool
            If Me.HitboxDisplay Then
                Me.BorderStyle = BorderStyle.FixedSingle
            End If

            Me.CurrentExplosionFrame = 0

            ColourTimer.Start()
            IntroMoveTimer.Start()
        End Sub

        Public Function FetchProjectiles() As List(Of BossProjectile)
            Return Me.ProjectileList
        End Function
        Private Sub ColourTimerTick() Handles ColourTimer.Tick
            GameWindow.BackColor = Color.FromArgb(ColourAmount, 0, 0)
            If ColourAmount > 220 Or ColourAmount = 0 Then
                ColourUp = Not ColourUp
            End If
            If ColourUp Then
                ColourAmount += 7
            Else
                ColourAmount -= 7
            End If
            'GameWindow.Location = New Point(GameWindow.Location.X + 1, GameWindow.Location.Y)
            GameWindow.Width += 1
        End Sub
        Private Sub IntroMoveTick() Handles IntroMoveTimer.Tick
            Me.Location = New Point((GameWindow.Width / 2) - (Me.Width / 2), Me.Location.Y + 1)
            Player.Location = New Point((GameWindow.Width / 2) - (Player.Width / 2), Player.Location.Y)
            If Me.Location.Y = 50 Then
                BeginFightPhase()

            End If
        End Sub
        Private Sub ScaleUpImage()
            Dim scaleFactor As Integer = 50 ' Set the desired scaling factor
            Dim originalImage As Image = New Bitmap(Form1.FilePath & "Alien1b.png") ' Load your small image from resources

            Me.Size = New Size(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor) ' Adjust the PictureBox size
            For index = 1 To 2
                Dim scaledImage As New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor, Imaging.PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(scaledImage)

                    For y As Integer = 0 To originalImage.Height - 1
                        For x As Integer = 0 To originalImage.Width - 1
                            Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                            If pixelColor <> ColorTranslator.FromHtml("#000000") Then

                                pixelColor = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                                For i As Integer = 0 To scaleFactor - 1
                                    For j As Integer = 0 To scaleFactor - 1
                                        If index = 2 Then
                                            pixelColor = Color.Black
                                        Else
                                            pixelColor = Color.White
                                        End If
                                        scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                    Next j
                                Next i
                            End If


                        Next x
                    Next y
                End Using
                If index = 1 Then
                    Me.WhiteImage = scaledImage
                Else
                    Me.BlackImage = scaledImage
                End If
            Next

            Me.Image = BlackImage
        End Sub
        Private Sub LoadExplosionImages()
            Dim scaleFactor As Integer = 50 ' Set the desired scaling factor
            Dim scaledImage As Bitmap
            For index = 1 To 7


                Dim originalImage As Image = New Bitmap(Form1.FilePath & "ExplosionF" & index.ToString() & ".png") ' Load your small image from resources
                scaledImage = New Bitmap(originalImage.Width * scaleFactor, originalImage.Height * scaleFactor, Imaging.PixelFormat.Format32bppArgb)

                Using g As Graphics = Graphics.FromImage(scaledImage)
                    For y As Integer = 0 To originalImage.Height - 1
                        For x As Integer = 0 To originalImage.Width - 1
                            Dim pixelColor As Color = DirectCast(originalImage, Bitmap).GetPixel(x, y)
                            If pixelColor <> ColorTranslator.FromHtml("#000000") Then
                                For i As Integer = 0 To scaleFactor - 1
                                    For j As Integer = 0 To scaleFactor - 1
                                        scaledImage.SetPixel(x * scaleFactor + i, y * scaleFactor + j, pixelColor)
                                    Next j
                                Next i
                            End If
                        Next x
                    Next y
                End Using
                Me.ExplosionSprites.Add(scaledImage)

            Next
            Me.ExplosionSize = scaledImage.Size

        End Sub
        Public Sub ProjHitTrigger()
            Me.BossHealthBar.ShowHealth()
        End Sub
        Public Sub MoveLocation(newLoc As Point)
            Me.Location = newLoc


            Me.BossHealthBar.Location = New Point(Me.Location.X + ((Me.Width / 2) - (BossHealthBar.Width / 2)), Me.Location.Y + ((Me.Height / 2) - (BossHealthBar.Height / 2)))
            'Syncs the bosshealthbar with the boss location
        End Sub
        Private Sub BeginFightPhase()
            IntroMoveTimer.Stop()
            ColourTimer.Stop()
            GameWindow.BackColor = Color.Black
            Me.Image = WhiteImage
            Player.EnableInput()
            Player.UpdateMaxRight()
            BossHealthBar = New Healthbar(GameWindow, Me.Size, Me.Location, Me)
            MoveCycleTimer.Start()
            ProjFireTimer.Start()
        End Sub
        Public Sub DeathProcedure()
            Player.DisableInput()
            Me.MoveCycleTimer.Stop()
            StartDeathAnimation()


        End Sub
        Private Sub StartDeathAnimation()
            GameWindow.Game.PlaySoundAsync("explosion.wav")
            Me.Image = Me.ExplosionSprites(Me.ExplosionSprites.Count - 1)
            Me.MoveLocation(New Point(Me.Location.X + ((Me.Width / 2) - (Me.ExplosionSize.Width / 2)), Me.Location.Y))
            Me.Size = Me.ExplosionSize
            Me.ExplosionTimer = New Timer() With {.Interval = 150}
            Me.ExplosionTimer.Start()
            Me.ProjFireTimer.Stop()
        End Sub
        Private Sub StepExplosionAnimation() Handles ExplosionTimer.Tick
            Me.Image = Me.ExplosionSprites(Me.CurrentExplosionFrame)
            Me.CurrentExplosionFrame += 1

            If Me.CurrentExplosionFrame = Me.ExplosionSprites.Count Then
                Me.ExplosionTimer.Stop()
                Me.Kill()
            End If
        End Sub
        Private Sub Kill()
            GameWindow.Game.GameWin()
            Me.Dispose()
            GameWindow.Game.KillBoss()
        End Sub
        Private Sub MoveTimer() Handles MoveCycleTimer.Tick
            Dim moveAmount As Int16
            If (Me.Location.X + Me.Width) >= (GameWindow.DesktopLocation.X + GameWindow.Width) - 40 Then
                moveDirection = Direction.LEFT
            ElseIf (Me.Location.X) <= 40 Then
                moveDirection = Direction.RIGHT

            End If
            If moveDirection = Direction.RIGHT Then
                moveAmount = 3
            Else
                moveAmount = -3
            End If
            Me.MoveLocation(New Point(Me.Location.X + moveAmount, Me.Location.Y))

        End Sub
        Private Sub FireTimer() Handles ProjFireTimer.Tick
            ProjectileList.Add(New BossProjectile(GameWindow, New Point(Me.Location.X + 15, Me.Location.Y + (Me.Height - 50)), 10, HitboxDisplay, Me))
            ProjectileList.Add(New BossProjectile(GameWindow, New Point(Me.Location.X + (Me.Width - 15), Me.Location.Y + (Me.Height - 50)), 10, HitboxDisplay, Me))

        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)

            ColourTimer.Stop()
            IntroMoveTimer.Stop()
            MoveCycleTimer.Stop()
            ProjFireTimer.Stop()
            If IsNothing(Me.ExplosionTimer) = False Then
                ExplosionTimer.Stop()
            End If


        End Sub
    End Class
    Private Class Healthbar
        Inherits PictureBox
        Private test As Int16
        Private HealthTotal As Int16 = 50
        Private WithEvents AppearTimer As New Timer() With {.Interval = 500}
        Private WithEvents DamageAnimationTimer As New Timer() With {.Interval = 100}
        Private HostBoss As BossAlien
        Private GameWindow As Form1

        Public Sub New(form As Form1, alienSize As Size, alienLocation As Point, bossref As BossAlien)
            Me.BackColor = Color.Gray
            InitialiseVisual(alienSize)
            Me.Location = New Point(alienLocation.X + ((alienSize.Width / 2) - (Me.Width / 2)), alienLocation.Y + ((alienSize.Height / 2) - (Me.Height / 2)))
            Me.Visible = False
            form.Controls.Add(Me)
            Me.BringToFront()
            Me.HostBoss = bossref
            GameWindow = form

        End Sub

        Public Sub ShowHealth()
            If HealthTotal > 0 Then
                'default = HealthTotal > 0
                GameWindow.Game.PlaySoundAsync("bosshitnoise.wav")
                Me.HealthTotal -= 1
                Me.Visible = True
                AppearTimer.Start()
                DamageAnimationTimer.Start()
            Else
                Me.HostBoss.DeathProcedure()
            End If
        End Sub

        Private Sub AppearTimerTick() Handles AppearTimer.Tick
            Me.Visible = False

            AppearTimer.Stop()
            DamageAnimationTimer.Stop()

        End Sub
        Private Sub DamageAnimationTick() Handles DamageAnimationTimer.Tick
            Dim numberOfTicks As Int16 = 5
            Dim healthTotal As Int16 = 50
            Dim segmentWidth As Int16 = (Me.Width / healthTotal) / numberOfTicks

            Dim segmentLocation As Int16
            Dim healthbarImage As New Bitmap(Me.Width, Me.Height, Imaging.PixelFormat.Format32bppArgb)
            healthbarImage = Me.Image

            Using g As Graphics = Graphics.FromImage(healthbarImage)

                For x As Integer = Me.Width - 1 To 0 Step -1
                    If healthbarImage.GetPixel(x, Me.Height - 1).ToString() = "Color [A=255, R=50, G=205, B=50]" Then

                        segmentLocation = x
                        Exit For
                    End If
                Next
                For x As Integer = segmentLocation To segmentLocation - segmentWidth Step -1
                    For y As Integer = healthbarImage.Height - 1 To 0 Step -1
                        Dim pixelColor As Color = Color.Transparent

                        healthbarImage.SetPixel(x, y, pixelColor)

                    Next y
                Next x
            End Using

            Me.Image = healthbarImage


        End Sub

        Private Sub InitialiseVisual(alienSize As Size)
            Me.Size = New Size(alienSize.Width + 50, alienSize.Height / 7)
            'Added 50 to healthbar width to allow perfect segmentation in damage animation
            Dim scaledImage As New Bitmap(Me.Width, Me.Height, Imaging.PixelFormat.Format32bppArgb)

            Using g As Graphics = Graphics.FromImage(scaledImage)

                For y As Integer = 0 To scaledImage.Height - 1
                    For x As Integer = 0 To scaledImage.Width - 1
                        Dim pixelColor As Color = Color.LimeGreen

                        scaledImage.SetPixel(x, y, pixelColor)

                    Next x
                Next y
            End Using
            Me.Image = scaledImage
        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
            Me.AppearTimer.Stop()
            Me.DamageAnimationTimer.Stop()


        End Sub
    End Class
    Private Class GameOverlay
        Inherits Form

        Protected Window As Form1

        Private WithEvents AppearTimer As New Timer() With {.Interval = 10}

        Protected Sub New(hostform As Form)
            ' Created a new form to display over the existing one
            Me.Window = hostform
            Me.FormBorderStyle = FormBorderStyle.None
            Me.Location = hostform.PointToScreen(Point.Empty)
            Me.ShowInTaskbar = False
            Me.ControlBox = False
            Me.StartPosition = FormStartPosition.Manual
            Me.Size = hostform.Size
            Me.Opacity = 0
            Me.BackColor = Color.White
            Me.Show()
            LoadScreenItems()
            AppearTimer.Start()
        End Sub
        Protected Overridable Sub AppearTimerTick() Handles AppearTimer.Tick
            Me.Opacity += 0.01

            If Me.Opacity > 0.5 Then
                AnimationEnd()

            End If
        End Sub

        Protected Overridable Sub AnimationEnd()
            AppearTimer.Stop()

            Me.Focus()
        End Sub
        Protected Overridable Sub LoadScreenItems()




        End Sub


        Protected Overridable Sub onFocus() Handles Me.GotFocus


        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)

            Me.AppearTimer.Stop()
            Me.AppearTimer = Nothing

        End Sub

    End Class
    Private Class WinOverlay
        Inherits GameOverlay
        Private ButtonForm, LabelForm As ItemForm
        Private GameWinLabel As Label
        Private WithEvents GameWinButton As Button
        Public Sub New(hostform As Form)
            MyBase.New(hostform)


        End Sub

        Protected Overrides Sub LoadScreenItems()
            'load custom items for win display
            LoadLabel()
            LoadButton()
        End Sub

        Private Sub LoadLabel()
            GameWinLabel = New Label()
            GameWinLabel.Text = "VICTORY"


            GameWinLabel.Font = New Font("Arial", 50, FontStyle.Bold)
            GameWinLabel.TextAlign = ContentAlignment.MiddleCenter
            GameWinLabel.Width = 441
            GameWinLabel.Height = 78
            ' GameWinLabel.AutoSize = True

            GameWinLabel.ForeColor = Color.White
            GameWinLabel.BackColor = Color.Red
            GameWinLabel.Location = New Point(0, 0)

            LabelForm = New ItemForm(New Size(GameWinLabel.Width, GameWinLabel.Height), New Point((Me.Location.X + (Me.Width / 2)) - (GameWinLabel.Width / 2), (Me.Location.Y + (Me.Height / 4))))
            LabelForm.Controls.Add(GameWinLabel)

            GameWinLabel.BringToFront()
        End Sub

        Private Sub LoadButton()
            GameWinButton = New Button()

            GameWinButton.Text = "Main Menu"
            GameWinButton.Font = New Font("Arial", 16, FontStyle.Bold)

            GameWinButton.ForeColor = Color.White
            GameWinButton.FlatStyle = FlatStyle.Popup
            GameWinButton.Width = 200
            GameWinButton.Height = 75

            GameWinButton.TextAlign = ContentAlignment.MiddleCenter
            GameWinButton.BackColor = ColorTranslator.FromHtml("#56A637")
            GameWinButton.Location = New Point(0, 0)
            ButtonForm = New ItemForm(New Size(GameWinButton.Width, GameWinButton.Height), New Point((Me.Location.X + (Me.Width / 2)) - (GameWinButton.Width / 2), (Me.Location.Y + (Me.Height / 2)) - GameWinButton.Height / 2))
            ButtonForm.Controls.Add(GameWinButton)
            GameWinButton.BringToFront()
            GameWinButton.Enabled = False

        End Sub
        Protected Overrides Sub AppearTimerTick()
            Me.ButtonForm.Opacity += 0.02
            Me.LabelForm.Opacity += 0.02
            MyBase.AppearTimerTick()
        End Sub
        Protected Overrides Sub onFocus()

            If IsNothing(ButtonForm) = False Then
                LabelForm.Focus()
                ButtonForm.Focus()
                GameWinButton.Focus()
            End If
        End Sub
        Protected Overrides Sub AnimationEnd()
            GameWinButton.Enabled = True
            MyBase.AnimationEnd()
        End Sub
        Private Sub OnButtonPress() Handles GameWinButton.Click
            ButtonForm.Close()
            LabelForm.Close()
            Me.Close()
            For Each control As Control In Form1.Controls
                control.Dispose()
            Next
            Alien.ArmyMoveTimer.Stop()
            Window.Controls.Clear()
            Window.MenuLoad()
        End Sub
    End Class
    Private Class LossOverlay
        Inherits GameOverlay

        Private ButtonForm, LabelForm As ItemForm
        Private GameOverLabel As Label
        Private WithEvents GameOverButton As Button
        Public Sub New(hostform As Form)
            MyBase.New(hostform)

        End Sub
        Protected Overrides Sub LoadScreenItems()
            LoadLabel()
            LoadButton()
        End Sub
        Private Sub LoadLabel()
            GameOverLabel = New Label()
            GameOverLabel.Text = "GAME OVER"


            GameOverLabel.Font = New Font("Arial", 50, FontStyle.Bold)
            GameOverLabel.TextAlign = ContentAlignment.MiddleCenter
            GameOverLabel.Width = 441
            GameOverLabel.Height = 78
            ' GameOverLabel.AutoSize = True

            GameOverLabel.ForeColor = Color.White
            GameOverLabel.BackColor = Color.Red
            GameOverLabel.Location = New Point(0, 0)

            LabelForm = New ItemForm(New Size(GameOverLabel.Width, GameOverLabel.Height), New Point((Me.Location.X + (Me.Width / 2)) - (GameOverLabel.Width / 2), (Me.Location.Y + (Me.Height / 4))))
            LabelForm.Controls.Add(GameOverLabel)

            GameOverLabel.BringToFront()


        End Sub
        Private Sub LoadButton()
            GameOverButton = New Button()

            GameOverButton.Text = "Main Menu"
            GameOverButton.Font = New Font("Arial", 16, FontStyle.Bold)

            GameOverButton.ForeColor = Color.White
            GameOverButton.FlatStyle = FlatStyle.Popup
            GameOverButton.Width = 200
            GameOverButton.Height = 75

            GameOverButton.TextAlign = ContentAlignment.MiddleCenter
            GameOverButton.BackColor = ColorTranslator.FromHtml("#56A637")
            GameOverButton.Location = New Point(0, 0)
            ButtonForm = New ItemForm(New Size(GameOverButton.Width, GameOverButton.Height), New Point((Me.Location.X + (Me.Width / 2)) - (GameOverButton.Width / 2), (Me.Location.Y + (Me.Height / 2)) - GameOverButton.Height / 2))
            ButtonForm.Controls.Add(GameOverButton)
            GameOverButton.BringToFront()
            GameOverButton.Enabled = False

        End Sub
        Protected Overrides Sub AppearTimerTick()
            Me.ButtonForm.Opacity += 0.02
            Me.LabelForm.Opacity += 0.02
            MyBase.AppearTimerTick()
        End Sub
        Protected Overrides Sub onFocus()

            If IsNothing(ButtonForm) = False Then
                LabelForm.Focus()
                ButtonForm.Focus()
                GameOverButton.Focus()
            End If
        End Sub
        Private Sub OnButtonPress() Handles GameOverButton.Click
            ButtonForm.Close()
            LabelForm.Close()
            Me.Close()
            For Each control As Control In Form1.Controls
                control.Dispose()
            Next
            Alien.ArmyMoveTimer.Stop()
            Window.Controls.Clear()
            Window.MenuLoad()
        End Sub
        Protected Overrides Sub AnimationEnd()
            GameOverButton.Enabled = True
            MyBase.AnimationEnd()
        End Sub
    End Class
    Private Class ItemForm
        Inherits Form

        Public Sub New(size As Size, loc As Point)

            Me.FormBorderStyle = FormBorderStyle.None

            Me.ShowInTaskbar = False
            Me.ControlBox = False
            Me.StartPosition = FormStartPosition.Manual
            Me.Opacity = 0
            Me.BackColor = Color.White

            Me.Size = size
            Me.Location = loc

            Me.Show()
        End Sub
    End Class
#End Region

    Private Enum Direction
        LEFT
        RIGHT

    End Enum
End Class
