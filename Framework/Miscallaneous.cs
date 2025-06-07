using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Timers;
using SharpDX;


struct Equation{
    public float Gradient;
    public float Y_intercept;
    public Equation(float X_coefficient, float Y_intercept){
        this.Gradient = X_coefficient;
        this.Y_intercept = Y_intercept;
    }
    public float SolveY(float X){
        return (X * Gradient) + Y_intercept;
    }
    public float SolveX(float Y){
        return (Y - Y_intercept)/Gradient;
    }
    public static Equation FromPoints(PointF a, PointF b){
        //Simultaenous equation.
        float gradient = (a.Y - b.Y)/(a.X - b.X);
        float y_intercept = a.Y - (a.X * gradient);
        return new Equation(gradient, y_intercept);
    }
    public static Equation GetPerpendicular(Equation e, Point p){
        float y = e.SolveY(p.X);
        float gradient = -(1/e.Gradient);
        float y_intercept = y - (p.X * gradient);
        return new Equation(gradient, y_intercept);
    }
    /// <summary>
    /// Finds the x-coordinate where both Equation a and b equal the same y-coordinate.
    /// </summary>
    /// <returns>A PointF detailing the x and y coordinates where x and y equal each other.</returns>
    public static PointF WhereEquationEquals(Equation a, Equation b){
        //a (y = mx + c)
        //b (y = nx + d)
        //a = (mx - y = c)
        //b = (nx - y = d)
        //Substitution method:
        //a = (x = (c + y)/m)
        //b = (n(c + y)/m - y = d)
        //b = ((nc + ny)/m - y = d)
        //b = (y - ny/m = d - nc/m)
        //b = (y(1 - n/m) = d - nc/m)
        //b = (y = (d - nc/m)/(1 - n/m))
        PointF p = new PointF();
        p.Y = (int)((b.Y_intercept - (b.Gradient * a.Y_intercept/a.Gradient))/(1 - (b.Gradient/a.Gradient)));
        p.X = (int)a.SolveX(p.Y);
        return p;
    }
}

class Camera{
    public Vector3 Position{get; set;} = Vector3.Zero;
    public Vector3 Rotation{get; set;} = Vector3.Zero;
	/// <summary>
	/// The normalised resolution at which textures are applied to a mesh
	/// </summary>
	public int Resolution;
    ///<summary>The angle from the camera to the window.</summary>
    public float theta{get{return (float)(2*Math.Atan((ViewPort.boundary.r/2)/this.near_));}}
    float near_;
    ///<summary>The length from the camera to the near plane.</summary>
    public float near{get{return near_;} set{near_ = (near_ == 0 && value < far? value: near_);}}
    float fov_ = 0;
    ///<summary>
    /// The render distance.
    ///</summary>
    ///<remarks>This property cant be changed after being assigned.</remarks>
    public float far{get{return fov_;} set{fov_ = (fov_ == 0? value: fov_);}}
    public Camera(float Fov = 15f, Vector3? pos = null, Vector3? rot = null, int Resolution = 1){
        this.Resolution = Resolution;
        this.far = Fov;
        this.Position = pos == null? Vector3.Zero: pos.Value;
        this.Rotation = pos == null? Vector3.Zero: pos.Value;
        _ = InputController.AttachKeyhandles(
            new ControlScheme(
                [Keys.W, Keys.A, Keys.S, Keys.D, Keys.Q, Keys.E], 
                [(duration, strength) => { Position = new Vector3(Position.X, Position.Y, Position.Z + 1); }, 
                    (duration, strength) => { Position = new Vector3(Position.X, Position.Y, Position.Z - 1); }, 
                    (duration, strength) => { Position = new Vector3(Position.X + 1, Position.Y, Position.Z); }, 
                    (duration, strength) => { Position = new Vector3(Position.X - 1, Position.Y, Position.Z); }, 
                    (duration, strength) => { Position = new Vector3(Position.X, Position.Y + 1, Position.Z); }, 
                    (duration, strength) => { Position = new Vector3(Position.X, Position.Y - .5f, Position.Z); }]
            ));
    }
    public void Translate(Vector3 position, Vector3 rotation){
        this.Position += position;
        this.Rotation += rotation;
    }
}

