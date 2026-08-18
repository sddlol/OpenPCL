Public Class PageLoginAccessTokenSkin

    Private ReadOnly TokenTimer As New Threading.DispatcherTimer With {.Interval = TimeSpan.FromSeconds(1)}

    Public Sub New()
        InitializeComponent()
        Skin.Loader = PageLaunchLeft.SkinAccessToken
        AddHandler TokenTimer.Tick, Sub() RefreshTokenInfo()
    End Sub

    Private Sub PageLoginAccessTokenSkin_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Skin.Loader.Start()
        TokenTimer.Start()
        RefreshTokenInfo()
    End Sub

    Private Sub PageLoginAccessTokenSkin_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        TokenTimer.Stop()
    End Sub

    Public Sub Reload(KeepInput As Boolean)
        TextName.Text = Settings.Get(Of String)("CacheAccessTokenName")
        RefreshTokenInfo()
    End Sub

    Private Sub RefreshTokenInfo()
        Dim ExpiresAt = McLoginAccessTokenExpires(Settings.Get(Of String)("LoginAccessToken"))
        If ExpiresAt = Long.MinValue Then
            ExpiresAt = Settings.Get(Of Long)("CacheAccessTokenExpires")
            If ExpiresAt <= 0 Then ExpiresAt = Long.MinValue
        End If
        If ExpiresAt = Long.MinValue Then
            TextTokenInfo.Text = "到期时间：未知" & vbCrLf & "剩余时间：未知"
            Return
        End If
        Try
            Dim RemainingSeconds = ExpiresAt - GetUnixTimestampUtc()
            Dim Expiry = DateTimeOffset.FromUnixTimeSeconds(ExpiresAt).LocalDateTime
            If RemainingSeconds <= 0 Then
                TextTokenInfo.Text = $"到期时间：{Expiry:yyyy-MM-dd HH:mm:ss}" & vbCrLf & "剩余时间：已过期"
                Return
            End If
            Dim Remaining = TimeSpan.FromSeconds(RemainingSeconds)
            TextTokenInfo.Text = $"到期时间：{Expiry:yyyy-MM-dd HH:mm:ss}" & vbCrLf &
                                 $"剩余时间：{Remaining.Days} 天 {Remaining.Hours} 小时 {Remaining.Minutes} 分 {Remaining.Seconds} 秒"
        Catch
            TextTokenInfo.Text = "到期时间：未知" & vbCrLf & "剩余时间：未知"
        End Try
    End Sub

    Private Sub ShowPanel(sender As Object, e As MouseEventArgs) Handles PanData.MouseEnter
        AniStart(AaOpacity(PanButtons, 1 - PanButtons.Opacity, 120), "PageLoginAccessTokenSkin Button")
    End Sub

    Public Sub HidePanel(sender As Object, e As EventArgs) Handles PanData.MouseLeave
        If BtnEdit.ContextMenu.IsOpen OrElse BtnSkin.ContextMenu.IsOpen OrElse PanData.IsMouseOver Then Return
        AniStart(AaOpacity(PanButtons, -PanButtons.Opacity, 120), "PageLoginAccessTokenSkin Button")
    End Sub

    Private Sub BtnSkin_Click(sender As Object, e As RoutedEventArgs) Handles BtnSkin.Click
        BtnSkin.ContextMenu.IsOpen = True
    End Sub

    Private Sub BtnEdit_Click(sender As Object, e As EventArgs) Handles BtnEdit.Click
        BtnEdit.ContextMenu.IsOpen = True
    End Sub

    Private Sub BtnExit_Click() Handles BtnExit.Click
        Settings.Set("LoginAccessToken", "")
        Settings.Set("CacheAccessTokenUuid", "")
        Settings.Set("CacheAccessTokenName", "")
        Settings.Set("CacheAccessTokenProfileJson", "")
        Settings.Set("CacheAccessTokenExpires", Long.MinValue)
        McLoginAccessTokenLoader.Cancel()
        PageLaunchLeft.SkinAccessToken.Cancel()
        If FrmLoginAccessToken IsNot Nothing Then FrmLoginAccessToken.TextAccessToken.Password = ""
        FrmLaunchLeft.RefreshPage(False, True)
    End Sub

    Public Sub BtnSkinRefresh_Click(sender As Object, e As RoutedEventArgs)
        Skin.RefreshClick()
    End Sub

    Public Sub BtnSkinSave_Click(sender As Object, e As RoutedEventArgs)
        Skin.BtnSkinSave_Click()
    End Sub

    Private IsChanging As Boolean = False
    Public Sub BtnSkinEdit_Click(sender As Object, e As RoutedEventArgs)
        If IsChanging Then
            Hint("正在更改皮肤中，请稍候！")
            Return
        End If
        Dim SkinInfo As McSkinInfo = McSkinSelect()
        If Not SkinInfo.IsVaild Then Return
        IsChanging = True
        Hint("正在更改皮肤……")
        RunInNewThread(Sub() EditSkin(SkinInfo), "Access Token Skin Upload")
    End Sub

    Private Sub EditSkin(SkinInfo As McSkinInfo)
        Try
            Dim AccessToken = Settings.Get(Of String)("LoginAccessToken")
            Dim Result As String = NetRequestByClientRetry("https://api.minecraftservices.com/minecraft/profile/skins", HttpMethod.Post,
                Content:=New Net.Http.MultipartFormDataContent From {
                    {New Net.Http.StringContent(If(SkinInfo.IsSlim, "slim", "classic")), "variant"},
                    {New Net.Http.ByteArrayContent(FileUtils.ReadAsBytes(SkinInfo.LocalFile)), "file", PathUtils.GetLastPart(SkinInfo.LocalFile)}
                },
                Headers:={{"Authorization", "Bearer " & AccessToken}, {"Accept", "*/*"}, {"User-Agent", "MojangSharp/0.1"}})
            Dim ResultJson As JObject = Result.DeserializeJson()
            If ResultJson.ContainsKey("errorMessage") Then Throw New Exception(ResultJson("errorMessage").ToString)
            Dim ActiveSkin = ResultJson("skins").FirstOrDefault(Function(Item) Item("state")?.ToString = "ACTIVE")
            If ActiveSkin Is Nothing Then Throw New Exception("服务器未返回启用的皮肤")

            WriteIni(PathTemp & "Cache\Skin\IndexMs.ini", Settings.Get(Of String)("CacheAccessTokenUuid"), ActiveSkin("url").ToString)
            PageLaunchLeft.SkinAccessToken.WaitForExit(IsForceRestart:=True)
            Hint("更改皮肤成功！", HintType.Green)
        Catch ex As HttpRequestCodeException When ex.StatusCode = HttpStatusCode.Unauthorized
            Logger.Warn(ex, "Access Token 已失效")
            Hint("Access Token 已失效，请重新输入！", HintType.Red)
        Catch ex As Exception
            Logger.Error(ex, "更改皮肤失败", LogBehavior.Toast)
        Finally
            IsChanging = False
        End Try
    End Sub

End Class
