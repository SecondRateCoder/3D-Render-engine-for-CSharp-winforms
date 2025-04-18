using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
class Entry{
    public static CancellationTokenSource cts{get; private set;}
    public static ElapsedEventHandler TUpdate;
    public static Action Update;
    public static BackgroundWorker bWorker = new BackgroundWorker();
    public static Action Start;
    static Pen def;
    
    static Form1 f = new Form1();

    public static void Main(){
        ApplicationConfiguration.Initialize();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        cts = new CancellationTokenSource();
		Start = ExternalControl.StartTimer;
		Update = Paint3D;
        Loop();
		Application.Run(f);
    }
    static async void Loop(){
        await Task.Run(() => {
            if(Entry.cts == null){cts = new CancellationTokenSource(360000);}
            while(!Entry.cts.IsCancellationRequested){
                if(Update != null){Entry.Update();}
            }
        });
    }

    //Paint the enviroment.
    static void Paint3D(){
        try{f.Name = $"TheWindowText, fps: {ExternalControl.fps}";}
        catch(NullReferenceException){f.Name = $"TheWindowText, fps: {0}";}
        int formWidth = 0;
        int formHeight = 0;
        try{
            formWidth = Form.ActiveForm.Width;
            formHeight = Form.ActiveForm.Height;
        }catch(NullReferenceException){return;}
            (Point p, Color color)[] values = [
                (new Point((int)(0.25 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.1 * formHeight)), Color.Black), 
                (new Point((int)(0.25 * formWidth), (int)(0.9 * formHeight)), Color.Black), 
                (new Point((int)(0.75 * formWidth), (int)(0.9 * Form.ActiveForm.Height)), Color.Black), 
            ];
        int color =0;
        for(int cc =1; cc < values.Length; cc+=2, color++){
            def = new Pen(values[color].color);
            f.G.DrawLines(def, ViewPort.DrawBLine(values[cc].p, values[cc+1].p));
        }
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