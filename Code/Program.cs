using Timer = System.Timers.Timer;
using System.Timers;
using System.ComponentModel;
class Entry{
    public static CancellationTokenSource cts{get; private set;} = new CancellationTokenSource();
    public static ElapsedEventHandler TUpdate;
    public static Action Update = Paint3D;
    public static Action Start = ExternalControl.StartTimer;
    static Pen def = new(Color.Black);
    static List<Point> pointBuffer = [];
    public static Form1 f = new Form1();
    public static void Main(){
        ApplicationConfiguration.Initialize();
		StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
		ExternalControl.Initialise();
        Loop();
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
        pointBuffer = [];
        for(int cc =0; cc < values.Length-1; cc++){
            def = new Pen(values[cc].color, 5);
            List<Point> Buffer = [.. ViewPort.DrawBLine(values[cc].p, values[cc+1].p)];
            if(!int.IsEvenInteger(Buffer.Count)){Buffer.RemoveAt(Buffer.Count-1);}
            pointBuffer.AddRange(Buffer);
        }
		//
		if(f.InvokeRequired){
			f.Invoke(() => {
        if(f != null){
            f.Refresh();
            f.G.Clear(Color.White);
            if(pointBuffer != null){f.G.DrawPolygon(def, pointBuffer.ToArray());}
        }
    });
		}else{
			return;
		}
		//
        
        
    }
}
static class ExternalControl{
    public static int fps{get; private set;}
    static ElapsedEventHandler ZFHandler = (sender, e) => {fps = 0;};
    static void IncrementFrames(){fps++;}

    public static Timer _1 = new Timer();
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