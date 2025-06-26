using SharpDX.DirectInput;
using System.Windows.Input;
using System.Linq;
using System.Timers;
using System.ComponentModel;

/// <summary>
/// Handles the control, calling and attaching of functions to Key calls.
/// </summary>
/// <remarks>For parameters named key; the TrueBind is needed and 
/// for Parameters named keyBind; the surface key bind is needed</remarks>
static class InputController{
	public static bool CompareTo(Keys key, Keys key2){ return ((int)key2 == (int)key); }
	static KeyPressedDelegate OnA;
	static KeyPressedDelegate OnB;
	static KeyPressedDelegate OnC;
	static KeyPressedDelegate OnD;
	static KeyPressedDelegate OnE;
	static KeyPressedDelegate OnF;
	static KeyPressedDelegate OnG;
	static KeyPressedDelegate OnH;
	static KeyPressedDelegate OnI;
	static KeyPressedDelegate OnJ;
	static KeyPressedDelegate OnK;
	static KeyPressedDelegate OnL;
	static KeyPressedDelegate OnM;
	static KeyPressedDelegate OnN;
	static KeyPressedDelegate OnO;
	static KeyPressedDelegate OnP;
	static KeyPressedDelegate OnQ;
	static KeyPressedDelegate OnR;
	static KeyPressedDelegate OnS;
	static KeyPressedDelegate OnT;
	static KeyPressedDelegate OnU;
	static KeyPressedDelegate OnV;
	static KeyPressedDelegate OnW;
	static KeyPressedDelegate OnX;
	static KeyPressedDelegate OnY;
	static KeyPressedDelegate OnZ;

	// Numbers (top row)
	static KeyPressedDelegate OnD0;
	static KeyPressedDelegate OnD1;
	static KeyPressedDelegate OnD2;
	static KeyPressedDelegate OnD3;
	static KeyPressedDelegate OnD4;
	static KeyPressedDelegate OnD5;
	static KeyPressedDelegate OnD6;
	static KeyPressedDelegate OnD7;
	static KeyPressedDelegate OnD8;
	static KeyPressedDelegate OnD9;

	// Numpad
	static KeyPressedDelegate OnNumPad0;
	static KeyPressedDelegate OnNumPad1;
	static KeyPressedDelegate OnNumPad2;
	static KeyPressedDelegate OnNumPad3;
	static KeyPressedDelegate OnNumPad4;
	static KeyPressedDelegate OnNumPad5;
	static KeyPressedDelegate OnNumPad6;
	static KeyPressedDelegate OnNumPad7;
	static KeyPressedDelegate OnNumPad8;
	static KeyPressedDelegate OnNumPad9;
	static KeyPressedDelegate OnDecimal;
	static KeyPressedDelegate OnAdd;
	static KeyPressedDelegate OnSubtract;
	static KeyPressedDelegate OnMultiply;
	static KeyPressedDelegate OnDivide;

	// Function keys
	static KeyPressedDelegate OnF1;
	static KeyPressedDelegate OnF2;
	static KeyPressedDelegate OnF3;
	static KeyPressedDelegate OnF4;
	static KeyPressedDelegate OnF5;
	static KeyPressedDelegate OnF6;
	static KeyPressedDelegate OnF7;
	static KeyPressedDelegate OnF8;
	static KeyPressedDelegate OnF9;
	static KeyPressedDelegate OnF10;
	static KeyPressedDelegate OnF11;
	static KeyPressedDelegate OnF12;
	static KeyPressedDelegate OnF13;
	static KeyPressedDelegate OnF14;
	static KeyPressedDelegate OnF15;
	static KeyPressedDelegate OnF16;
	static KeyPressedDelegate OnF17;
	static KeyPressedDelegate OnF18;
	static KeyPressedDelegate OnF19;
	static KeyPressedDelegate OnF20;
	static KeyPressedDelegate OnF21;
	static KeyPressedDelegate OnF22;
	static KeyPressedDelegate OnF23;
	static KeyPressedDelegate OnF24;

