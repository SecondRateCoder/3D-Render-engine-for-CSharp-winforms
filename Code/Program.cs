using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
class Entry{
    public static CancellationTokenSource cts{get; private set;}
    public static ElapsedEventHandler TUpdate;
    public static Action Update;
    public static Action Start;
    public static unsafe WriteableBitmap Buffer;
    public static Form1 f;
    public static void Main(){
        ApplicationConfiguration.Initialize();
        Initialise();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        Update += f._Invoke;
        Loop();
        Application.Run(f);
    }
    static SynchronizationContext uiContext;
    static unsafe void Initialise(){
        cts = new CancellationTokenSource();
        TUpdate = CollisionManager.Collider;
        Update = Paint3D;
        Start = ExternalControl.StartTimer;
        f = new();
        Buffer = new(f.Width, f.Height);
        uiContext = SynchronizationContext.Current;
    }
    public static void UpdateUI(Action action){
        uiContext?.Post(_ => action(), null);
    }
    public static async void Loop(){
        await Task.Run(() => {
            if(Entry.cts == null){cts = new CancellationTokenSource(360000);}
            while(!Entry.cts.IsCancellationRequested){
                    UpdateUI(() => f.Refresh());
                    if(Entry.Buffer != null){f.Invalidate();}
                }
                if(Update != null){Entry.Update();}
        }).ContinueWith(t => {
            f.Refresh();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    //Paint the enviroment.
    static void Paint3D(){
        try{f.Name = $"TheWindowText, fps: {ExternalControl.fps}, Cancel? : {Entry.cts.IsCancellationRequested}";}
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
}
static class ExternalControl{
    public static int deltaTime{get{return 1/fps;}}
    public static int fps{get; private set;}
    static ElapsedEventHandler ElapsedHandler = (sender, e) => {
        fps = 0;
        if(Entry.TUpdate != null){_1.Elapsed += Entry.TUpdate;}
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
    public void SetFormTitle(string title){
        if(this.InvokeRequired){
            this.BeginInvoke(new Action(() => this.Text = title));
        } else {
            this.Text = title;
        }
    }
    protected override void OnClosing(CancelEventArgs e){
        base.OnClosing(e);
        Entry.cts?.Cancel();
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