class Light{
    public Vector3 Source{get; private set;}
    public Color Colour{get; private set;}
    public int Radius;
    public int Intensity = 0;

    public Light(Vector3? source = null, Color? color = null, int mag = 10){
        this.Source = source == null? new Vector3(): source.Value;
        this.Colour = color == null? Color.WhiteSmoke: color.Value;
        this.Radius = (int)(mag*(3/4));
        this.Intensity = mag-Radius;
    }
}
class TextureStyles{
    /// <summary>Stretches a TextureDatabase to fit the inputted Polygon.</summary>
    /// <remarks>To apply this function; it requires the parameters: "TextureDatabase tD, Mesh m, Equation p0_p1, Equation p1_p2, Equation p0_p2, int StartAt = "0""</remarks>
    public enum Styles{
        Empty = 0,
        StretchToFit = 1,
        EdgeFillBlack = 2,
        EdgeFillWhite = 3,
        ClipToFit = 4,
        ClipToFit_EdgeFillBlack = 5,
        ClipToFit_EdgeFillWhite = 6
    }
    public static TextureStyles ClipToFit = (TextureStyles)3;
    /// <summary>Converts an integer to a TextureStyle</summary>
    /// <param name="i"></param>
    public static explicit operator TextureStyles(int i) {return Enum.IsDefined(typeof(Styles), i) ? new TextureStyles(i) : new((int)Styles.Empty);}
    public static implicit operator int(TextureStyles t){return t.type;}
    public static implicit operator Styles(TextureStyles tS){return (Styles)tS.type;}
    public static implicit operator TextureStyles(Styles tS){return new TextureStyles((int)tS);}
    int type;
    internal TextureStyles(int i){this.type = i;}
    public static TextureDatabase Apply(TextureStyles tS, TextureDatabase tD, Mesh m, Equation p0_p1, Equation p1_p2, Equation p0_p2, int Start = 0){
        return tS.type switch{
            (int)Styles.StretchToFit => StretchToFit_(tD, m, p0_p1, p1_p2, p0_p2, 0).Result,
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
                PolygonAsPointF = [.. CustomFunctions.SortPointArray_ByY(PolygonAsPointF).Result];


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
    public enum Colliders{
        Undefined = 0,
        Cube = 1,
        Capsule = 2,
        Sphere = 3
    }
    //public static readonly TrueCollider Sphere = 0;
    public static TrueCollider Cube { get { return Colliders.Cube; } }
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
        if(Enum.IsDefined(typeof(Colliders), type)){
            this.Name = TrueCollider.GetCollider(tyPe);
            this.type = tyPe;
            this.Dimensions = (Height, Width);
        }else{
            this.Name = "Cube";
            this.type = TrueCollider.Cube.type;
            this.Dimensions = TrueCollider.Cube.Dimensions;
        }
    }
    public static string GetCollider(int type){
        if(!Enum.IsDefined(typeof(Colliders), type)){return "";}else{
            return type switch{
                1 => "Cube",
                2 => "Cylinder",
                _ => "",
            };
        }
    }
    public static explicit operator TrueCollider(int type){if(Enum.IsDefined(typeof(Colliders), type)){return new TrueCollider(type);}else{return TrueCollider.Cube;}}
    public static explicit operator int(TrueCollider tC){return tC.type;}
    public static implicit operator Colliders(TrueCollider tC){ return (Colliders)tC.type; }
    public static implicit operator TrueCollider(Colliders c){
        (int, int) Dimensions = c switch{
            Colliders.Cube => (10, 10),
            Colliders.Sphere => (10, 10),
            Colliders.Capsule => (10, 5),
            _ => (0, 0)
        };
        return new TrueCollider((int)c, Dimensions.Item1, Dimensions.Item2);
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
static class CustomFunctions{
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
                                CustomFunctions.SwapItems(points, cc, cc_);
                                break;
                            }
                        }while((points[cc_].Y > points[cc_+1].Y) | cc_< points.Length);
                    }


