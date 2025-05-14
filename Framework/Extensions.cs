using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

class TextureStyles{
    /// <summary>Stretches a TextureDatabase to fit the inputted Polygon.</summary>
    /// <remarks>To apply this function; it requires the parameters: "TextureDatabase tD, Mesh m, Equation p0_p1, Equation p1_p2, Equation p0_p2, int StartAt = "0""</remarks>
    public static TextureStyles StretchToFit = (TextureStyles)0;
    public static TextureStyles EdgeFillBlack = (TextureStyles)1;
    public static TextureStyles EdgeFillWhite = (TextureStyles)2;
    public static TextureStyles Empty = (TextureStyles)int.MaxValue;
    public static TextureStyles ClipToFit = (TextureStyles)3;
    public static explicit operator TextureStyles(int i){if(IsStyle(i)){return new TextureStyles(i);}else{return TextureStyles.Empty;}}
    public static implicit operator int(TextureStyles t){return t.type;}
    static int[] Styles = [0, 1, 2, 3, 10];
    public static bool IsStyle(int type){return Styles.Contains(type);}
    int type;
    internal TextureStyles(int i){this.type = i;}
    public static TextureDatabase Apply(TextureStyles tS, TextureDatabase tD, Mesh m, Equation p0_p1, Equation p1_p2, Equation p0_p2, int Start = 0){
        return tS.type switch{
            0 => StretchToFit_(tD, m, p0_p1, p1_p2, p0_p2, 0).Result,
            _ => tD,
        };
    }
    static async Task<TextureDatabase> StretchToFit_(TextureDatabase tD, Mesh m, Equation p0_p1, Equation p1_p2, Equation p0_p2, int Start = 0){
        return await Task.Run(() => {
            foreach(Polygon p in m){
                tD = StretchToFit_(tD, p, Start, p0_p1, p1_p2, p0_p2).Result;
                Start++;
            }
            return tD;
        });
    }

