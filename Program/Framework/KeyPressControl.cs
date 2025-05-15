using SharpDX.DirectInput;


/// <summary>
/// Handles the control, calling and attaching of functions to Key calls.
/// </summary>
/// <remarks>For parameters named key; the TrueBind is needed and 
/// for Parameters named keyBind; the surface key bind is needed</remarks>
static class InputController{
    public delegate void KeyPressedDelegate(int duration);
    static void OnA(int duration) { /* handle A key */ }
    static void OnB(int duration) { /* handle B key */ }
    static void OnC(int duration) { /* handle C key */ }
    static void OnD(int duration) { /* handle D key */ }
    static void OnE(int duration) { /* handle E key */ }
    static void OnF(int duration) { /* handle F key */ }
    static void OnG(int duration) { /* handle G key */ }
    static void OnH(int duration) { /* handle H key */ }
    static void OnI(int duration) { /* handle I key */ }
    static void OnJ(int duration) { /* handle J key */ }
    static void OnK(int duration) { /* handle K key */ }
    static void OnL(int duration) { /* handle L key */ }
    static void OnM(int duration) { /* handle M key */ }
    static void OnN(int duration) { /* handle N key */ }
    static void OnO(int duration) { /* handle O key */ }
    static void OnP(int duration) { /* handle P key */ }
    static void OnQ(int duration) { /* handle Q key */ }
    static void OnR(int duration) { /* handle R key */ }
    static void OnS(int duration) { /* handle S key */ }
    static void OnT(int duration) { /* handle T key */ }
    static void OnU(int duration) { /* handle U key */ }
    static void OnV(int duration) { /* handle V key */ }
    static void OnW(int duration) { /* handle W key */ }
    static void OnX(int duration) { /* handle X key */ }
    static void OnY(int duration) { /* handle Y key */ }
    static void OnZ(int duration) { /* handle Z key */ }

    // Numbers (top row)
    static void OnD0(int duration) { /* handle 0 key */ }
    static void OnD1(int duration) { /* handle 1 key */ }
    static void OnD2(int duration) { /* handle 2 key */ }
    static void OnD3(int duration) { /* handle 3 key */ }
    static void OnD4(int duration) { /* handle 4 key */ }
    static void OnD5(int duration) { /* handle 5 key */ }
    static void OnD6(int duration) { /* handle 6 key */ }
    static void OnD7(int duration) { /* handle 7 key */ }
    static void OnD8(int duration) { /* handle 8 key */ }
    static void OnD9(int duration) { /* handle 9 key */ }

    // Numpad
    static void OnNumPad0(int duration) { /* handle NumPad0 key */ }
    static void OnNumPad1(int duration) { /* handle NumPad1 key */ }
    static void OnNumPad2(int duration) { /* handle NumPad2 key */ }
    static void OnNumPad3(int duration) { /* handle NumPad3 key */ }
    static void OnNumPad4(int duration) { /* handle NumPad4 key */ }
    static void OnNumPad5(int duration) { /* handle NumPad5 key */ }
    static void OnNumPad6(int duration) { /* handle NumPad6 key */ }
    static void OnNumPad7(int duration) { /* handle NumPad7 key */ }
    static void OnNumPad8(int duration) { /* handle NumPad8 key */ }
    static void OnNumPad9(int duration) { /* handle NumPad9 key */ }
    static void OnDecimal(int duration) { /* handle Decimal key */ }
    static void OnAdd(int duration) { /* handle Add key */ }
    static void OnSubtract(int duration) { /* handle Subtract key */ }
    static void OnMultiply(int duration) { /* handle Multiply key */ }
    static void OnDivide(int duration) { /* handle Divide key */ }

    // Function keys
    static void OnF1(int duration) { /* handle F1 key */ }
    static void OnF2(int duration) { /* handle F2 key */ }
    static void OnF3(int duration) { /* handle F3 key */ }
    static void OnF4(int duration) { /* handle F4 key */ }
    static void OnF5(int duration) { /* handle F5 key */ }
    static void OnF6(int duration) { /* handle F6 key */ }
    static void OnF7(int duration) { /* handle F7 key */ }
    static void OnF8(int duration) { /* handle F8 key */ }
    static void OnF9(int duration) { /* handle F9 key */ }
    static void OnF10(int duration) { /* handle F10 key */ }
    static void OnF11(int duration) { /* handle F11 key */ }
    static void OnF12(int duration) { /* handle F12 key */ }
    static void OnF13(int duration) { /* handle F13 key */ }
    static void OnF14(int duration) { /* handle F14 key */ }
    static void OnF15(int duration) { /* handle F15 key */ }
    static void OnF16(int duration) { /* handle F16 key */ }
    static void OnF17(int duration) { /* handle F17 key */ }
    static void OnF18(int duration) { /* handle F18 key */ }
    static void OnF19(int duration) { /* handle F19 key */ }
    static void OnF20(int duration) { /* handle F20 key */ }
    static void OnF21(int duration) { /* handle F21 key */ }
    static void OnF22(int duration) { /* handle F22 key */ }
    static void OnF23(int duration) { /* handle F23 key */ }
    static void OnF24(int duration) { /* handle F24 key */ }