	// Navigation keys
	static KeyPressedDelegate OnUp;
	static KeyPressedDelegate OnDown;
	static KeyPressedDelegate OnLeft;
	static KeyPressedDelegate OnRight;
	static KeyPressedDelegate OnHome;
	static KeyPressedDelegate OnEnd;
	static KeyPressedDelegate OnPageUp;
	static KeyPressedDelegate OnPageDown;
	static KeyPressedDelegate OnInsert;
	static KeyPressedDelegate OnDelete;

	// Other common keys
	static KeyPressedDelegate OnEscape;
	static KeyPressedDelegate OnTab;
	static KeyPressedDelegate OnCapsLock;
	static KeyPressedDelegate OnBack;
	static KeyPressedDelegate OnEnter;
	static KeyPressedDelegate OnSpace;
	static KeyPressedDelegate OnPrintScreen;
	static KeyPressedDelegate OnScroll;
	static KeyPressedDelegate OnPause;

	// Punctuation and symbols
	static KeyPressedDelegate OnOemtilde;
	static KeyPressedDelegate OnOemMinus;
	static KeyPressedDelegate OnOemplus;
	static KeyPressedDelegate OnOemOpenBrackets;
	static KeyPressedDelegate OnOem6;
	static KeyPressedDelegate OnOem5;
	static KeyPressedDelegate OnOem1;
	static KeyPressedDelegate OnOem7;
	static KeyPressedDelegate OnOemcomma;
	static KeyPressedDelegate OnOemPeriod;
	static KeyPressedDelegate OnOemQuestion;