    /// <summary>Take the TextureData and it's corresponding Mesh, stretching and squashing the colors to ensure they fit</summary>
    /// <param name="_12mid">The midpoint between the first and second UVPoint</param>
    /// <param name="p0_p1">The equation of the line that is the first UVPoint to the second UVPoint</param>
    /// <param name="p1_p12mid">The equation describing the 1st UVPoint to the _12mid Point.</param>
    /// <param name="p1_p2">The equation describing the linethat is the 1st UVPoint to the 2nd Point.</param>
    /// <param name="tD">The TextureDatabase to be manipulated.</param>
    /// <param name="p">The Polygon that the TextureDatabase is being manipulated to.</param>
    static async Task<TextureDatabase> StretchToFit_(TextureDatabase tD, Polygon p, int Sectionindex, 
    Equation p0_p1, Equation p1_p2, Equation p0_p2){
        //This will use p's UVPoint data to manipulate tD
        return await Task.Run(() => {
            for(int i =0; i < p.UVPoints.Length;i++){
                //Fnd the length at different points, blend or Compress
                Equation perpendicular = Equation.FromPoints(p.UVPoints[0], 
                //Midpoint between p.UVPoints[1] and p.UVPoints[2]
                new PointF((p.UVPoints[1].Y + p.UVPoints[2].Y)/2, (p.UVPoints[1].X + p.UVPoints[2].X)/2));
                float MinY = Texturer.Min([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
                float MaxY = Texturer.Max([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
                TextureDatabase SlicedData= tD.Slice_PerSectionRanges(Sectionindex);
                PointF[] PolygonAsPointF = [new PointF(((PointF)p.A).X * p.A.Magnitude, ((PointF)p.A).X * p.A.Magnitude), 
                    new PointF(((PointF)p.B).X * p.B.Magnitude, ((PointF)p.B).X * p.B.Magnitude), 
                        new PointF(((PointF)p.C).X * p.C.Magnitude, ((PointF)p.C).X * p.C.Magnitude)];
                PolygonAsPointF = CustomSort.SortPointArray_ByY(PolygonAsPointF).Result.ToArray();


                //To handle how TextureData is scaled, or whether it is scaled.
                Equation Polyp0_p1 = Equation.FromPoints(PolygonAsPointF[0], PolygonAsPointF[1]);
                Equation Polyp1_p2 = Equation.FromPoints(PolygonAsPointF[1], PolygonAsPointF[2]);
                Equation Polyp0_p2 = Equation.FromPoints(PolygonAsPointF[0], PolygonAsPointF[2]);
                int cc =0;
                for(float y =MinY; y < MaxY - MinY;y++){
                    float xUpper = p0_p2.SolveX(y);
                    float shiftingX = y <= p.UVPoints[1].Y? p0_p1.SolveX(y): p1_p2.SolveX(y);
                    float xRange = xUpper - shiftingX;
                    float PolyxRange = Polyp0_p2.SolveX(y) - (y <= PolygonAsPointF[1].Y? Polyp0_p1.SolveX(y): Polyp1_p2.SolveX(y));
                    float increment = 0;
                    for(float x = 0; x < PolyxRange;increment = PolyxRange/xRange, x += increment, cc++){
                        float index = x + (y * xRange);
                        TextureDatabase.TexturePoint buffer;
                        if(increment > 1){buffer = BlendColors(increment, tD[cc-1], tD[cc]);}else{buffer = CompressColors(.5f, tD[cc-1], tD[cc]);}
                        tD.AddAt((int)index, buffer);
                    }
                }
            }
            return tD;
        });
        
    }
    /// <summary>Produces a TexturePoint that is inbetween <see cref="color1"/> and <see cref="color2"/>.</summary>
    /// <param name="color1">The 1st Color to be blended against.</param>
    /// <param name="color2">The 2nd Color to be blended against.</param>
    /// <param name="p">The Location that this <see cref="TextureDatabase.TexturePoint"/> located in reference the line: color1 => color2</param>
    /// <returns>A TexturePoint.</returns>
    public static TextureDatabase.TexturePoint BlendColors(float p, TextureDatabase.TexturePoint color1, TextureDatabase.TexturePoint color2){
        string ArgString = $"TextureStyles.BlendColors(p: {p}, "+
            $"color1: \"p: {color1.p.X}, {color1.p.X}; c: {color1.c.A}, {color1.c.R}, {color1.c.G}, {color1.c.B}), "+
            $"color1: \"p: {color2.p.X}, {color2.p.X}; c: {color2.c.A}, {color2.c.R}, {color2.c.G}, {color2.c.B})";
        
        TextureDatabase.TexturePoint result = new(new((int)((color1.p.X + color2.p.X) * p), (int)((color1.p.Y + color2.p.Y) * p)), Color.White);
        if(((color1.p.X * color1.p.X) + (color1.p.Y * color1.p.Y)) < ((color2.p.X * color2.p.X) + (color2.p.Y * color2.p.Y))){throw new ArgumentException("color1 cannot be larger that color2", "At: "+ArgString);}
        if(p >= 1 | p <= 0){throw new ArgumentOutOfRangeException("TextureStyles.BlendColors");}
        //The weight from Color1 to point
        float Weight1 = (float)Math.Sqrt(((result.p.X - color1.p.X) * (result.p.X - color1.p.X)) + ((result.p.Y - color1.p.Y) * (result.p.Y - color1.p.Y)));
        //The Weight from Color2 to point
        float Weight2 = (float)Math.Sqrt(((color2.p.X - result.p.X) * (color2.p.X - result.p.X)) + ((color2.p.Y - result.p.Y) * (color2.p.Y - result.p.Y)));
        result.c = BlendColors(color1.c, Color.FromArgb((byte)(color1.c.A/Weight1), (byte)(color1.c.R/Weight1), (byte)(color1.c.R/Weight1), (byte)(color1.c.R/Weight1)), Weight1);
        result.c = BlendColors(color2.c, result.c, Weight2);
        return result;
    }
    /// <summary>Produces a TexturePoint that is inbetween <see cref="color1"/> and <see cref="color2"/> and at <see cref="p"/>.</summary>
    /// <param name="p">The Location of the TexturePoint</param>
    /// <returns>A Color that is a blend of <see cref="color1"/> and <see cref="color2"/>, blended in reference to <see cref="p"/>.</returns>
    /// <exception cref="ArgumentException">If <see cref="color2"/> is not greater than <see cref="color1"/>.</exception>
    public static TextureDatabase.TexturePoint BlendColors(PointF p, TextureDatabase.TexturePoint color1, TextureDatabase.TexturePoint color2){
        if(((color1.p.X * color1.p.X) + (color1.p.Y * color1.p.Y)) < ((color2.p.X * color2.p.X) + (color2.p.Y * color2.p.Y))){
            string ArgString = $"TextureStyles.BlendColors()";
            throw new ArgumentException("color1 cannot be larger that color2", "At: "+ArgString);}
        float Magnitude = (float)Math.Sqrt(((color2.p.X - color1.p.X) * (color2.p.X - color1.p.X)) + ((color2.p.Y - color1.p.Y) * (color2.p.Y - color1.p.Y)));
        return BlendColors(Magnitude, color1, color2);
    }
    /// <summary>
    /// Blend <see cref="color1"/> and <see cref="color2"/> by <see cref="t"/>.
    /// </summary>
    /// <param name="t">The weight of the blending.</param>
    /// <returns>A blended Color.</returns>
    static Color BlendColors(Color color1, Color color2, float t){
        Math.Clamp(t, 0, 1);
        int r = (int)(color1.R + t * (color2.R - color1.R));
        int g = (int)(color1.G + t * (color2.G - color1.G));
        int b = (int)(color1.B + t * (color2.B - color1.B));
        int a = (int)(color1.A + t * (color2.A - color1.A));
        return Color.FromArgb(a, r, g, b);
    }
    static TextureDatabase.TexturePoint CompressColors(float strength, params TextureDatabase.TexturePoint[] colors){
        Color result = colors[0].c;
        int cc = 0;
        PointF point = colors[0].p;
        foreach(TextureDatabase.TexturePoint c in  colors){
            if(cc == 0){continue;}
            result = Color.FromArgb((int)((result.A + c.c.A)/strength), (int)((result.R + c.c.R)/strength), (int)((result.A + c.c.G)/strength), (int)((result.A + c.c.B)/strength));
            point = new PointF((point.X + c.p.X)/2, (point.Y + c.p.Y)/2);
        }
        return new TextureDatabase.TexturePoint(point, result);
    }
}
/// <summary>
/// The true shape of a Collider
/// </summary>
class TrueCollider{
    //public static readonly TrueCollider Sphere = 0;
    public static TrueCollider Cube{get{_cube.Dimensions = (5, 5); return _cube;}}
#pragma warning disable CS8601 // Possible null reference assignment.
    static readonly TrueCollider _cube = (TrueCollider?)1;
    public static TrueCollider Capsule{get{_capsule.Dimensions = (5, 3); return _capsule;}}
    static readonly TrueCollider _capsule = (TrueCollider?)2;
#pragma warning restore CS8601 // Possible null reference assignment.

    internal string Name{get; private set;}
    internal int type;

    public (int height, int width) Dimensions{get; private set;}
    public void UpdateDimensions(int height, int width){this.Dimensions = (height, width);}

    internal TrueCollider(int tyPe, int Height = 10, int Width = 3){
        if(IsCollider(tyPe)){
            this.Name = TrueCollider.GetCollider(tyPe);
            this.type = tyPe;
            this.Dimensions = (Height, Width);
        }else{
            this.Name = "Cube";
            this.type = TrueCollider.Cube.type;
            this.Dimensions = TrueCollider.Cube.Dimensions;
        }
    }

    public static bool IsCollider(int type){
        switch(type){
            case 1: return true;
            case 2: return true;
            default: return false;
        }
    }
    public static string GetCollider(int type){
        if(!IsCollider(type)){return "";}else{
            switch(type){
                case 1: return  "Cube";
                case 2: return "Cylinder";
                default: return "";
            }
        }
    }
    public static explicit operator TrueCollider(int type){if(IsCollider(type)){return new TrueCollider(type);}else{return TrueCollider.Cube;}}
    public static explicit operator int(TrueCollider tC){
        return tC.type;
    }
    public static Polygon[] GetMesh(TrueCollider? tC){
        int bevel = 0;
        if(tC == null){return Polygon.Mesh(10, 10, bevel);}
        bevel = tC.type switch{
            1 => 0,
            2 => 2,
            _ => 0,
        };
        return Polygon.Mesh(tC.Dimensions.height, tC.Dimensions.width, bevel);
    }
    public static void Assign(TrueCollider tC1, TrueCollider tC2){tC1 = tC2;}
}

/// <summary>
/// This will affect how physics interacts with the attached object.
/// </summary>
class PhysicsMaterial{
    public static readonly PhysicsMaterial SandPaper = new PhysicsMaterial(10, 0);
    public static readonly PhysicsMaterial Rubber = new PhysicsMaterial(5, 7);
    public static readonly PhysicsMaterial GlazedWood = new PhysicsMaterial(3, 1);
    public static explicit operator PhysicsMaterial((int a, int b) tC){return new PhysicsMaterial(tC);}
    public static bool IsPhysicsMaterial((int f, int b) item){
        switch(item){
            case (10, 0): return true;
            case (5, 7): return true;
            case (3, 1): return true;
            default: return false;
        }
    }

    public int Friction{get; private set;}
    public int Bounciness{get; private set;}
    public PhysicsMaterial(int Friction, int Bounciness){
        this.Friction = Friction;
        this.Bounciness = Bounciness;
    }
    public PhysicsMaterial((int f, int b) item){
        this.Friction = item.f;
        this.Bounciness = item.b;
    }
}

/// <summary>
/// Affects how velocity is Applied to the attached object.
/// </summary>
class ForceMode{
#pragma warning disable CS8601 // Possible null reference assignment.
    static ForceMode(){
        Impulse = ForceMode.FromType(0);
        Acceleration = ForceMode.FromType(1);
        VelocityChange = ForceMode.FromType(2);
#pragma warning restore CS8601 // Possible null reference assignment.
    }
    ///<summary>Apply a force that is affected by the gameObjects mass and is multiplied by <see cref"ExternalControl.deltaTime"></summary>
    /// <remarks>This is also the default ForceMode value.</remarks>
    public static readonly ForceMode Impulse;
    ///<summary>Apply a force that is affected by the gameObjects mass.</summary>
    public static readonly ForceMode Acceleration;
    ///<summary>Apply a force that ignores the object's mass.</summary>
    public static readonly ForceMode VelocityChange;
    public static explicit operator ForceMode?(int type){if(IsForceMode(type)){return new ForceMode(type);}else{return null;}}
    static ForceMode FromType(int type){return new ForceMode(type);}
    public static bool IsForceMode(int type){
        switch(type){
            case 0: return true;
            case 1: return true;
            case 2: return true;
            default: return false;
        }
    }

    public int type;
    internal ForceMode(int type){this.type = type;}
    public void Apply(RigidBdy rB, Vector3 v){
        if(rB.MetaData.fM == null){return;}
        if(!ForceMode.IsForceMode(rB.MetaData.fM.type)){return;}else{
            switch(this.type){
                case 0:
                    //Is Impulse.
                    rB.velocity += new Vector3((float)Math.Sqrt(v.X/(rB.Mass/2)), (float)Math.Sqrt(v.Y/(rB.Mass/2)), (float)Math.Sqrt(v.Z/(rB.Mass/2))) * ExternalControl.deltaTime;
                    break;
                case 1:
                    //Is Acceleration.
                    rB.velocity += new Vector3((float)Math.Sqrt(v.X/(rB.Mass/2)), (float)Math.Sqrt(v.Y/(rB.Mass/2)), (float)Math.Sqrt(v.Z/(rB.Mass/2)));
                    break;
                case 2:
                    //Is VelocityChange.
                    rB.velocity = v;
                    break;
                default:    return;
            }
        }
    }
}
static class CustomSort{
    /// <summary>
    /// Sort the Point array on all Axis.</summary>
    /// <param name="points">The array to be sorted.</param>
    /// <returns>A sorted array in the format, smallest to largest.</returns>
    public static async Task<PointF[]> SortPointArray_BySize(PointF[] points){
        bool swapped = false;
        await Task.Run(() => {
            do{
                //Hold the index at list.
                for(int cc =0; cc < points.Length;cc++){
                    swapped = false;
                    //Swap Ys first.
                    if(points[cc].Y > points[cc+1].Y){
                        swapped = true;
                        int cc_ = cc+1;
                        do{
                            cc_++;
                            if(points[cc].Y <= points[cc_].Y){
                                CustomSort.SwapItems(points, cc, cc_);
                                break;
                            }
                        }while((points[cc_].Y > points[cc_+1].Y) | cc_< points.Length);
                    }


                    //Swap Xs next
                    if((points[cc].X > points[cc+1].X) && (points[cc].Y == points[cc+1].Y)){
                        swapped = true;
                        CustomSort.SwapItems(points, cc, cc+1);
                    }
                }
            }while(swapped);
        });
        return points;
    }
    public static async Task<PointF[]> SortPointArray_By0(PointF[] points){
        bool swapped = false;
        await Task.Run(() => {
            int cc =0;
            do{
                if(CustomSort.GetDistance(PointF.Empty, points[cc]) > CustomSort.GetDistance(PointF.Empty, points[cc+1])){
                    swapped = true;
                    CustomSort.SwapItems(points, cc, cc+1);
                }
                cc++;
            }while(swapped);
        });
        return points;
    }
    public static Task<bool> Unsorted_By0(PointF[] points){
        bool swapped = false;
        Task<bool> t = Task.Run(() => {
            for(int cc =0; cc < points.Length;cc++){
                if(CustomSort.GetDistance(Point.Empty, points[cc]) > CustomSort.GetDistance(Point.Empty, points[cc+1])){
                    swapped = true;
                    break;
                }
            }
            return swapped;
        });
        return t;
    }
    public static async Task<PointF[]> SortPointArray_ByX(PointF[] points){
        bool swapped = false;
        await Task.Run(() => {
            int cc =0;
            do{
                if(points[cc].X > points[cc+1].X){
                    swapped = true;
                    CustomSort.SwapItems(points, cc, cc+1);
                }
                cc++;
            }while(swapped);
        });
        return points;
    }
    public static Task<bool> Unsorted_ByX(IEnumerable<PointF> points){
        bool swapped = false;
        Task<bool> t = Task.Run(() => {
            for(int cc =0; cc < points.Count();cc++){
                if(points.ElementAt(cc).X > points.ElementAt(cc+1).X){
                    swapped = true;
                    break;
                }
            }
            return swapped;
        });
        return t;
    }
    public static async Task<IEnumerable<PointF>> SortPointArray_ByY(IEnumerable<PointF> points){
        bool swapped = false;
        await Task.Run(() => {
            int cc =0;
            do{
                if(points.ElementAt(cc).Y > points.ElementAt(cc+1).Y){
                    swapped = true;
                    CustomSort.SwapItems(points, cc, cc+1);
                }
                cc++;
            }while(swapped);
        });
        return points;
    }
    public static Task<bool> Unsorted_ByY(Point[] points){
        bool swapped = false;
        Task<bool> t = Task.Run(() => {
            for(int cc =0; cc < points.Length;cc++){
                if(points[cc].Y > points[cc+1].Y){
                    swapped = true;
                    break;
                }
            }
            return swapped;
        });
        return t;
    }
    public static IEnumerable<T> SwapItems<T>(IEnumerable<T> items, int FirstItem, int SecondItem){
        T[] values = items.ToArray();
        T buffer = values[FirstItem];
        values[FirstItem]= values[SecondItem];
        values[SecondItem] = buffer;
        return values;
    }
    static float GetDistance(PointF a, PointF b){
        float x = Math.Abs(a.X - b.X);
        float y = Math.Abs(a.Y - b.Y);
        return (float)Math.Sqrt((x * x) + (y * y));
    }
}
[Serializable]
class InconsistentDimensionException : Exception{
    public InconsistentDimensionException(string message) : base(message){}
    public InconsistentDimensionException(string message, Exception inner) : base(message, inner){}
    public InconsistentDimensionException(){}
    public override string Message => base.Message;
    public static void ThrowIf(bool condition, string message, Exception? InnerException = null){
        if(condition){throw new InconsistentDimensionException(message, inner: InnerException ?? new Exception());}
    }
}
[Serializable]
class AssemblyLoadException : Exception{
    public AssemblyLoadException(string message) : base(message){}
    public AssemblyLoadException(string message, Exception inner) : base(message, inner){}
    public AssemblyLoadException(){}
    public override string Message => base.Message;
    public static void ThrowIf(bool condition, string message, Exception? InnerException = null){
        if(condition){throw new AssemblyLoadException(message, inner: InnerException ?? new Exception());}
    }
}



/// <summary>
/// Represents a job that runs within a lock to prevent deadlocks.
/// This works by running the job and then calling the OnFinish delegate to complete the job.
/// However if the job takes too long, the OnTimeout delegate is called.
/// </summary>
/// 
class LockJob<T, R>{
    /// <summary>
    /// The delegate that represents a job.
    /// </summary>
    /// <typeparam name="_T">The parameters for the delegate, for multiple parameters make this a tuple.</typeparam>
    /// <typeparam name="_R">The return type for the delegate.</typeparam>
    public delegate _R LockJobDelegate<_T, _R>(_T obj);
    /// <summary>
    /// The delegate that represents when the job exceeds it's allocated runtime.
    /// </summary>
    /// <param name="sender">The name of the object that requested this job.</param>
    public delegate void OnTimeout(object sender);
    /// <summary>
    /// Has this job been disposed of.
    /// </summary>
	bool disposed;
    /// <summary>
    /// Has this job been started. Alternatively, is this job running right now.
    /// </summary>
	bool running;

    /// <summary>
    /// The name of the object that requested this job.
    /// </summary>
	object? sender;
    /// <summary>
    /// The ID of the job.
    /// </summary>
	int _ID;
    /// <summary>
    /// The time in seconds that the job is allowed to run.
    /// </summary>
    int Timeout;
    /// <summary>
    /// The job that is being run.
    /// </summary>
	LockJobDelegate<T, R> Job;
    /// <summary>
    /// The delegate run when the job is completed.
    /// </summary>
    Action OnFinish;
    /// <summary>
    /// The delegate run when the job times out.
    /// </summary>
    OnTimeout OnTimeExceed;
    /// <summary>
    /// Cancellation token source to cancel the job.
    /// </summary>
    CancellationTokenSource cts;


    LockJob(int ID, int Timeout, LockJobDelegate<T, R> job, Action? OnFinish = null, OnTimeout? @OnTimeout = null, object? sender = null){
        if(OnFinish == null){
            this.OnFinish = TrueFinish;
        }else{
            this.OnFinish = OnFinish;
            this.OnFinish += TrueFinish;
        }
        if(OnTimeout == null){
            this.OnTimeExceed = TrueTimeout;
        }else{
            this.OnTimeExceed = OnTimeout;
            this.OnTimeExceed += TrueTimeout;
        }
        this._ID = ID;
        this.Job = job;
        this.Timeout = Timeout;
        this.cts = new CancellationTokenSource();
        result = null;
        this.cts.Token.Register(() => {this.OnTimeExceed(this.sender ?? new object());});
    }
    R? Start(T param){
        R? result = Task.Run(() => this._Start(param)).Result;
        this.OnFinish();
        return result;
    }
    private R? _Start(T param){
        this.running = true;
        cts.CancelAfter(Timeout * 1000);
        R? result = this.Job(param);
        return result;
    }
    void TrueFinish(){
        try{
            this.Dispose();
        }catch(Exception ex){
            MessageBox.Show($"Error in LockJob: ObjectDisposalException.\nMessage: {ex.Message}\nFrom sender: {this.sender}");
        }
        this.running = false;
    }
    void TrueTimeout(object sender){
        MessageBox.Show($"Error in LockJob: TimeoutException.\nFrom sender: {sender}");
    }

    IAsyncResult? result;
	public void Dispose(bool disposing = true){
		if(!(disposed && running)){
			if(disposing){
                this.running = false;
				this.Job.EndInvoke(result);
			}
			disposed = true;
		}
		GC.SuppressFinalize(this);
	}


	public override bool Equals([NotNullWhen(true)]object? obj){
		if (obj is LockJob<T, R> other){
			return _ID == other._ID;
    }
    return false;
	}


	public static bool operator ==(LockJob<T, R> l1, LockJob<T, R> l2){return l1.Equals(l2);}
	public static bool operator !=(LockJob<T, R> l1, LockJob<T, R> l2){return !(l1 == l2);}
	public override int GetHashCode(){return HashCode.Combine(_ID);}

    public static class LockJobHandler{
        /// <summary>
        /// The queue of jobs to be processed.
        /// </summary>
        /// <remarks>Thread safe.</remarks>
		static readonly ConcurrentQueue<LockJob<T, R>> jobs = new();
        /// <summary>
        /// Process a job, Both running it and returning the result.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="input"></param>
        /// <param name="token"></param>
        /// <param name="sender"></param>
        /// <param name="Timeout"></param>
        /// <returns></returns>
		public static async Task<R?> PassJob(LockJobDelegate<T, R> job,  T input, Action? OnFinish = null, int Timeout = 1000, OnTimeout? @OnTimeout = null, object? sender = null){
            return await ProcessJob(new LockJob<T, R>(0, Timeout, job, OnFinish, OnTimeout, sender), input);
		}
		public static void AddJob(LockJobDelegate<T, R> job, object? sender = null, int timeout = 1000){
            jobs.Enqueue(new LockJob<T, R>(jobs.Count, timeout, new LockJob<T, R>.LockJobDelegate<T, R>(job), null, null, sender));
		}
		public static async Task<R?> ProcessJob(CancellationToken token, T input){
			return await Task.Run(() => {
                if(jobs.TryDequeue(out LockJob<T, R>? job)){
                    return job.Start(input);
                }else{
                    return default;
                }
            });
    	}
        static async Task<R?> ProcessJob(LockJob<T, R> job, T input){
            return await Task.Run(() => {
                return job.Start(input);
            });
        }
    }
}


