using System.Timers;
using Timer = System.Timers.Timer;
using System.ComponentModel;
using System.Linq.Expressions;
static class Entry{
    public static class FrameControl{
        public static void IncrementFrames(){frames++;}
        /// <summary>
        ///  The property returns the amount of frames that have been drawn in one second;
        /// </summary>
        public static float fps{get{return frames/(TimeBetweenFrames.time2-TimeBetweenFrames.time1);}}
        //The time passed in one DUpdate(Default Update, represents the executions that occur in one frame) execution;
        internal static (float time1, float time2) TimeBetweenFrames;
        //To track the number of TUpdate calls to make sure it's equal to 5
        internal static int _5 = 0;
        //This increments every DUpdate call and is reset every 5 TUpdate calls(1 second), it tracks the number of frame calls in a second
        internal static int frames;
    }
    /// <summary>
    ///  This delegate property is called every <see cref="Entry.timer.Interval"/> seconds, it is called automatically.
    /// </summary>
    public static ElapsedEventHandler TUpdate;
    /// <summary>
    ///  (Default update.)This delegate property runs as frequntly as possible.
    /// </summary>
    public static Action DUpdate;
    /// <summary>
    ///  This delegate property runs when the form loads.
    /// </summary>
    public static Action Start;
    /// <summary>
    ///  This delegate runs at the end of a frame.
    /// </summary>
    public static Action FrameEnd;
    public delegate void FrameEndCall(IAsyncResult Ia);
    public static FrameEndCall Diagnose; 
    static Brush def = new SolidBrush(Color.Black);
    static Form1 f = new Form1();
    public static AsyncCallback callback;
    public static object @object;
    public static object sender;
    public static EventArgs e;
    public static CancellationTokenSource cts;
    public static void Main(){
        ApplicationConfiguration.Initialize();
        Diagnose = Diagnostics;
        cts = new CancellationTokenSource();
        e = new EventArgs();
        sender = new object();
        callback = new AsyncCallback(Diagnose);
        @object = new object();
        StorageManager.filePath = AppDomain.CurrentDomain.BaseDirectory;
        Start = TimerControl.InitialiseTimer;
        Start += TimerControl.StartTimer;
        Start += Loop;
        DUpdate = FrameControl.IncrementFrames;
        DUpdate += Paint3D;
        Start.BeginInvoke(callback, @object);
        Application.Run(f);
    }
    //This boolean will be what ends the running loop and does some ending stuff.
    public static bool isRunning{private get; set;}
    /// <summary>
    ///  This is the enclosing loop that will infinitely cause DUpdate to be called.
    /// </summary>
    static async void Loop(){
        await Task.Delay(0);
        while(isRunning){
            FrameControl.TimeBetweenFrames.time1 = DateTime.Now.Millisecond/1000;
            if(DUpdate != null){
                DUpdate.Invoke();
            }
            FrameControl.TimeBetweenFrames.time2 = DateTime.Now.Millisecond/1000;
            f.Tbox.Name = float.IsInfinity(FrameControl.fps)? "0": Convert.ToString(FrameControl.fps);
            if(FrameEnd != null){
                FrameEnd.BeginInvoke(callback, @object);
            }
        }
    }
    public static void Diagnostics(IAsyncResult result){if(result.IsCompleted){return;}else{System.Windows.Forms.MessageBox.Show("Error in the engine."); Thread.Sleep(5000);}}
    //The 3d stuff.
    static void Paint3D(){
        f.Name = "TheWindowText"+FrameControl.fps;
        (Point a, Point b, Point c, Color color)[] values = ViewPort.Convert_();
        for (int cc = 0; cc < values.Length; cc++){
            def = new SolidBrush(values[cc].color);
            f.g.DrawPolygon(new Pen(def), new Point[3]{values[cc].a, values[cc].b, values[cc].c});
        }
    }
    public static gameObj BuildWorld(){
        List<Polygon> polys = [new Polygon(new Vector3(20, 0, 20), new Vector3(20, 0, -20), new Vector3(-20, 0, 20)), 
        new Polygon(new Vector3(20, 0, -20), new Vector3(-20, 0, -20), new Vector3(-20, 0, 20))];
        gameObj gO = new gameObj(Vector3.zero, Vector3.zero, polys);
        World.worldData.Add(gO);
        return gO;
    }


    //The timer stuff.
    public class TimerControl{
        static Timer timer = new Timer();
        /// <summary>
        ///  Initialises the Entry class' timer property, the timer will start when the form loads.
        /// </summary>
        internal static void InitialiseTimer(){
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += TimerElapsed;
            timer.Elapsed += TUpdate;
        }
        internal static void TimerElapsed(object sender, ElapsedEventArgs e){
            if(TUpdate != null){
                TUpdate(sender, e);
            }
            if(FrameControl._5 == 5){
                FrameControl.frames = 0;
                FrameControl._5 = 0;
            }else{
                FrameControl._5++;
            }
        }
        public static void StartTimer(){timer.Start();}
        public static void StopTimer(){
            timer.Stop();
            timer.Dispose();
        }
    }
}
public partial class Form1 : Form
{
    private Graphics GrphcsPrprty;
    public Graphics g { get{return GrphcsPrprty == null? this.CreateGraphics() : GrphcsPrprty;} private set{GrphcsPrprty = value;} }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Form1()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        InitializeComponent();
    }
    override protected void OnLoad(EventArgs e){
        base.OnLoad(e);
        Entry.isRunning = true;
        Entry.Start(Entry.cts.Token);
    }
    protected override void OnClosing(CancelEventArgs e){
        base.OnClosing(e);
        Entry.isRunning = false;
        Entry.TimerControl.StopTimer();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        g = e.Graphics;
    }
}
