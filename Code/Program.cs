using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
class Entry{
    public static CancellationTokenSource cts{get; private set;}
    public static ElapsedEventHandler TUpdate;
    public static Action Update;
    public static Action Start;
    static Pen def;
    static event Action RefreshyForm = () => {f.Refresh();};
    static Thread T;
    static Form1 f = new Form1();
    public static void Main(){
        ApplicationConfiguration.Initialize();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        cts = new CancellationTokenSource();
		Start = ExternalControl.StartTimer;
		Update = Paint3D;
        T = Thread.CurrentThread;
        Application.Run(f);
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
        List<Point> points = [];
        for(int cc =0; cc < values.Length-1; cc++){
            def = new Pen(values[cc].color, 5);
            List<Point> Buffer = [.. ViewPort.DrawBLine(values[cc].p, values[cc+1].p)];
            if(!int.IsEvenInteger(Buffer.Count)){Buffer.RemoveAt(Buffer.Count-1);}
            points.AddRange(Buffer);
        }
        f.G.Clear(Color.White);
        f.G.DrawLines(def, points.ToArray());
    }
}
static class ExternalControl{
    public static int fps{get; private set;}
    static void ZeroFrames(object sender, ElapsedEventArgs e){fps = 0;}
    static ElapsedEventHandler ZFHandler = ZeroFrames;
    static void IncrementFrames(){fps++;}

    static Timer _1 = new Timer();
    public static void Initialise(){
        _1.AutoReset = true;
        _1.Interval = 1000;
		_1.Elapsed += Entry.TUpdate;
        _1.Elapsed += ZFHandler;
    }
    public static void StartTimer(){_1.Start();}
    public static void StopTimer(){fps = 0;		_1.Stop();		_1.Dispose();}
}


public partial class Form1 : Form{
    Graphics GrphcsPrprty;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Graphics G {get{return GrphcsPrprty == null? this.CreateGraphics() : GrphcsPrprty;} private set{GrphcsPrprty = value;}}
    public Form1(){
        InitializeComponent();
    }
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