	// Application and browser keys
	static KeyPressedDelegate OnApps;
	static KeyPressedDelegate OnSleep;
	static KeyPressedDelegate OnBrowserBack;
	static KeyPressedDelegate OnBrowserForward;
	static KeyPressedDelegate OnBrowserRefresh;
	static KeyPressedDelegate OnBrowserStop;
	static KeyPressedDelegate OnBrowserSearch;
	static KeyPressedDelegate OnBrowserFavorites;
	static KeyPressedDelegate OnBrowserHome;
	static KeyPressedDelegate OnVolumeMute;
	static KeyPressedDelegate OnVolumeDown;
	static KeyPressedDelegate OnVolumeUp;
	static KeyPressedDelegate OnMediaNextTrack;
	static KeyPressedDelegate OnMediaPreviousTrack;
	static KeyPressedDelegate OnMediaStop;
	static KeyPressedDelegate OnMediaPlayPause;
	static KeyPressedDelegate OnLaunchMail;
	static KeyPressedDelegate OnSelectMedia;
	static KeyPressedDelegate OnLaunchApplication1;
	static KeyPressedDelegate OnLaunchApplication2;
#pragma warning disable CS8619
	public static readonly (Keys key, Keys TrueKey, KeyPressedDelegate AttachableElement)[] KeyBinds = [
		  (Keys.D0, Keys.D0, OnD0), (Keys.D1, Keys.D1, OnD1), (Keys.D2, Keys.D2, OnD2), (Keys.D3, Keys.D3, OnD3), (Keys.D4, Keys.D4, OnD4)
		, (Keys.D5, Keys.D5, OnD5), (Keys.D6, Keys.D6, OnD6), (Keys.D7, Keys.D7, OnD7), (Keys.D8, Keys.D8, OnD8), (Keys.D9, Keys.D9, OnD9)
		, (Keys.NumPad0, Keys.NumPad0, OnNumPad0), (Keys.NumPad1, Keys.NumPad1, OnNumPad1), (Keys.NumPad2, Keys.NumPad2, OnNumPad2)
		, (Keys.NumPad3, Keys.NumPad3, OnNumPad3), (Keys.NumPad4, Keys.NumPad4, OnNumPad4), (Keys.NumPad5, Keys.NumPad5, OnNumPad5)
		, (Keys.NumPad6, Keys.NumPad6, OnNumPad6), (Keys.NumPad7, Keys.NumPad7, OnNumPad7), (Keys.NumPad8, Keys.NumPad8, OnNumPad8)
		, (Keys.NumPad9, Keys.NumPad9, OnNumPad9), (Keys.Decimal, Keys.Decimal, OnDecimal), (Keys.Add, Keys.Add, OnAdd)
		, (Keys.Subtract, Keys.Subtract, OnSubtract), (Keys.Multiply, Keys.Multiply, OnMultiply), (Keys.Divide, Keys.Divide, OnDivide)
		, (Keys.F1, Keys.F1, OnF1), (Keys.F2, Keys.F2, OnF2), (Keys.F3, Keys.F3, OnF3), (Keys.F4, Keys.F4, OnF4), (Keys.F5, Keys.F5, OnF5)
		, (Keys.F6, Keys.F6, OnF6), (Keys.F7, Keys.F7, OnF7), (Keys.F8, Keys.F8, OnF8), (Keys.F9, Keys.F9, OnF9), (Keys.F10, Keys.F10, OnF10)
		, (Keys.F11, Keys.F11, OnF11), (Keys.F12, Keys.F12, OnF12), (Keys.F13, Keys.F13, OnF13), (Keys.F14, Keys.F14, OnF14)
		, (Keys.F15, Keys.F15, OnF15), (Keys.F16, Keys.F16, OnF16), (Keys.F17, Keys.F17, OnF17), (Keys.F18, Keys.F18, OnF18)
		, (Keys.F19, Keys.F19, OnF19), (Keys.F20, Keys.F20, OnF20), (Keys.F21, Keys.F21, OnF21), (Keys.F22, Keys.F22, OnF22)
		, (Keys.F23, Keys.F23, OnF23), (Keys.F24, Keys.F24, OnF24)
		, (Keys.Up, Keys.Up, OnUp), (Keys.Down, Keys.Down, OnDown), (Keys.Left, Keys.Left, OnLeft), (Keys.Right, Keys.Right, OnRight)
		, (Keys.Home, Keys.Home, OnHome), (Keys.End, Keys.End, OnEnd), (Keys.PageUp, Keys.PageUp, OnPageUp), (Keys.PageDown, Keys.PageDown, OnPageDown)
		, (Keys.Insert, Keys.Insert, OnInsert), (Keys.Delete, Keys.Delete, OnDelete)
		, (Keys.Escape, Keys.Escape, OnEscape), (Keys.Tab, Keys.Tab, OnTab), (Keys.CapsLock, Keys.CapsLock, OnCapsLock)
		, (Keys.Back, Keys.Back, OnBack), (Keys.Enter, Keys.Enter, OnEnter), (Keys.Space, Keys.Space, OnSpace)
		, (Keys.PrintScreen, Keys.PrintScreen, OnPrintScreen), (Keys.Scroll, Keys.Scroll, OnScroll), (Keys.Pause, Keys.Pause, OnPause)
		, (Keys.Oemtilde, Keys.Oemtilde, OnOemtilde), (Keys.OemMinus, Keys.OemMinus, OnOemMinus), (Keys.Oemplus, Keys.Oemplus, OnOemplus)
		, (Keys.OemOpenBrackets, Keys.OemOpenBrackets, OnOemOpenBrackets), (Keys.Oem6, Keys.Oem6, OnOem6), (Keys.Oem5, Keys.Oem5, OnOem5)
		, (Keys.Oem1, Keys.Oem1, OnOem1), (Keys.Oem7, Keys.Oem7, OnOem7), (Keys.Oemcomma, Keys.Oemcomma, OnOemcomma)
		, (Keys.OemPeriod, Keys.OemPeriod, OnOemPeriod), (Keys.OemQuestion, Keys.OemQuestion, OnOemQuestion)
		, (Keys.Apps, Keys.Apps, OnApps), (Keys.Sleep, Keys.Sleep, OnSleep), (Keys.BrowserBack, Keys.BrowserBack, OnBrowserBack)
		, (Keys.BrowserForward, Keys.BrowserForward, OnBrowserForward), (Keys.BrowserRefresh, Keys.BrowserRefresh, OnBrowserRefresh)
		, (Keys.BrowserStop, Keys.BrowserStop, OnBrowserStop), (Keys.BrowserSearch, Keys.BrowserSearch, OnBrowserSearch)
		, (Keys.BrowserFavorites, Keys.BrowserFavorites, OnBrowserFavorites), (Keys.BrowserHome, Keys.BrowserHome, OnBrowserHome)
		, (Keys.VolumeMute, Keys.VolumeMute, OnVolumeMute), (Keys.VolumeDown, Keys.VolumeDown, OnVolumeDown), (Keys.VolumeUp, Keys.VolumeUp, OnVolumeUp)
		, (Keys.MediaNextTrack, Keys.MediaNextTrack, OnMediaNextTrack), (Keys.MediaPreviousTrack, Keys.MediaPreviousTrack, OnMediaPreviousTrack)
		, (Keys.MediaStop, Keys.MediaStop, OnMediaStop), (Keys.MediaPlayPause, Keys.MediaPlayPause, OnMediaPlayPause)
		, (Keys.LaunchMail, Keys.LaunchMail, OnLaunchMail), (Keys.SelectMedia, Keys.SelectMedia, OnSelectMedia)
		, (Keys.LaunchApplication1, Keys.LaunchApplication1, OnLaunchApplication1), (Keys.LaunchApplication2, Keys.LaunchApplication2, OnLaunchApplication2),
		(Keys.A, Keys.A, OnA), (Keys.B, Keys.B, OnB), (Keys.C, Keys.C, OnC), (Keys.D, Keys.D, OnD), (Keys.E, Keys.E, OnE),
		(Keys.F, Keys.F, OnF), (Keys.G, Keys.G, OnG), (Keys.H, Keys.H, OnH), (Keys.I, Keys.I, OnI), (Keys.J, Keys.J, OnJ),
		(Keys.K, Keys.K, OnK), (Keys.L, Keys.L, OnL), (Keys.M, Keys.M, OnM), (Keys.N, Keys.N, OnN), (Keys.O, Keys.O, OnO),
		(Keys.P, Keys.P, OnP), (Keys.Q, Keys.Q, OnQ), (Keys.R, Keys.R, OnR), (Keys.S, Keys.S, OnS), (Keys.T, Keys.T, OnT),
		(Keys.U, Keys.U, OnU), (Keys.V, Keys.V, OnV), (Keys.W, Keys.W, OnW), (Keys.X, Keys.X, OnX), (Keys.Y, Keys.Y, OnY),
		(Keys.Z, Keys.Z, OnZ)
	];
#pragma warning restore CS8619
	static InputController(){
		joySticks = [];
		keyBinds = [];
		TrueKeyToIndex = [];
		int cc = 0;
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        Entry.TUpdate += InvokeKeyHandles;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        foreach ((Keys key, Keys TrueKey, KeyPressedDelegate AttachableElement) item in KeyBinds){
			TrueKeyToIndex[item.TrueKey] = cc;
			cc++;
		}

		OnA = (duration, strength) => { };
		OnB = (duration, strength) => { };
		OnC = (duration, strength) => { };
		OnD = (duration, strength) => { };
		OnE = (duration, strength) => { };
		OnF = (duration, strength) => { };
		OnG = (duration, strength) => { };
		OnH = (duration, strength) => { };
		OnI = (duration, strength) => { };
		OnJ = (duration, strength) => { };
		OnK = (duration, strength) => { };
		OnL = (duration, strength) => { };
		OnM = (duration, strength) => { };
		OnN = (duration, strength) => { };
		OnO = (duration, strength) => { };
		OnP = (duration, strength) => { };
		OnQ = (duration, strength) => { };
		OnR = (duration, strength) => { };
		OnS = (duration, strength) => { };
		OnT = (duration, strength) => { };
		OnU = (duration, strength) => { };
		OnV = (duration, strength) => { };
		OnW = (duration, strength) => { };
		OnX = (duration, strength) => { };
		OnY = (duration, strength) => { };
		OnZ = (duration, strength) => { };

		OnD0 = (duration, strength) => { };
		OnD1 = (duration, strength) => { };
		OnD2 = (duration, strength) => { };
		OnD3 = (duration, strength) => { };
		OnD4 = (duration, strength) => { };
		OnD5 = (duration, strength) => { };
		OnD6 = (duration, strength) => { };
		OnD7 = (duration, strength) => { };
		OnD8 = (duration, strength) => { };
		OnD9 = (duration, strength) => { };

		OnNumPad0 = (duration, strength) => { };
		OnNumPad1 = (duration, strength) => { };
		OnNumPad2 = (duration, strength) => { };
		OnNumPad3 = (duration, strength) => { };
		OnNumPad4 = (duration, strength) => { };
		OnNumPad5 = (duration, strength) => { };
		OnNumPad6 = (duration, strength) => { };
		OnNumPad7 = (duration, strength) => { };
		OnNumPad8 = (duration, strength) => { };
		OnNumPad9 = (duration, strength) => { };
		OnDecimal = (duration, strength) => { };
		OnAdd = (duration, strength) => { };
		OnSubtract = (duration, strength) => { };
		OnMultiply = (duration, strength) => { };
		OnDivide = (duration, strength) => { };

		OnF1 = (duration, strength) => { };
		OnF2 = (duration, strength) => { };
		OnF3 = (duration, strength) => { };
		OnF4 = (duration, strength) => { };
		OnF5 = (duration, strength) => { };
		OnF6 = (duration, strength) => { };
		OnF7 = (duration, strength) => { };
		OnF8 = (duration, strength) => { };
		OnF9 = (duration, strength) => { };
		OnF10 = (duration, strength) => { };
		OnF11 = (duration, strength) => { };
		OnF12 = (duration, strength) => { };
		OnF13 = (duration, strength) => { };
		OnF14 = (duration, strength) => { };
		OnF15 = (duration, strength) => { };
		OnF16 = (duration, strength) => { };
		OnF17 = (duration, strength) => { };
		OnF18 = (duration, strength) => { };
		OnF19 = (duration, strength) => { };
		OnF20 = (duration, strength) => { };
		OnF21 = (duration, strength) => { };
		OnF22 = (duration, strength) => { };
		OnF23 = (duration, strength) => { };
		OnF24 = (duration, strength) => { };

		OnUp = (duration, strength) => { };
		OnDown = (duration, strength) => { };
		OnLeft = (duration, strength) => { };
		OnRight = (duration, strength) => { };
		OnHome = (duration, strength) => { };
		OnEnd = (duration, strength) => { };
		OnPageUp = (duration, strength) => { };
		OnPageDown = (duration, strength) => { };
		OnInsert = (duration, strength) => { };
		OnDelete = (duration, strength) => { };

		OnEscape = (duration, strength) => { };
		OnTab = (duration, strength) => { };
		OnCapsLock = (duration, strength) => { };
		OnBack = (duration, strength) => { };
		OnEnter = (duration, strength) => { };
		OnSpace = (duration, strength) => { };
		OnPrintScreen = (duration, strength) => { };
		OnScroll = (duration, strength) => { };
		OnPause = (duration, strength) => { };

		OnOemtilde = (duration, strength) => { };
		OnOemMinus = (duration, strength) => { };
		OnOemplus = (duration, strength) => { };
		OnOemOpenBrackets = (duration, strength) => { };
		OnOem6 = (duration, strength) => { };
		OnOem5 = (duration, strength) => { };
		OnOem1 = (duration, strength) => { };
		OnOem7 = (duration, strength) => { };
		OnOemcomma = (duration, strength) => { };
		OnOemPeriod = (duration, strength) => { };
		OnOemQuestion = (duration, strength) => { };

		OnApps = (duration, strength) => { };
		OnSleep = (duration, strength) => { };
		OnBrowserBack = (duration, strength) => { };
		OnBrowserForward = (duration, strength) => { };
		OnBrowserRefresh = (duration, strength) => { };
		OnBrowserStop = (duration, strength) => { };
		OnBrowserSearch = (duration, strength) => { };
		OnBrowserFavorites = (duration, strength) => { };
		OnBrowserHome = (duration, strength) => { };
		OnVolumeMute = (duration, strength) => { };
		OnVolumeDown = (duration, strength) => { };
		OnVolumeUp = (duration, strength) => { };
		OnMediaNextTrack = (duration, strength) => { };
		OnMediaPreviousTrack = (duration, strength) => { };
		OnMediaStop = (duration, strength) => { };
		OnMediaPlayPause = (duration, strength) => { };
		OnLaunchMail = (duration, strength) => { };
		OnSelectMedia = (duration, strength) => { };
		OnLaunchApplication1 = (duration, strength) => { };
		OnLaunchApplication2 = (duration, strength) => { };
	}
	/// <summary>The delegate to benchmark KeyPress events.</summary>
	/// <param name="duration">How long was the keypressed for.</param>
	/// <param name="strength">How much strength was applied to the keypress as a normalised float, (FOR JOYSTICK INTEGRATION, do not handle unless).</param>
	public delegate void KeyPressedDelegate(int duration, float strength);
	public static Dictionary<Keys, int> TrueKeyToIndex { get; set; }



