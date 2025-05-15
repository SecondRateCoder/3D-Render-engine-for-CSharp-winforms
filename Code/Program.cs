using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
class Entry{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static CancellationTokenSource Cts{get; private set;}
    public static unsafe WriteableBitmap Buffer;
    public static ElapsedEventHandler TUpdate;
    internal static SynchronizationContext? uiContext;
    public static Action Update;
    public static Action Start;
    public static Form1 f;
    static int stackDepthChecking;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static void Main(){
        Entry.uiContext = SynchronizationContext.Current;
        ApplicationConfiguration.Initialize();
        Initialise();
		StorageManager.filePath = StorageManager.ApplicationPath;
		ExternalControl.Initialise();
        Update += f._Invoke;
        Update += InputController.InvokeKeyHandles;
        TUpdate += (sender, e) => {
            Entry.ActualMemUsage = cProc.WorkingSet64;
            Entry.PeakMemUsage = cProc.PeakWorkingSet64;
            Entry.TotalMemUsage = GC.GetTotalMemory(false);
            stackDepthChecking++;
            if(stackDepthChecking >= 10){
                GetStackDepth();
            }
        };
        Start += () => {
            GC.Collect();
            GetStackDepth();
        };
        gameObj.Create(Vector3.Zero, Vector3.Zero, Polygon.Mesh(5, 5, 0, 4), [(typeof(Texturer), new Texturer(StorageManager.ApplicationPath+@"Cache\Images\GrassBlock.png"))], "Cube");
        _ = Loop();
        //Source of Exception
        Application.Run(f);
    }
    static unsafe void Initialise(){
        Cts = new CancellationTokenSource();
        TUpdate = CollisionManager.Collider;
        Update = BuildSquare;
        Start = ExternalControl.StartTimer;
        Start += ()=>{PeakMemUsage = GC.GetTotalAllocatedBytes();};
        f = new();
        Buffer = new(f.Width, f.Height);
    }
    public static void UpdateUI(Action action){uiContext?.Post(_ => action(), null);}
    static long TotalMemUsage;
    static long PeakMemUsage;
    static long ActualMemUsage;
    /// <summary>
    /// The size of the Stack available to use.
    /// </summary>
    static int stackDepth;
    static int selfDelay;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static Process cProc;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static async Task Loop(){
        float iteration = 0;
        await Task.Run(async () => {
            cProc = Process.GetCurrentProcess();
            if(Entry.Cts == null){Cts = new CancellationTokenSource();}
            while(!Entry.Cts.IsCancellationRequested){
                //Self regulate this function so that it does'nt take up at most 60% of Mem usage.
                MemControl();
                if(Entry.Buffer != null && iteration >= selfDelay/10){UpdateUI(() => f.Invalidate());}
                await Task.Delay(selfDelay, Entry.Cts.Token);
            }
            if(Update != null){Entry.Update();}
            iteration+=.1f;
        });
    }
    static void MemControl(){
        if(ActualMemUsage > TotalMemUsage* .5){selfDelay = Math.Min(selfDelay + 10, 500);}
        if(ActualMemUsage > PeakMemUsage * .6){selfDelay = Math.Min(selfDelay + 100, 500);}
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
        _1.Interval = 1000;
		if(Entry.TUpdate != null){_1.Elapsed += Entry.TUpdate;}
        _1.Elapsed += ElapsedHandler;
    }
    public static void StartTimer(){_1.Start();}
    public static void StopTimer(){fps = 0;		_1.Stop();		_1.Dispose();}
}


public partial class Form1 : Form{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Form1(){
        InitializeComponent();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    override protected void OnLoad(EventArgs e){
        base.OnLoad(e);
        this.buffer = this.Size;
        Entry.Start();
    }
    public void _Invoke(){
        if(this.InvokeRequired){
            this.BeginInvoke(
                () => {
                    if(this.Focused | Debugger.IsAttached){
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
        if(buffer != this.Size){
            ViewPort.MarkPPMatrixDirty();
            buffer = this.Size;
        }
        base.OnPaint(e);
        if(Entry.Buffer != null){e.Graphics.DrawImage((Bitmap)Entry.Buffer, Point.Empty);}
    }
    public (DateTime Start, Keys key)[] keyBuffer{get; private set;} = new (DateTime Start, Keys key)[20];
    int Position = 0;
    void AddKeyValue(Keys key){
        Position++;
        if(Position >= 20){InputController.InvokeKeyHandles();   Position = 0;}
        keyBuffer[Position] = (DateTime.Now, key);
    }
    protected override void OnKeyDown(KeyEventArgs e){
        base.OnKeyDown(e);
        lock(keyBuffer){
            AddKeyValue(e.KeyCode);
        }
    }
    protected override void OnKeyUp(KeyEventArgs e){
        base.OnKeyUp(e);
        lock(keyBuffer){
            AddKeyValue(e.KeyCode);
        }
    }
    protected override void OnKeyPress(KeyPressEventArgs e){
        base.OnKeyPress(e);
    }
}