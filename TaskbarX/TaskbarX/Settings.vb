Option Strict On

Public Class Settings
    Public Shared Pause As Boolean

    Public Shared TaskbarStyle As Integer
    Public Shared SecondaryTaskbarStyle As Integer
    Public Shared PrimaryTaskbarOffset As Integer
    Public Shared SecondaryTaskbarOffset As Integer
    Public Shared CenterPrimaryOnly As Integer
    Public Shared CenterSecondaryOnly As Integer
    Public Shared AnimationStyle As String
    Public Shared AnimationSpeed As Integer
    Public Shared LoopRefreshRate As Integer
    Public Shared CenterInBetween As Integer
    Public Shared FixToolbarsOnTrayChange As Integer
    Public Shared SkipResolution As Integer
    Public Shared CheckFullscreenApp As Integer
    Public Shared DefaultTaskbarStyleOnWinMax As Integer
    Public Shared DontCenterTaskbar As Integer
    Public Shared HidePrimaryStartButton As Integer
    Public Shared HideSecondaryStartButton As Integer
    Public Shared HidePrimaryNotifyWnd As Integer
    Public Shared HideSecondaryNotifyWnd As Integer
    Public Shared ShowTrayIcon As Integer
    Public Shared TaskbarStyleOnMax As Integer
    Public Shared TaskbarStyleRed As Integer
    Public Shared TaskbarStyleGreen As Integer
    Public Shared TaskbarStyleBlue As Integer
    Public Shared TaskbarStyleAlpha As Integer
    Public Shared ConsoleEnabled As Integer

    'If on battery TaskbarX will override with these
    Public Shared OnBatteryAnimationStyle As String

    Public Shared OnBatteryLoopRefreshRate As Integer

End Class