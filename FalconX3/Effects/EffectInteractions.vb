Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing

Namespace VisualEffects.Animations.Effects
	<Flags>
	Public Enum EffectInteractions
		X = 1
		Y = 2
		WIDTH = 8
		HEIGHT = 4
		COLOR = 16
		TRANSPARENCY = 32
		LOCATION = X Or Y
		SIZE = WIDTH Or HEIGHT
		BOUNDS = X Or Y Or WIDTH Or HEIGHT
	End Enum
End Namespace
