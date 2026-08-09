Public Class PageLoginAccessToken

    Public Sub Reload(KeepInput As Boolean)
        If Not KeepInput Then TextAccessToken.Password = Settings.Get(Of String)("LoginAccessToken")
        Dim Name = Settings.Get(Of String)("CacheAccessTokenName")
        LabAccount.Text = If(Name = "", "尚未验证", Name)
    End Sub

    Public Shared Function GetLoginData() As McLoginAccessToken
        Dim Token As String = Settings.Get(Of String)("LoginAccessToken")
        If FrmLoginAccessToken IsNot Nothing Then
            RunInUiWait(Sub() Token = FrmLoginAccessToken.TextAccessToken.Password.Trim)
        End If
        Return New McLoginAccessToken With {.AccessToken = Token}
    End Function

    Public Shared Function IsVaild(LoginData As McLoginAccessToken) As String
        If LoginData.AccessToken.Trim = "" Then Return "Access Token 不能为空！"
        Return ""
    End Function

    Public Function IsVaild() As String
        Return IsVaild(GetLoginData())
    End Function

    Private Sub BtnVerify_Click(sender As Object, e As EventArgs) Handles BtnVerify.Click
        TextAccessToken.IsEnabled = False
        BtnVerify.IsEnabled = False
        BtnVerify.Text = "验证中"
        RunInNewThread(
        Sub()
            Try
                McLoginAccessTokenLoader.WaitForExit(GetLoginData(), IsForceRestart:=True)
                RunInUi(
                Sub()
                    LabAccount.Text = McLoginAccessTokenLoader.Output.Name
                    FrmLaunchLeft.RefreshPage(True, False)
                    Hint("Access Token 验证成功！", HintType.Green)
                End Sub)
            Catch ex As Exception
                If Not ex.IsCanceled Then Logger.Error(ex, "Access Token 验证失败", LogBehavior.Alert)
            Finally
                RunInUi(
                Sub()
                    TextAccessToken.IsEnabled = True
                    BtnVerify.IsEnabled = True
                    BtnVerify.Text = "验证"
                End Sub)
            End Try
        End Sub, "Access Token Login")
    End Sub

End Class