    // Navigation keys
    static void OnUp(int duration) { /* handle Up key */ }
    static void OnDown(int duration) { /* handle Down key */ }
    static void OnLeft(int duration) { /* handle Left key */ }
    static void OnRight(int duration) { /* handle Right key */ }
    static void OnHome(int duration) { /* handle Home key */ }
    static void OnEnd(int duration) { /* handle End key */ }
    static void OnPageUp(int duration) { /* handle PageUp key */ }
    static void OnPageDown(int duration) { /* handle PageDown key */ }
    static void OnInsert(int duration) { /* handle Insert key */ }
    static void OnDelete(int duration) { /* handle Delete key */ }

    // Other common keys
    static void OnEscape(int duration) { /* handle Escape key */ }
    static void OnTab(int duration) { /* handle Tab key */ }
    static void OnCapsLock(int duration) { /* handle CapsLock key */ }
    static void OnBack(int duration) { /* handle Back key */ }
    static void OnEnter(int duration) { /* handle Enter key */ }
    static void OnSpace(int duration) { /* handle Space key */ }
    static void OnPrintScreen(int duration) { /* handle PrintScreen key */ }
    static void OnScroll(int duration) { /* handle Scroll key */ }
    static void OnPause(int duration) { /* handle Pause key */ }

    // Punctuation and symbols
    static void OnOemtilde(int duration) { /* handle Oemtilde key */ }
    static void OnOemMinus(int duration) { /* handle OemMinus key */ }
    static void OnOemplus(int duration) { /* handle Oemplus key */ }
    static void OnOemOpenBrackets(int duration) { /* handle OemOpenBrackets key */ }
    static void OnOem6(int duration) { /* handle Oem6 key */ }
    static void OnOem5(int duration) { /* handle Oem5 key */ }
    static void OnOem1(int duration) { /* handle Oem1 key */ }
    static void OnOem7(int duration) { /* handle Oem7 key */ }
    static void OnOemcomma(int duration) { /* handle Oemcomma key */ }
    static void OnOemPeriod(int duration) { /* handle OemPeriod key */ }
    static void OnOemQuestion(int duration) { /* handle OemQuestion key */ }

    // Application and browser keys
    static void OnApps(int duration) { /* handle Apps key */ }
    static void OnSleep(int duration) { /* handle Sleep key */ }
    static void OnBrowserBack(int duration) { /* handle BrowserBack key */ }
    static void OnBrowserForward(int duration) { /* handle BrowserForward key */ }
    static void OnBrowserRefresh(int duration) { /* handle BrowserRefresh key */ }
    static void OnBrowserStop(int duration) { /* handle BrowserStop key */ }
    static void OnBrowserSearch(int duration) { /* handle BrowserSearch key */ }
    static void OnBrowserFavorites(int duration) { /* handle BrowserFavorites key */ }
    static void OnBrowserHome(int duration) { /* handle BrowserHome key */ }
    static void OnVolumeMute(int duration) { /* handle VolumeMute key */ }
    static void OnVolumeDown(int duration) { /* handle VolumeDown key */ }
    static void OnVolumeUp(int duration) { /* handle VolumeUp key */ }
    static void OnMediaNextTrack(int duration) { /* handle MediaNextTrack key */ }
    static void OnMediaPreviousTrack(int duration) { /* handle MediaPreviousTrack key */ }
    static void OnMediaStop(int duration) { /* handle MediaStop key */ }
    static void OnMediaPlayPause(int duration) { /* handle MediaPlayPause key */ }
    static void OnLaunchMail(int duration) { /* handle LaunchMail key */ }
    static void OnSelectMedia(int duration) { /* handle SelectMedia key */ }
    static void OnLaunchApplication1(int duration) { /* handle LaunchApplication1 key */ }
    static void OnLaunchApplication2(int duration) { /* handle LaunchApplication2 key */ }
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
    static Dictionary<Keys, Keys> SurfaceKeyToTrueBind{get; set;}
    static Dictionary<Keys, int> TrueKeyToIndex{get; set;}