	/// <summary>
	/// Sets the corresponding Key to a Bind.
	/// </summary>
	/// <param name="TrueBind">The original value of the KeyBind.</param>
	/// <param name="NewBind">The Surface Bind that will be created.</param>
	/// <returns>If false the corresponding KeyBind isn't supported</returns>
	public static bool ChangeBinding(Keys TrueBind, Keys NewBind){
		if (TrueKeyToIndex.TryGetValue(TrueBind, out int cc)){
			KeyBinds[cc].key = NewBind;
			return true;
		}
		return false;
	}
	/// <summary>
	/// Attaches a function to be called at the Keys calling.
	/// </summary>
	/// <param name="TrueBind"></param>
	/// <param name="function"></param>
	/// <returns></returns>
	public static bool AttachKeyhandle(Keys TrueBind, KeyPressedDelegate function){
		if (TrueKeyToIndex.TryGetValue(TrueBind, out int cc)){
			KeyBinds[cc].AttachableElement += function;
			return true;
		}
		return false;
	}
	public static bool AttachKeyhandles(ControlScheme cS){
		for (int cc = 0; cc < cS.Count; cc++){
			if (TrueKeyToIndex.TryGetValue(cS[cc].key, out int cc_)){
				KeyBinds[cc_].AttachableElement += cS[cc].kD;
			}
		}
		return true;
	}
	/// <summary>
	/// Return the length that <seealso cref="key"/> was held for.
	/// </summary>
	/// <param name="key">The Key that is being found.</param>
	/// <returns>How long the key has been held, in seconds.</returns>
	/// <remarks>This method retrieves all it's data from <see cref="Entry.f.keyBuffer"/></remarks>
	public static async Task<int> GetDuration(Keys key){
		return await Task.Run(() =>{
			int seconds = 0;
			for (int cc = 0; cc < Entry.f.KeyBuffer.Length - 1; cc++){
				if (CompareTo(Entry.f.KeyBuffer[cc].key, key) && CompareTo(Entry.f.KeyBuffer[cc + 1].key, key)){
					seconds++;
					for (int cc_ = cc + 1; cc_ < Entry.f.KeyBuffer.Length; cc_++){
						if (CompareTo(Entry.f.KeyBuffer[cc_ - 1].key, key) && CompareTo(Entry.f.KeyBuffer[cc_].key, key)){
							seconds += Entry.f.KeyBuffer[cc_ - 1].Start.Second - Entry.f.KeyBuffer[cc_ - 1].Start.Second;
						}
					}
				}
			}
			return seconds == 0 ? 1 : seconds;
		});
	}
	/// <summary>Handles the Invoking and handling for Key events directly from <see cref="Entry.f.KeyBuffer"/></summary>
	public static void InvokeKeyHandles(object sender, ElapsedEventArgs e){
		Keys[] keys = new Keys[Entry.f.KeyBuffer.Length];
		int cc = 0;
		foreach((DateTime Start, Keys key) item in Entry.f.KeyBuffer){
			keys[cc] = item.key;
			cc++;
		}
		HandleSpecialKeys(keys);
		InvokeKeyHandles(keys, false);
	}
	/// <summary>Handles the Invoking and Handling of Key events directly from <paramref name="keys"/>.</summary>
	public static void InvokeKeyHandles(Keys[] keys, bool HandleSpecialKeysLocally = true){
		foreach (Keys item in keys){
			if(HandleSpecialKeysLocally){HandleSpecialKeys(keys);}
			InvokeKeyHandle(item);
		}
	}
	/// <summary>Handles the Invoking and handling of Key events directly from <paramref name="keyBind"/>.</summary>
	static void InvokeKeyHandle(Keys keyBind){
		if (TrueKeyToIndex.TryGetValue(keyBind, out int cc)){
			KeyBinds[cc].AttachableElement(GetDuration(keyBind).Result, 1);
			joySticks[cc].HandleKeyPress(KeyBinds[cc].AttachableElement, keyBind);
		}
		return;
	}
	public delegate bool SpecialKeyCombos();
	/// <summary>Contains a database for Special keys that may need to be handled and the Functions to handle them.</summary>
	/// <see cref="Keys.Escape"/>, <see cref="Keys.Space"/>, <see cref="Keys.ControlKey"/> and thier respective combined events.</summary>
	static (Keys[] SpecialKeyCombos, SpecialKeyCombos predicate)[] SpecialKeys = [([Keys.ControlKey, Keys.Space, Keys.J], () => { AttemptAddJoystickHandle();  return true; })];
	/// <summary>Handles the handling of special Keys: <see cref="Keys.Delete"/>, 
	/// <see cref="Keys.Enter"/>, <see cref="Keys.Return"/>, <see cref="Keys.Tab"/>, 
	/// <see cref="Keys.Escape"/>, <see cref="Keys.Space"/>, <see cref="Keys.ControlKey"/> and thier respective combined events.</summary>
	static bool HandleSpecialKeys(Keys[] keys){
		//Search for specific combinations of Keys, e.g"Ctrl + Space + J in series  == AttemptAddJoyStickHandle.
		//A Spen for looking through the List
		Span<Keys> Spankeys = new(keys);
		int Length = Spankeys.Length;
		for (int cc = 0; cc < Length; cc++) {
			int UpperBounds = Length - cc;
			//Store the indexes for the identified special keys.
			Keys[] Buffer = Spankeys.Slice(cc, (cc + 4 > Length ? UpperBounds : 4)).ToArray();
			foreach((Keys[] KeyCombos, SpecialKeyCombos predicate) item in SpecialKeys){
				if(Buffer == item.KeyCombos){ return item.predicate(); }
			}
		}
		return false;
	}



