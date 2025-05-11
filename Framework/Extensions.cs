using Timer = System.Timers.Timer;
using System.Timers;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

public class TextureStyles{
    public static TextureStyles StretchToFit = (TextureStyles)0;
    public static TextureStyles EdgeFillBlack = (TextureStyles)1;
    public static TextureStyles EdgeFillWhite = (TextureStyles)2;
    public static TextureStyles Empty = EdgeFillBlack;
    public static TextureStyles ClipToFit = (TextureStyles)3;
    public static explicit operator TextureStyles(int i){if(IsStyle(i)){return new TextureStyles(i);}else{return TextureStyles.Empty;}}
    public static implicit operator int(TextureStyles t){return t.type;}
    public static bool IsStyle(int type){
        switch(type){
            case 0:
                return true;
            case 1:
                return true;
            case 10:
                return true;
            case 2:
                return true;
            case 3:
                return true;
            default:
                return false;
        }
    }
    int type;
    internal TextureStyles(int i){this.type = i;}

    /// <summary>Take the TextureData and it's corresponding Mesh, stretching and squashing the colors to ensure they fit</summary>
    /// <param name="_12mid">The midpoint between the first and second UVPoint</param>
    /// <param name="p0_p1">The equation of the line that is the first UVPoint to the second UVPoint</param>
    /// <param name="p1_p12mid">The equation describing the 1st UVPoint to the _12mid Point.</param>
    /// <param name="p1_p2">The equation describing the linethat is the 1st UVPoint to the 2nd Point.</param>
    /// <param name="tD">The TextureDatabase to be manipulated.</param>
    /// <param name="p">The Polygon that the TextureDatabase is being manipulated to.</param>
    /// <param name="Start">The starting index of the Polygon's TextureData</param>
    /// <param name="End">The ending index of the Polygon's TextureData</param>
    static TextureDatabase StretchToFit_(TextureDatabase tD, Polygon p, int index){
        //This will use p's UVPoint data to manipulate tD
        for(int cc =0; cc < p.UVPoints.Length;cc++){
            
            Equation perpendicular = Equation.FromPoints(p.UVPoints[0], 
            //Midpoint between p.UVPoints[1] and p.UVPoints[2]
            new Point((p.UVPoints[1].Y + p.UVPoints[2].Y)/2, (p.UVPoints[1].X + p.UVPoints[2].X)/2));
            int MinY = Texturer.Min([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
            int MaxY = Texturer.Max([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
            for(int y =MinY; y < MaxY - MinY;y++){
                //For the length of the Texture.

				//buffer is a section of tD.
				TextureDatabase buffer= tD.RetrieveTexture_PerSectionBasis(index);
                
            }
        }
        
    return tD;
    }
    public static Color BlendColors(Color color1, Color color2, float t){
        t = Math.Clamp(t, 0f, 1f); // Ensure t is between 0 and 1
        int r = (int)(color1.R + t * (color2.R - color1.R));
        int g = (int)(color1.G + t * (color2.G - color1.G));
        int b = (int)(color1.B + t * (color2.B - color1.B));
        int a = (int)(color1.A + t * (color2.A - color1.A));
        return Color.FromArgb(a, r, g, b);
    }
    public static Color CompressColors(params Color[] colors){
        if (colors.Length == 0) return Color.Transparent;
        int totalR = 0, totalG = 0, totalB = 0, totalA = 0;
        foreach (var color in colors){
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            totalA += color.A;
        }
        int count = colors.Length;
        return Color.FromArgb(totalA / count, totalR / count, totalG / count, totalB / count);
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
        Impulse = (ForceMode?)0;
        Acceleration = (ForceMode?)1;
        VelocityChange = (ForceMode?)2;
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
    public static async Task<Point[]> SortPointArray_BySize(Point[] points){
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
    public static async Task<Point[]> SortPointArray_By0(Point[] points){
        bool swapped = false;
        await Task.Run(() => {
            int cc =0;
            do{
                if(CustomSort.GetDistance(Point.Empty, points[cc]) > CustomSort.GetDistance(Point.Empty, points[cc+1])){
                    swapped = true;
                    CustomSort.SwapItems(points, cc, cc+1);
                }
                cc++;
            }while(swapped);
        });
        return points;
    }
    public static Task<bool> Unsorted_By0(Point[] points){
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
    public static async Task<Point[]> SortPointArray_ByX(Point[] points){
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
    public static Task<bool> Unsorted_ByX(Point[] points){
        bool swapped = false;
        Task<bool> t = Task.Run(() => {
            for(int cc =0; cc < points.Length;cc++){
                if(points[cc].X > points[cc+1].X){
                    swapped = true;
                    break;
                }
            }
            return swapped;
        });
        return t;
    }
    public static async Task<Point[]> SortPointArray_ByY(Point[] points){
        bool swapped = false;
        await Task.Run(() => {
            int cc =0;
            do{
                if(points[cc].Y > points[cc+1].Y){
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
    public static T[] SwapItems<T>(T[] items, int FirstItem, int SecondItem){
        T buffer = items[FirstItem];
        items[FirstItem] = items[SecondItem];
        items[SecondItem] = buffer;
        return items;
    }
    static float GetDistance(Point a, Point b){
        int x = Math.Abs(a.X - b.X);
        int y = Math.Abs(a.Y - b.Y);
        return (float)Math.Sqrt((x^2) + (y^2));
    }
}
[Serializable]
class InconsistentDimensionException : Exception{
    public InconsistentDimensionException(string message) : base(message){}
    public InconsistentDimensionException(string message, Exception inner) : base(message, inner){}
    public InconsistentDimensionException(){}
    public override string Message => base.Message;
    public void ThrowIf(bool condition, string message){
        if(condition){throw new InconsistentDimensionException(message);}
    }
}



/// <summary>Represents a scheduled job that runs within a lock to prevent deadlocks.</summary>
class LockJob<T, R>{
    public delegate R LockJobDelegate<T, R>(T obj);
	bool disposed;
	bool running;
	DateTime LockStart;
	object? sender;
	int _ID;
    int _Timeout;
	LockJobDelegate<T, R> Job;
	IAsyncResult AsyncResult;
    LockJob(int ID, int Timeout, LockJobDelegate<T, R> job = null, object? sender = null){
        this.LockStart = DateTime.Now;
        this._ID = ID;
		if(job != null){
			this.Job = job;
			this.Job += OnTimeout;
		}
        this._Timeout = Timeout;
		this.sender = sender;
    }
	async Task<R?> Start(CancellationToken token, T obj){
		this.LockStart = DateTime.Now;
		this.running = true;
		return await Task.Run(() => this.Job(obj), token);
	}
	R OnTimeout(T input){
		if(DateTime.Now.Ticks - this.LockStart.Ticks > _Timeout){
			this.Break(this);
		}
		return default;
	}
	void Break(object sender){
		try{
			Job.EndInvoke(AsyncResult);
		}catch(Exception ex){
			System.Windows.Forms.MessageBox.Show($"Error in LockJob: {ex.Message}\nFrom sender: {sender}");
		}
		this.running = false;
	}
	void OnFinish(IAsyncResult result){
		try{
			this.Dispose();
		}catch(Exception ex){
			System.Windows.Forms.MessageBox.Show($"Error in LockJob: {ex.Message}");
		}
	}
	public void Dispose(bool disposing = true){
		if(!(disposed && running)){
			if(disposing){
				this.Job = null;
			}
			disposed = true;
		}
		GC.SuppressFinalize(this);
	}


	public override bool Equals(object obj){
		if (obj is LockJob<T, R> other){
			return _ID == other._ID &&
				LockStart == other.LockStart &&
				_Timeout == other._Timeout;
    }
    return false;
	}


	public static bool operator ==(LockJob<T, R> l1, LockJob<T, R> l2){return l1.Equals(l2);}
	public static bool operator !=(LockJob<T, R> l1, LockJob<T, R> l2){return !(l1 == l2);}
	public override int GetHashCode(){return HashCode.Combine(_ID, LockStart, _Timeout);}

    public static class LockJobHandler<T, R>{
		static readonly ConcurrentQueue<LockJob<T, R>> jobs = new ConcurrentQueue<LockJob<T, R>>();
		public static async Task<R?> PassJob(LockJobDelegate<T, R> job, CancellationToken token,  T input, object? sender = null, int Timeout = 1000){
			AddJob(job, sender, Timeout);
			return await ProcessJob(token, input);
		}
		public static void AddJob(LockJobDelegate<T, R> job, object? sender = null, int timeout = 1000){
            jobs.Enqueue(new LockJob<T, R>(jobs.Count, timeout, new LockJob<T, R>.LockJobDelegate<T, R>(job), sender));
		}
		public static async Task<R?> ProcessJob(CancellationToken token, T input){
			if(jobs.TryDequeue(out LockJob<T, R> job)){
				return await job.Start(token, input);
			}else{
				return default;
			}
			
    	}
    }
}