using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
class Entry{
    public static CancellationTokenSource cts{get; private set;}
    public static ElapsedEventHandler TUpdate;
    public static Action Update;
    public static Action Start;
    static Brush def = new SolidBrush(Color.Black);
    static Form1 f = new Form1();


    public static void Main(){
        ApplicationConfiguration.Initialize();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
		Start = ExternalControl.Start_1;
		Update = Paint3D;
		Application.Run(f);
    }

    //Paint the enviroment.
    static void Paint3D(){
        f.Name = $"TheWindowText, fps: {ExternalControl.fps}";
        (Point a, Point b, Point c, Color color)[] values = ViewPort.Convert_();
        for (int cc = 0; cc < values.Length; cc++){
            def = new SolidBrush(values[cc].color);
            f.G.DrawPolygon(new Pen(def), [values[cc].a, values[cc].b, values[cc].c]);
        }
    }
}
static class ExternalControl{
    public static int fps{get; private set;}
    static void ZeroFrames(object sender, ElapsedEventArgs e){fps = 0;}
    static void IncrementFrames(){fps++;}

    static Timer _1 = new Timer();
    static object sender;
    static ElapsedEventArgs e;
    public static void Initialise(){
        e = new ElapsedEventArgs(new DateTime());
        _1.AutoReset = true;
        _1.Interval = 1000;
		_1.Elapsed += Entry.TUpdate;
        _1.Elapsed += ZeroFrames;
    }
    public static void StartTimer(){_1.Start();}
    public static void StopTimer(){fps = 0;		Timer.Stop();		Timer.Dispose();}
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
        Task.Run(() =>{
			await Task.Run(() => {
				while(cts.IsCancellationRequested){
					if(Update != null){
						Update();
					}
				}
			});
		};);
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