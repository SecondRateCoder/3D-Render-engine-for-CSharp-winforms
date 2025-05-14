using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Concurrent;
class Entry{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static CancellationTokenSource Cts{get; private set;}
    public static unsafe WriteableBitmap Buffer;
    public static ElapsedEventHandler TUpdate;
    internal static SynchronizationContext? uiContext;
    public static Action Update;
    public static Action Start;
    public static Form1 f;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static void Main(){
        Entry.uiContext = SynchronizationContext.Current;
        ApplicationConfiguration.Initialize();
        Initialise();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        Update += f._Invoke;
        TUpdate += (sender, e) => {
            Entry.ActualMemUsage = cProc.WorkingSet64;
            Entry.PeakMemUsage = cProc.PeakWorkingSet64;
            Entry.TotalMemUsage = GC.GetTotalAllocatedBytes(true);
        };
        Loop();
        Application.Run(f);
        gameObj.Create(Vector3.Zero, Vector3.Zero, Polygon.Mesh(5, 5, 0, 4), [(typeof(Texturer), new Texturer(AppDomain.CurrentDomain.BaseDirectory+@"Cache\Images\GrassBlock.png"))], "Cube");
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
    static int selfDelay;
    static Process cProc;
    public static async void Loop(){
        await Task.Run(() => {
            cProc = Process.GetCurrentProcess();
            if(Entry.Cts == null){Cts = new CancellationTokenSource();}
            while(!Entry.Cts.IsCancellationRequested){
                //Self regulate this function so that it does'nt take up at most 60% of Mem usage.
                if(ActualMemUsage > TotalMemUsage* .5){selfDelay += 10;}
                if(ActualMemUsage > PeakMemUsage * .6){selfDelay += 100;}
                GC.Collect();
                UpdateUI(() => f.Refresh());
                if(Entry.Buffer != null){f.Invalidate();}
                Task.Delay(selfDelay);
            }
            if(Update != null){Entry.Update();}
            GC.Collect();
        });
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
        for(int cc =0; cc < values.Length-1; cc++){
            List<Point> Buffer = [.. ViewPort.DrawBLine(values[cc].p, values[cc+1].p)];
            if(!int.IsEvenInteger(Buffer.Count)){Buffer.RemoveAt(Buffer.Count-1);}
            Entry.Buffer.Initialise(new TextureDatabase(Buffer, Color.White), formWidth, formHeight);
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
        Entry.Buffer?.Dispose();
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
}