	//!Code for Joystick integration.
	class JoystickChoosingForm : Form{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private IContainer? components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing){
			if (disposing && (components != null)){
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		public Label Tbox;
		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent(){
			SuspendLayout();
			//Text Box
			Tbox = new Label();
			Tbox.Location = new Point(0, 0);
			Tbox.Size = new Size(90, 180);
			//Form
			this.Name = "TheWindow";
			this.Text = "TheWindowText";
			this.components = new Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new Size(900, 900);
			this.Controls.Add(this.Tbox);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
	static List<Joystickhandle> joySticks;
	static Dictionary<int, Keys[]> keyBinds;
	public static void AttemptAddJoystickHandle(){
		
		Guid joyStick = Guid.Empty;
		DirectInput dI = new();
		IList<DeviceInstance> instances = dI.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
		foreach (DeviceInstance instance in instances){
			joyStick = instance.InstanceGuid;
			if (joyStick == Guid.Empty & !joySticks.All(j => j.j.Information.InstanceGuid != joyStick)){
				MessageBox.Show("A new joystick was not found.");
				return;
			}
			int index = joySticks.Count;
			joySticks.Add(new Joystickhandle(new Joystick(dI, joyStick)));
			joySticks[index].j.Properties.AxisMode = DeviceAxisMode.Absolute;
		}
	}
	public static JoystickState[] AcquireAll(){
		JoystickState[] states = new JoystickState[joySticks.Count];
		int cc = 0;
		foreach (Joystickhandle j in joySticks){
			j.j.Acquire();
			j.j.GetCurrentState(ref states[cc]);
			cc++;
		}
		return states;
	}
	public static void PollAll(){
		foreach (Joystickhandle j in joySticks){
			j.j.Poll();
		}
	}
}
/// <summary>
/// This class provides a way for a joyStick to interface with the <see cref="InputController.InvokeKeyHandles()"/>, <see cref="InputController.InvokeKeyHandle(Keys)"/> and <see cref="InputController.InvokeKeyHandles(Keys[])"/> functions.
/// </summary>
class Joystickhandle{
	/// <summary>
	/// This joystick.
	/// </summary>
	public Joystick j { get; private set; }
	/// <summary>Get the number of Buttons associated with this JoyStick.</summary>
	/// <returns>The numbe of buttons associated with this joyStick.</returns>
	public int GetButtonNumber(){ return this.j.GetCurrentState().Buttons.Length; }
	/// <summary>The Keys</summary>
	List<Keys> KeyIndex;
	DateTime[] ButtonStart;
	public Joystickhandle(Joystick j) {
		this.j = j;
		int length = this.j.GetCurrentState().Buttons.Length;
		this.KeyIndex = [];
		this.ButtonStart = new DateTime[length];
		Entry.Update += RecordButtonStart;
	}
	public void RecordButtonStart(){_= RecordButtonStart_();}
	/// <summary>
	/// Record when Buttons when first pressed.
	/// </summary>
	async Task RecordButtonStart_() {
		await Task.Run(() =>{
			j.Poll();
			bool[] buttonsPressed = j.GetCurrentState().Buttons;
			for(int cc =0; cc < buttonsPressed.Length;cc++){
				if(buttonsPressed[cc] && (ButtonStart[cc] == DateTime.MinValue)){ ButtonStart[cc] = DateTime.Now; }else{
					ButtonStart[cc] = DateTime.MinValue;
				}
			}
		});
	}
	/// <summary>
	/// Return the length of a button press.
	/// </summary>
	/// <param name="index">The index of the button.</param>
	/// <returns>the length of the selected button's press, in Milliseconds.</returns>
	int GetButtonPressLength(int index){ return (int)((DateTime.Now - ButtonStart[index]).TotalMilliseconds); }
	/// <summary>Attach Keys to this object so that they can be handled later in <see cref="HandleKeyPress"/></summary>
	/// <param name="truekeys">The Keys to be handled</param>
	/// <returns>Did this function suceed, if not it also sends the index of the <paramref name="truekeys"/> that was problematic</returns>
	public (bool, int) AttachKeyHandles(Keys[] truekeys) {
		int cc = 0;
		foreach (Keys k in truekeys) {
			bool worked = InputController.TrueKeyToIndex.TryGetValue(k, out int index);
			if (worked == false) { return (false, cc); }
			this.KeyIndex.Add(truekeys[cc]);
			cc++;
		}
		return (true, -1);
	}
	/// <summary>Handle a <see cref="InputController.KeyPressedDelegate"/> so that it can function properly with the joyStick, by supplementing it Runtime values</summary>
	/// <param name="delegateToHandle"></param>
	/// <remarks>
	public void HandleKeyPress(InputController.KeyPressedDelegate delegateToHandle, Keys supposedKey){
		JoystickState jS = new();
		j.GetCurrentState(ref jS);
		for(int cc =0; cc < this.GetButtonNumber();cc++){
			if(supposedKey != this.KeyIndex[cc]){ continue; }
			Point p = new(jS.X, jS.Y);
			double normalisednetForce = Math.Sqrt(Math.Pow(p.X - 32767, 2) + Math.Pow(p.Y - 32767, 2))/(32767 * Math.Sqrt(2));
			delegateToHandle(GetButtonPressLength(cc), (float)normalisednetForce);
		}
	}
}