                    //Swap Xs next
                    if((points[cc].X > points[cc+1].X) && (points[cc].Y == points[cc+1].Y)){
                        swapped = true;
                        CustomFunctions.SwapItems(points, cc, cc+1);
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
                if(CustomFunctions.GetDistance(PointF.Empty, points[cc]) > CustomFunctions.GetDistance(PointF.Empty, points[cc+1])){
                    swapped = true;
                    CustomFunctions.SwapItems(points, cc, cc+1);
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
                if(CustomFunctions.GetDistance(Point.Empty, points[cc]) > CustomFunctions.GetDistance(Point.Empty, points[cc+1])){
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
                    CustomFunctions.SwapItems(points, cc, cc+1);
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
                    CustomFunctions.SwapItems(points, cc, cc+1);
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
        T[] values = [.. items];
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
    /// <summary>Convert an array to a string</summary>
    /// <typeparam name="T">The Type of the array's elements.</typeparam>
    /// <param name="array">The array to be converted.</param>
    /// <param name="AddCommas">Should it add Commas to the string? when false essentially returns a raw string.</param>
    /// <returns>A string containing <paramref name="array"/>'s elements.</returns>
    public static string ToString<T>(IEnumerable<T> array, bool AddCommas = true){
        if(array == null){ return string.Empty;}
        StringBuilder sb = new();
        bool first = true;
        foreach (T item in array){
            if (!first && AddCommas) { sb.Append(", "); }
            sb = sb.Append(item?.ToString());
            first = false;
        }
        return sb.ToString();
    }
    public static T[] GetTupleArrayT<T, R>(IEnumerable<(T, R)> data){
        T[] values = new T[data.Count()];
        int cc = 0;
        foreach ((T t, R r) item in data) {
            values[cc] = item.t;
            cc++;
        }
        return values;
    }
    public static R[] GetTupleArrayR<T, R>(IEnumerable<(T, R)> data){
        R[] values = new R[data.Count()];
        int cc = 0;
        foreach ((T t, R r) item in data) {
            values[cc] = item.r;
            cc++;
        }
        return values;
    }
    public static IEnumerable<byte> ToBytes(string s){
        List<byte> array = new List<byte>();
        foreach(char c in s){
            array.AddRange(BitConverter.GetBytes(c));
        }
        return array;
    }
    public static bool AnyAt<T>(T[] array, int index){
        try{
            _ = array[index];
            return true;
        }catch(IndexOutOfRangeException){
            return false;
        }
    }
    public static string IsolateWord(string str, int StartFrom = 0){
        StringBuilder sB = new();
        for(int cc =StartFrom; cc < str.Length;cc++){
            if(str[cc] != ' '){
                while(str[cc] != ' '){
                    sB.Append(str[cc]);
                    cc++;
                }
                return sB.ToString();
            }
        }
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
[Serializable]
public class ParameterNotFoundException : Exception{
    public ParameterNotFoundException(string message) : base(message){}
    public ParameterNotFoundException(string message, Exception inner) : base(message, inner){}
    public ParameterNotFoundException(){}
    public override string Message => base.Message;
    public static void ThrowIf(bool condition, string message, Exception? InnerException = null){
        if(condition){throw new InconsistentDimensionException(message, inner: InnerException ?? new Exception());}
    }
}
[Serializable]
public class JsonFormatexception : Exception{
    public JsonFormatexception(string message) : base(message){}
    public JsonFormatexception(string message, Exception inner) : base(message, inner){}
    public JsonFormatexception(){}
    public override string Message => base.Message;
    public static void ThrowIf(bool condition, string message, Exception? InnerException = null){
        if(condition){throw new InconsistentDimensionException(message, inner: InnerException ?? new Exception());}
    }
}

/// <summary>
/// Represents a job that runs within a lock to prevent deadlocks.
/// This works by running the job and then calling the OnFinish delegate to complete the job.
/// However if the job takes too long, the OnTimeout delegate is called.
/// </summary>
/// <remarks>THIS CLASS MAY BE REWORKED.</remarks>
class BackdoorJob<T, R>{
    Type paramType { get; set; }
    Type returnType { get; set; }
    /// <summary>
    /// The delegate that represents a job.
    /// </summary>
    /// <typeparam name="_T">The parameters for the delegate, for multiple parameters make this a tuple.</typeparam>
    /// <typeparam name="_R">The return type for the delegate.</typeparam>
    public delegate _R BackdoorDelegate<_T, _R>(_T obj);
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
	BackdoorDelegate<T, R> Job;
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


    BackdoorJob(int Timeout, BackdoorDelegate<T, R> job, Action? OnFinish = null, OnTimeout? @OnTimeout = null, object? sender = null) {
        this.paramType = typeof(T);
        this.returnType = typeof(R);
        if(OnFinish == null){
            this.OnFinish = TrueFinish;
        }else{
            this.OnFinish = OnFinish;
            this.OnFinish += TrueFinish;
        }
        if (OnTimeout == null){
            this.OnTimeExceed = TrueTimeout;
        }else{
            this.OnTimeExceed = OnTimeout;
            this.OnTimeExceed += TrueTimeout;
        }
        this.Job = job;
        this.Timeout = Timeout;
        this.cts = new CancellationTokenSource();
        result = null;
        this.sender = sender ?? this.GetType();
        this.cts.Token.Register(() => { this.OnTimeExceed(this.sender ?? new object()); });
    }
    private R? _Start(T param) {
        this.running = true;
        cts.CancelAfter(Timeout * 1000);
        R? result = this.Job(param);
        this.OnFinish();
        return result;
    }
    void TrueFinish() {
        try {
            this.Dispose();
        } catch (Exception ex) {
            _ = MessageBox.Show($"Error in LockJob: ObjectDisposalException.\nMessage: {ex.Message}\nFrom sender: {(string)(sender ?? "ERROR: Undefined")}");
        }
        this.running = false;
    }
    void TrueTimeout(object sender) {
        _ = MessageBox.Show($"Error in LockJob: TimeoutException.\nFrom sender: {(string)sender}");
    }

    IAsyncResult? result;
    public void Dispose(bool disposing = true) {
        if (!(disposed && running)) {
            if (disposing) {
                this.running = false;
                this.Job.EndInvoke(result);
            }
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is BackdoorJob<T, R> other) {
            return _ID == other._ID;
        }
        return false;
    }
    void ChandeID(int ID){this._ID = ID;}

    public static bool operator ==(BackdoorJob<T, R> l1, BackdoorJob<T, R> l2) { return l1.Equals(l2); }
    public static bool operator !=(BackdoorJob<T, R> l1, BackdoorJob<T, R> l2) { return !(l1 == l2); }
    public override int GetHashCode() { return HashCode.Combine(_ID); }

    /// <summary>
    /// A static class that controls how BackdoorJobs are handled, it does this through a <see cref="System.Collections.IEnumerable"/> that stores the jobs and 
    /// a <see cref="System.Collections.IEnumerable"/> that stores job parametwers locally.
    /// </summary>
    public static class BackdoorJobHandler{
        static enum JobProtocol{
            ///<summary>The job will be cached with it's parameters (and return value when it has been run), allowing it to be run at anytime.</summary>
            /// <remarks>For jobs that are run frequently.</remarks>
            FullCache = 0,
            ///<summary>The job will be cached with it's parameters, but not the return value.</summary>
            /// <remarks>This is for jobs that do not return a value but require parameters, such as those that only change states or values according to what is passed to them.</remarks>
            CacheParam = 1,
            ///<summary>The job will be cached with it's return value, but not the parameters.</summary>
            /// <remarks>This is for jobs that return a value, but do not require parameters.</remarks>
            CacheReturn = 2,
            ///<summary>The job will not be cached, it will be run immediately and then will be erased, as well as it's return value.</summary>
            /// <remarks>This is for jobs that do not require caching, such as those that only change states or values.</remarks>
            RunErase = 3,
            ///<summary>The job will not be cached, it will be run immediately and then will be erased, but the return value will be returned.</summary>
            /// <remarks>This is for jobs that do not require caching, but do return a value.</remarks>
            InstantRunReturn = 4,

        }
        static List<(JobPotocol jP, Type ParamType, Type ReturnType, object Job)>? jobs;
        ///<summary>
        /// Stores a dismantled BackDoorJob.
        /// Stors it with the relevant info needed to regenerate it.
        /// </summary>
        /// <remarks>
        /// <see cref="jobs_"/>.methods[0] = _Start,
        /// <see cref="jobs_"/>.methods[0] = OnFinish,
        /// <see cref="jobs_"/>.methods[0] = ,
        /// </remarks>
        static List<(JobProtocol jP, object sender, int timeout, int ID, MethodInfo[] methods)> jobs_;
        static List<(Type t, object Data)> Parameters;
        ///<summary>Store a job in <see cref="jobs"/>, optionally including it's parameters and how the return type should be regarded, maybe the process sho.</summary>
        public static _R? QueueJob<_T, _R>(BackdoorJob<_T, _R> job, JobPotocol jP, _T? params)where _R : Nullable{
            switch(jP){
                case JobProtocol.FullCache | JobProtocol.CacheParam | JobProtocol.CacheReturn:
                    if(params == null){
                        throw new ArgumentNullException(nameof(params), $"Parameters cannot be null for {Enum.GetName(jP)} jobs.");
                    }else{
                        jobs.Add(jP, _T, _R, job);
                        Parameters.Add(params);
                    }
                    return null;
                case RunErase:
                    _ = job.Start(params);
                    return null;
                case InstantRunReturn:
                    return job._Start(params);
            }
        }
        int Pointer= 0;
        public static void ProcessJob(ElapsedEventArgs args, object sender){
            if (jobs == null || jobs.Count == 0) { return default; }
            if (Pointer >= jobs.Count) { Pointer = 0; }
            
        }



        static (Type r, object data)[] ReturnResult = new (Type r, object data)[20];
        static int Pointer;
        static void AddNextResult(Type t, object data) {
            Pointer++;
            if (Pointer >= 20) { Pointer = 0; }
            lock(ReturnResult){ReturnResult[Pointer] = (t, data);}
        }
        static Result? GetMyResult<Result>(int timeout = 1000){
            CancellationTokenSource cts = new();
            cts.CancelAfter(timeout);
            for(int cc =0; cc< ReturnResult.Length; cc++){
                WaitResult<Result>(cc);
            }
            return default;
        }
        /// <summary>Watch a point in <see cref = "ReturnResult"/> until Timeout.</summary>
        /// <typeparam name="Result">The type that should be watched for.</typeparam>
        /// <param name="WatchIndex">The index of <see cref="ReturnResult"/> that the type should be looked for in.</param>
        /// <param name="timeOut">How long should the function wait for, looking at the specified index of <see cref="ReturnResult"/>.</param>
        /// <returns>If the expected Result type was found then return true, if Timeout was reached return false.</returns>
        static bool WaitResult<Result>(int WatchIndex, int timeOut = 20){
            CancellationTokenSource cts = new();
            cts.CancelAfter(timeOut);
            while(!cts.IsCancellationRequested && ReturnResult[WatchIndex].GetType() != typeof(Result)){
                Task.Delay(100);
            }
            return !cts.IsCancellationRequested && ReturnResult[WatchIndex].GetType() == typeof(Result);
        }
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
        static t? RecoverTrueParameters<t, r>() {
            int iD = RecoverParameterIndex(typeof(t), typeof(r));
            if (iD == -1) { return default(t); }
            return (t)parameterData[iD].data;
        }
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
        static int RecoverParameterIndex(Type T, Type R) {
            int cc = 0;
            foreach (((Type paramType, Type returnType) Types, object data) item in parameterData) {
                if (item.Types.paramType == T && item.Types.returnType == R) { return cc; }
                Task.Delay(100);
                cc++;
            }
            return -1;
        }
    }
}

public class EncryptionKey{
    public static EncryptionKey Empty = new();
    static EncryptionKey(){
        _masterKey = new EncryptionKey(AppDomain.CurrentDomain.BaseDirectory + "Cache/KeyData");
    }
/// <summary>The master key for this application.</summary>
    public static EncryptionKey masterKey{get{
        if(_masterKey == null){
            _masterKey = new EncryptionKey(AppDomain.CurrentDomain.BaseDirectory + "Cache/KeyData");
        }
        return _masterKey;
    }}
    static EncryptionKey? _masterKey;
    public int key_{get; private set;}
    byte[] scrambleCode;
    int ScramblePerDigit;
    EncryptionKey(){ this.key_ = 0; this.ScramblePerDigit = 0; this.scrambleCode = []; }
/// <summary>
/// Constructs a key from a Key.key and Key.mtf file.
/// </summary>
/// <param name="keysPath">The Directory where the Keys are stored.</param>
/// <exception cref="TypeInitializationException">If the Key.key or Key.mtf were not found.</exception>
    public EncryptionKey(string keysPath){
        if(Directory.Exists(keysPath)){
            DirectoryInfo di = new DirectoryInfo(keysPath);
            FileInfo[] finfo = [new(System.IO.Path.Combine(keysPath, "Key.key")), new(System.IO.Path.Combine(keysPath, "Key.mtf"))];
            if(!finfo[0].Exists){throw new TypeInitializationException(nameof(EncryptionKey), new FileNotFoundException($"The file Key.key was not found at {keysPath}"));}
            if(!finfo[0].Exists){throw new TypeInitializationException(nameof(EncryptionKey), new FileNotFoundException($"The file Key.mtf was not found at {keysPath}"));}
            int[] keyBytes = new int[finfo[0].Length];
            byte[] keyScrambler = new byte[finfo[1].Length];
            int ScramblePerIncrement;
            if(finfo[1].Length % finfo[0].Length == 0){
                ScramblePerIncrement = (int)(finfo[1].Length / finfo[0].Length);
            }else{throw new TypeInitializationException(nameof(EncryptionKey), new ArgumentOutOfRangeException(System.Reflection.ConstructorInfo.ConstructorName));}
            using(BinaryReader keyStream = new BinaryReader(File.OpenRead(System.IO.Path.Combine(keysPath, "Key.key")))){
                for(int i =0; i < finfo[0].Length; i++){keyBytes[i] = keyStream.ReadByte();}
                for(int i =0; i < finfo[1].Length; i++){keyScrambler[i] = keyStream.ReadByte();}
                keyStream.Dispose();
            }
            int i_ = 1;
            int buffer =0;
            for(int i = 0; i < finfo[0].Length; i++, i_+= ScramblePerIncrement){
                if(keyScrambler[i_] > keyScrambler[0]){
                    //Bit Shift Right.
                    for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(keyBytes[i] + buffer)) >> keyScrambler[cc];}
                }else{
                    //Bit Shift Left.
                    for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(keyBytes[i] - buffer)) << keyScrambler[cc];}
                }
            }
            this.ScramblePerDigit = ScramblePerIncrement;
            this.scrambleCode = keyScrambler;
            this.key_ = buffer;
        }else{
            this.key_ = int.MinValue;
            this.scrambleCode = [];
            this.ScramblePerDigit = 0;
        }
    }
    public EncryptionKey(int[] key, byte[] scrambleCode){
        if(scrambleCode.Length % key.Length != 0){throw new ArgumentOutOfRangeException(nameof(scrambleCode), "The scramble array must be divisible by the key length.");}
        int ScramblePerIncrement = scrambleCode.Length/key.Length;
        int buffer =0;
        int i = 0;
        for(int i_ = 1; i_ < key.Length;i++, i_+= ScramblePerIncrement){
            for (int cc = i_; cc < (ScramblePerIncrement + i_ > scrambleCode.Length? scrambleCode.Length: ScramblePerIncrement + i_); cc++){
                if (scrambleCode[cc] > scrambleCode[0]){
                    //Bit Shift Right.
                    buffer += (key[i] - buffer) >> Math.Clamp(scrambleCode[cc], byte.MinValue, byte.MaxValue);
                }else{
                    //Bit Shift Left.
                    buffer -= (key[i] + buffer) << Math.Clamp(scrambleCode[cc], byte.MinValue, byte.MaxValue);
                }
            }
            i++;
        }
        this.ScramblePerDigit = ScramblePerIncrement;
        this.scrambleCode = scrambleCode;
        this.key_ = buffer;
    }
    public static int ScrambleValue(int value, byte scramble){
        // XOR then rotate left by scramble bits
        return (value ^ scramble) << (scramble % 32) | (value ^ scramble) >> (32 - (scramble % 32));
    }
    public static int DescrambleValue(int encoded, byte scramble){
        // Reverse the rotation, then XOR
        int rotated = (encoded >> (scramble % 32)) | (encoded << (32 - (scramble % 32)));
        return rotated ^ scramble;
    }
    /// <summary>Creates an encoded key from a key object.</summary>
    /// <param name="key">The key to have it's data decoded.</param>
    /// <returns>A decoded Key object.</returns>
    public static int[] DecodeKey(EncryptionKey key) => DecodeKey(key.key_, key.scrambleCode, key.ScramblePerDigit);
/// <summary>Creates an encoded key from a key and a scramble array.</summary>
/// <param name="key">The key to be encoded.</param>
/// <param name="scrambleArray">The encoding array that the key is encoded by.</param>
/// <param name="ScrambleCode">The integer that helps the algorithm decide whether to Bit shift forward or Backward.</param>
/// <param name="ScramblePerDigit">the amount of scrambling that will occur per digit.</param>
/// <returns>An integer array that encodes the key.</returns>
/// <exception cref="ArgumentOutOfRangeException">If the ScramblePerDigit and the ScrambleArray are not compatible.</exception>
/// <remarks>The ScrambleArray and the <see cref="return"/> must be retained for the original key to be restored.</remarks> 
    public static int[] DecodeKey(int key, byte[] scrambleArray, int ScramblePerDigit = 4){
        int ScrambleCode = scrambleArray[0];
        if (scrambleArray.Length % ScramblePerDigit != 0) { throw new ArgumentOutOfRangeException(nameof(scrambleArray), "The scramble array must be divisible by the ScramblePerDigit."); }
        int[] TrueKey = new int[scrambleArray.Length/ScramblePerDigit];
        if(!(ScrambleCode <= 0 | ScrambleCode > byte.MaxValue))ScrambleCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, byte.MaxValue);

        int i_ = scrambleArray.Length;
        for(int i = TrueKey.Length - 1; i >= 0; i--, i_ -= ScramblePerDigit){
            for(int cc = i + ScramblePerDigit; cc > i; cc--){
                if (scrambleArray[cc] > ScrambleCode){
                    // Force Bit Shift Right#
                    // key -= (key[i] - key) >> Math.Clamp(scrambleCode[cc], byte.MinValue, byte.MaxValue);
                    // key[i] = ??
                    // (key << Math.Clamp(scrambleCode[cc], byte.MinValue, byte.MaxValue)) + key -= key[i]
                    // key[i] += (key << Math.Clamp(scrambleCode[cc], byte.MinValue, byte.MaxValue)) + key
                    TrueKey[i] -= (key << Math.Clamp(scrambleArray[cc], byte.MinValue, byte.MaxValue)) + key;
                }else{
                    // Force Bit Shift Left
                    TrueKey[i] += (key >> Math.Clamp(scrambleArray[cc], byte.MinValue, byte.MaxValue)) - key;
                }
            }
        }
        return TrueKey;
    }
    public override bool Equals(object? obj){
        EncryptionKey? k = obj as EncryptionKey;
        if (k is not null){
            return this.GetHashCode() == k.GetHashCode();
        }else{ return false; }
    }
    public override int GetHashCode() { return HashCode.Combine(this.GetType(), CustomFunctions.ToString(this.scrambleCode), this.key_ + this.ScramblePerDigit); }
    public static unsafe bool operator ==(EncryptionKey? k1, EncryptionKey? k2){if((object?)k1 != null){return k1.Equals(k2);}else if((object?)k1 == null && (object?)k2 != null){ return false; }else{ return true; }}
    public static bool operator !=(EncryptionKey k1, EncryptionKey k2){return !(k1.key_ == k2.key_);}
}
