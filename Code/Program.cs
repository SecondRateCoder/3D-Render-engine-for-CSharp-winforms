using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
class Entry{
    public static CancellationTokenSource cts{get; private set;}
    public static ElapsedEventHandler TUpdate;
    public static Action Update;
    public static Action Start;
    static WriteableBitmap Buffer;
    public static Form1 f;
    public static void Main(){
        ApplicationConfiguration.Initialize();
        Initialise();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        Update += () => {
            if(f.InvokeRequired){
                f.Invoke(() => {
                    if(f != null && (f.Focused | Debugger.IsAttached)){
                        f.Refresh();
                        if(Buffer != null){f.G.DrawImage((Bitmap)Buffer, Point.Empty);}
                    }
                });
            }
        };
        Loop();
        Application.Run(f);
    }
    static void Initialise(){
        cts = new CancellationTokenSource();
        TUpdate = CollisionManager.Collider;
        Update = Paint3D;
        Start = ExternalControl.StartTimer;
        f = new();
        Buffer = new(f.Width, f.Height);
    }
    public static async void Loop(){
        await Task.Run(() => {
            if(Entry.cts == null){cts = new CancellationTokenSource(360000);}
            while(!Entry.cts.IsCancellationRequested){
                if(Update != null){Entry.Update();}
            }
        });
    }

    //Paint the enviroment.
    static void Paint3D(){
        try{f.Name = $"TheWindowText, fps: {ExternalControl.fps}, Cancel? : {Entry.cts.IsCancellationRequested}";}
        catch(NullReferenceException){f.Name = $"TheWindowText, fps: {0}";}
        int formHeight;
        int formWidth;
        formWidth = f.Width;
        formHeight = f.Height;
        (Point p, Color color)[] values = [
                (new Point((int)(0.25 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.9 * formHeight)), Color.Black), 
                (new Point((int)(0.25 * formWidth), (int)(0.9 * formHeight)), Color.Black), 
            ];
        for(int cc =0; cc < values.Length-1; cc++){
            List<Point> Buffer = [.. ViewPort.DrawBLine(values[cc].p, values[cc+1].p)];
            if(!int.IsEvenInteger(Buffer.Count)){Buffer.RemoveAt(Buffer.Count-1);}
            Entry.Buffer.Initialise(new TextureDatabase(Buffer), formWidth, formHeight);
        }
        
        
    }
}
static class ExternalControl{
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
    Graphics GrphcsPrprty;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Graphics G {get{return GrphcsPrprty == null? this.CreateGraphics() : GrphcsPrprty;} private set{GrphcsPrprty = value;}}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Form1(){
        InitializeComponent();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    override protected void OnLoad(EventArgs e){
        base.OnLoad(e);
        Entry.Start();
    }
    protected override void OnClosing(CancelEventArgs e){
        base.OnClosing(e);
        Entry.cts.Cancel();
        ExternalControl.StopTimer();
    }

    protected override void OnPaint(PaintEventArgs e){
        base.OnPaint(e);
        G = e.Graphics;
    }
}