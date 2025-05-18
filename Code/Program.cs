using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NUnit.Framework;
static class Entry{
    /// <summary>The maximum amount of time that <see cref="Entry.Loop"/> is allowed to delay itself by, 
    /// this is needed because <see cref="Entry.Loop"/> regulates itself by delaying itself between each Loop.</summary>
    /// <remarks>In Milliseconds.</remarks>
    static int MaxDelay;
    /// <summary>Defines the Upper percentage of the <see cref="Entry.TotalMemUsage"/> where if <see cref="Entry.Loop"/>'s Memory usage falls below the value
    /// , it's Regulation becomes cautious.</summary>
    public static float UpperLimit { get { return _uLimit; } set { _uLimit = Math.Clamp(value, 0, 1); } }
    static float _uLimit;
    /// <summary>Defines the Utmost percentage bounds of <see cref="Entry.TotalMemUsage"/> that <see cref="Entry.Loop"/> is allowed to reach, 
    /// Beyond this point Regulation becomes aggressive.</summary>
    public static float GreatestLimit { get { return _gLimit - .1f < 0? _gLimit: _gLimit - .1f; } set { _gLimit = Math.Clamp(value, 0, 1); } }
    static float _gLimit;
    /// <summary>Defines the Lower percentage bounds of <see cref="Entry.TotalMemUsage"/> where if <see cref="Entry.Loop"/>'s Memory usage falls below the value, 
    /// it's Regulation becomes lax.</summary>
    public static float LowerLimit { get { return _lLimit; } set { _lLimit = Math.Clamp(value, 0, 1); } }
    static float _lLimit;
    /// <summary>The public <see cref="CancellationTokenSource"/> that the render loop is architectured around, use <see cref="CancellationTokenSource.Cancel()"/> to stop the Rendering loop.</summary>
    /// <remarks>Cancelling the Rendering Loop does not end the Program, instead stops the handling of Rendering, all other functions like <see cref="CollisionManager.HandleCollisions(object?, ElapsedEventArgs)"/> runs on <see cref="TUpdate"/></remarks>
    public static CancellationTokenSource Cts { get; private set; }
    /// <summary>The context where <see cref="Entry.f"/>'s UI thread lies.</summary>
    internal static SynchronizationContext? uiContext;
    /// <summary>This delegate is called 60 times a second, it controls the calling of arbitrary functions e.g: <see cref="HandleMemUsage"/>, <see cref="GetStackDepth"/></summary>
    public static ElapsedEventHandler TUpdate;
    /// <summary>This delegate runs as many times as possible, it self-regulates to prevent a <see cref="StackOverflowException"/> and the termination of the Program;</summary>
    public static Action Update;
    /// <summary>The delegate called at <see cref="Entry.f"/>'s creation.</summary>
    public static Action Start;
    public static Form1 f;
    /// <summary>The delay that should be between each <see cref="HandleMemUsage"/> call.</summary>
    static int MemCheckRate;
    /// <summary>An estimation of how much Memory has been assigned to <see cref ="Entry.Loop"/>.</summary>
    static long TotalMemUsage;
    /// <summary>An estimation of how many bytes have been used by <see cref ="Entry.Loop"/> overall.</summary>
    static long PeakMemUsage;
    /// <summary>An estimation of the number of bytes used by <see cref ="Entry.Loop"/> at the moment.</summary>
    static long ActualMemUsage;
    /// <summary>The estimation of the Stack depth.</summary>
    static int stackDepth;
    static int selfDelay;
    static Process cProc;
    /// <summary>The Buffer to allow GraphicsData from <see cref="Entry.BuildWorld"/> and <see cref="BuildSquare"/> to be communicated with <see cref="Entry.f"/>'s UI.</summary>
    public static WriteableBitmap Buffer;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static Entry(){
        try{ExtensionHandler.PreLoadExtensions();}catch{MessageBox.Show("Extensions could not be Pre-Loaded", "ExtensionPre_LoadingException: Extensions could not be loaded.", MessageBoxButtons.OK, MessageBoxIcon.Warning);}
        ApplicationConfiguration.Initialize();
        f = new();
        Cts = new CancellationTokenSource();
        CollisionManager.ColliderCheckTime = 5000;
        TUpdate = CollisionManager.HandleCollisions;
        Start = ExternalControl.StartTimer;
        Buffer = new(f.Width, f.Height);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static void Main(){
        Update += f._Invoke;
        Application.Run(f);
        Entry.uiContext = SynchronizationContext.Current;
		ExternalControl.Initialise();
        TUpdate += (sender, e) => {
            Entry.ActualMemUsage = cProc.WorkingSet64;
            Entry.PeakMemUsage = cProc.PeakWorkingSet64;
            Entry.TotalMemUsage = GC.GetTotalMemory(false);
            MemCheckRate++;
            if(MemCheckRate >= 10){
                GetStackDepth();
            }
        };
        Start += (() => {
            GC.Collect(-1, GCCollectionMode.Aggressive);
            GetStackDepth();
        });
        TestKey();
        gameObj.Create(Vector3.Zero, Vector3.Zero, Polygon.Mesh(5, 5, 0, 4), [(typeof(Texturer), new Texturer(StorageManager.ApplicationPath+@"Cache\Images\GrassBlock.png"))], "Cube");
        _ = Loop();
        //Source of Exception
    }
    public static async Task Loop(){
        float Runs = 0;
        await Task.Run(async () => {
            cProc = Process.GetCurrentProcess();
            if(Entry.Cts == null){Cts = new CancellationTokenSource();}
            while(!Entry.Cts.IsCancellationRequested){
                //Self regulate this function so that it does'nt take up at most 60% of Mem usage.
                HandleMemUsage();
                BuildSquare();
                if (Entry.Buffer != null && Runs >= selfDelay / 10) { UpdateUI(() => f.Invalidate()); }
                await Task.Delay(selfDelay, Entry.Cts.Token);
            }
            if(Update != null){Entry.Update();}
            Runs+=.1f;
        });
    }
    [Test]
    static void HandleMemUsage(){
        if(ActualMemUsage > ((TotalMemUsage + PeakMemUsage)/2) * Entry._uLimit){selfDelay = Math.Min(selfDelay + 10, Entry.MaxDelay);}
        if(ActualMemUsage > ((TotalMemUsage + PeakMemUsage)/2) * Entry._gLimit){selfDelay = Math.Min(selfDelay + 50, Entry.MaxDelay);}
        if(ActualMemUsage < (TotalMemUsage + PeakMemUsage)/2* .5 && selfDelay > 10){selfDelay -= 10;}
        if(stackDepth > 1000){
            selfDelay += 200;
        }else{
            selfDelay -= 10;
        }
    }
    static void GetStackDepth(){
        try{
            while(true){
                stackDepth++;
                AllocateStackSpace(stackDepth);
            }
        }catch(InsufficientExecutionStackException){
            return;
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void AllocateStackSpace(int depth){
        Span<byte> stackSpace = stackalloc byte[255];
        stackSpace[0] = (byte)depth;
    }
    [Test]
    static void TestKey(){
        EncryptionKey k = new([20, 3, 45, 6, 6], 
        [(byte)20, (byte)25, (byte)10, (byte)15, (byte)0, (byte)5, (byte)20, (byte)25, (byte)10, (byte)15]);
        MessageBox.Show(k.key_.ToString());
        int[] array = EncryptionKey.DecodeKey(k.key_, [(byte)20, (byte)25, (byte)10, (byte)15, (byte)0, (byte)5, (byte)20, (byte)25, (byte)10, (byte)15], 2);
        MessageBox.Show($"{CustomSort.ToString(array)}");
    }
    static void UpdateUI(Action action){uiContext?.Post(_ => action(), null);}
    //Paint the enviroment.
    static void BuildSquare(){
        try{f.Name = $"TheWindowText, fps: {ExternalControl.fps}, Cancel? : {Entry.Cts.IsCancellationRequested}";}
        catch(NullReferenceException){f.Name = $"TheWindowText, fps: {0}";}
        int formHeight;
        int formWidth;
        formWidth = f.Width;
        formHeight = f.Height;
        (Point p, Color color)[] values = {
                (new Point((int)(0.25 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.9 * formHeight)), Color.Black), 
                (new Point((int)(0.25 * formWidth), (int)(0.9 * formHeight)), Color.Black), 
            };
        Point[] Buffer = [.. ViewPort.DrawBLine(values[0].p, values[1].p)];
        Span<Point> Buffer_ = new(Buffer);
        for(int cc =2; cc < values.Length-1; cc++){
            if(!int.IsEvenInteger(Buffer_.Length)){Buffer_[Buffer_.Length-1] = Point.Empty;}
            Entry.Buffer.Set(new TextureDatabase(Buffer, Color.White), 255);
        }
    }
    static void BuildWorld(){
        try{f.Name = $"TheWindowText, fps: {ExternalControl.fps}, Cancel? : {Entry.Cts.IsCancellationRequested}";}
        catch(NullReferenceException){f.Name = $"TheWindowText, fps: {0}";}
        int formHeight = f.Height;
        int formWidth = f.Width;
        Entry.Buffer = ViewPort.Convert(f.Width, f.Height);
    }
}
static class ExternalControl{
    public static int deltaTime{get{return 1/fps;}}
    public static int fps{get; private set;}
    static readonly ElapsedEventHandler ElapsedHandler = (sender, e) => {
        fps = 0;
        if(Entry.TUpdate != null && _1 != null){_1.Elapsed += Entry.TUpdate;}
    };
    static void IncrementFrames(){fps++;}
    public static Timer _1 = new Timer();
    public static void Initialise(){
        _1.AutoReset = true;
        _1.Interval = 167;
		if(Entry.TUpdate != null){_1.Elapsed += Entry.TUpdate;}
        _1.Elapsed += ElapsedHandler;
    }
    public static void StartTimer(){_1.Start();}
    public static void StopTimer(){fps = 0;		_1.Stop();		_1.Dispose();}
}


partial class Form1 : Form{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Form1(){
        InitializeComponent();
        this.EnterFullScreenMode();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    override protected void OnLoad(EventArgs e){
        base.OnLoad(e);
        this.buffer = this.Size;
        Entry.Start();
    }
    public void _Invoke(){
        if (this.InvokeRequired){
            this.BeginInvoke(() =>{
                    if (this.Focused | Debugger.IsAttached){
                        this.Refresh();
                    }
                });
        }
    }
    protected override void OnClosing(CancelEventArgs e){
        base.OnClosing(e);
        Entry.Cts?.Cancel();
        ExternalControl.StopTimer();
        GC.SuppressFinalize(Entry.Buffer);
    }
    Size buffer;
    protected override void OnPaint(PaintEventArgs e){
        if (buffer != this.Size){
            ViewPort.MarkPPMatrixDirty();
            buffer = this.Size;
        }
        base.OnPaint(e);
        if (Entry.Buffer != null) { e.Graphics.DrawImage((Bitmap)Entry.Buffer, Point.Empty); }
    }
    public (DateTime Start, InputController.Keys key)[] KeyBuffer { get; private set; } = new (DateTime Start, InputController.Keys key)[20];
    int Position = 0;
    void AddKeyValue(Keys key){
        lock (KeyBuffer){
            Position++;
            if (Position >= 20) {Position = 0; }
            KeyBuffer[Position] = (DateTime.Now, InputController.ToInputCotrollerKeys(key));
        }
    }
    protected override void OnKeyDown(KeyEventArgs e){
        base.OnKeyDown(e);
        AddKeyValue(e.KeyCode);
    }
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        switch (e.KeyChar)
        {
            case '\b':
                AddKeyValue(Keys.Delete);
                return;
            case '\n':
                AddKeyValue(Keys.Enter);
                return;
            case '\r':
                AddKeyValue(Keys.Return);
                return;
            case '\t':
                AddKeyValue(Keys.Tab);
                return;
            case (char)27:
                AddKeyValue(Keys.Escape);
                return;
            case (char)Keys.Space:
                AddKeyValue(Keys.Space);
                return;
            case (char)Keys.ControlKey:
                AddKeyValue(Keys.ControlKey);
                return;
        }
        e.Handled = true;
    }
    public bool isFullScreen{ get; private set;}
    public void ToggleFullScreen(){if(isFullScreen){ this.LeaveFullScreenMode(); }else{ this.EnterFullScreenMode();}}
    public void EnterFullScreenMode(){
        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.isFullScreen = true;
    }

    public void LeaveFullScreenMode()
    {
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.WindowState = FormWindowState.Normal;
        this.isFullScreen = false;
    }
}