    static InputController(){
        TrueKeyToIndex = [];
        SurfaceKeyToTrueBind = [];
        int cc =0;
        foreach((Keys key, Keys TrueKey, KeyPressedDelegate AttachableElement) item in KeyBinds){
            TrueKeyToIndex[item.TrueKey] = cc;
            SurfaceKeyToTrueBind[item.key] = item.TrueKey;
            cc++;
        }
    }


/// <summary>
/// Sets the corresponding Key to a Bind.
/// </summary>
/// <param name="TrueBind">The original value of the KeyBind.</param>
/// <param name="NewBind">The Surface Bind that will be created.</param>
/// <returns>If false the corresponding KeyBind isn't supported</returns>
    public static bool ChangeBinding(Keys TrueBind, Keys NewBind){
        if(TrueKeyToIndex.TryGetValue(TrueBind, out int cc)){
            KeyBinds[cc].key = NewBind;
            SurfaceKeyToTrueBind[KeyBinds[cc].TrueKey] = NewBind;
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
        if(TrueKeyToIndex.TryGetValue(TrueBind, out int cc)){
            KeyBinds[cc].AttachableElement += function;
            return true;
        }
        return false;
    }
    public static bool AttachKeyhandles(ControlScheme cS){
        if(cS.Count != cS.Count){throw new ArgumentOutOfRangeException();}
        for(int cc =0; cc < cS.Count;cc++){
            if(TrueKeyToIndex.TryGetValue(cS[cc].key, out int cc_)){
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
        return await Task.Run(() => {
            int seconds = 0;
            for(int cc =0; cc < Entry.f.keyBuffer.Length-1;cc++){
                if(Entry.f.keyBuffer[cc].key == key && Entry.f.keyBuffer[cc+1].key == key){
                    seconds++;
                    for(int cc_ =cc+1; cc_< Entry.f.keyBuffer.Length;cc_++){
                        if(Entry.f.keyBuffer[cc_-1].key == key && Entry.f.keyBuffer[cc_].key == key){
                            seconds += Entry.f.keyBuffer[cc_-1].Start.Second - Entry.f.keyBuffer[cc_-1].Start.Second;
                        }
                    }
                }
            }
            return seconds == 0? 1: seconds;
        });
    }
    public static void InvokeKeyHandles(){
        Keys[] keys = new Keys[Entry.f.keyBuffer.Length];
        int cc =0;
        foreach((DateTime Start, Keys key) item in Entry.f.keyBuffer){
            keys[cc] = item.key;
            cc++;
        }
        InvokeKeyHandles();
    }
    public static void InvokeKeyHandles(Keys[] keys){
        foreach(Keys item in keys){
            InvokeKeyHandle(item);
        }
    }
    static void InvokeKeyHandle(Keys keyBind){
        if(TrueKeyToIndex.TryGetValue(keyBind, out int cc)){
            KeyBinds[cc].AttachableElement(GetDuration(keyBind).Result);
        }
        return;
    }
    class ExternalController{
        Joystick rudder;
        JoystickState state;
        bool isAcquired = false;
        public ExternalController(){
            Joystick? rudder = Create();
            this.rudder = rudder ?? throw new TypeInitializationException(nameof(ExternalController), new ArgumentOutOfRangeException(null, "The next Device was not found"));
            rudder.Unacquire();
            Entry.Update += (() => {
                this.rudder.GetCurrentState(ref this.state);
            });
        }
        public bool AcquireDevice(){
            if(rudder != null && !isAcquired){
                rudder.Acquire();
                isAcquired = true;
                MessageBox.Show("Device acquired.");
                return true;
            }
            return false;
        }
        public void ReleaseDevice(){
            if (rudder != null && isAcquired) { 
                rudder.Unacquire();
                isAcquired = false;
                MessageBox.Show("Device Released.");
            }
        }
/// <summary>
/// Attach new KeyBinds to the keys corresponding to <see cref="TrueKey"/>, using <see cref="InputController.KeyBinds"/> to find the correspoding value for the index.
/// </summary>
/// <param name="TrueKeys">The original keys that the new Bind should be applied to.</param>
/// <param name="myButtons">The indexes of <see cref="this.state.Buttons"/> that this should apply to</param>
/// <returns>Did this function end properly?</returns>
/// <exception cref="TypeInitializationException">If <see cref="TrueKey"/> and <see cref="myButtons"/> don't have the same Length, a <see cref="TypeInitialisationException"/> will be thrown</exception>
        public bool AttachKeyBinds(Keys[] TrueKeys, int[] myButtons){
            if(TrueKeys.Length != myButtons.Length){throw new TypeInitializationException(nameof(ExternalController), new ArgumentOutOfRangeException(null, "TrueKeys and myButtons must have the same length"));}
            int Length = TrueKeys.Length;
            for(int cc =0; cc < Length;cc++){
                if(this.state.Buttons[cc] == true){InputController.ChangeBinding(TrueKeys[cc], InputController.KeyBinds[myButtons[cc]].TrueKey);}
            }
            return true;
        }


        static Joystick? Create(){
            DirectInput directInput = new();
            // Find a Gamepad Guid
            Guid joystickGuid = Guid.Empty;
            DeviceInstance deviceInstance = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly)!.First();
            joystickGuid = deviceInstance.InstanceGuid;
            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty){
                deviceInstance = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly).First();
                joystickGuid = deviceInstance.InstanceGuid;
            }
            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty){
                MessageBox.Show("No Joystick or Gamepad found.");
                return null;
            }
            // Instantiate the joystick
            Joystick joystick = new Joystick(directInput, joystickGuid);
            MessageBox.Show($"Found Joystick/Gamepad with GUID: {joystickGuid.ToByteArray().Length}");
            joystick.Properties.AxisMode = DeviceAxisMode.Absolute;
            joystick.SetCooperativeLevel(0, CooperativeLevel.Exclusive | CooperativeLevel.Background);
            joystick.Properties.BufferSize = 128;
            // Poll events from joystick
            Entry.Update += (() =>{
                joystick.Poll();
                _ = joystick.GetBufferedData();
            });
            return joystick;
        }
    }
}