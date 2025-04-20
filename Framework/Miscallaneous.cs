struct Equation{
    public float XCoff;
    public float Y_intercept;
    public Equation(float X_coefficient, float Y_intercept){
        this.XCoff = X_coefficient;
        this.Y_intercept = Y_intercept;
    }
    public float SolveY(float X){
        return (X * XCoff) + Y_intercept;
    }
    public float SolveX(float Y){
        return (Y - Y_intercept)/XCoff;
    }
    public static Equation FromPoints(Point a, Point b){
        //Simultaenous equation.
        float gradient = (a.Y - b.Y)/(a.X - b.X);
        float y_intercept = a.Y - (a.X * gradient);
        return new Equation(gradient, y_intercept);
    }
    /// <summary>
    /// Finds the x-coordinate where both Equation a and b equal the same y-coordinate.
    /// </summary>
    /// <returns>A Point detailing the x and y coordinates where x and y equal each other.</returns>
    public static Point WhereEquationEquals(Equation a, Equation b){
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
        Point p = new Point();
        p.Y = (int)((b.Y_intercept - (b.XCoff * a.Y_intercept/a.XCoff))/(1 - (b.XCoff/a.XCoff)));
        p.X = (int)a.SolveX(p.Y);
        return p;
    }
}

class Camera{
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
	/// <summary>
	/// The resolution at which textures are applied to a mesh
	/// </summary>
	public int Resolution;
    /// <summary>
    ///  The tolerance for how small a polygon at the boundary of the viewport can be clipped before it's simply deleted.
    /// </summary>
    public float clipTol;
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
    public static Form? form{get{return Form.ActiveForm;}}
    public Camera(float Fov = 15f, Vector3? pos = null, Vector3? rot = null){
        this.far = Fov;
        this.Position = pos == null? Vector3.zero: pos.Value;
        this.Rotation = pos == null? Vector3.zero: pos.Value;
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

abstract class ITextureEffect{
    public abstract int iterations{get; set;}
    public abstract TextureDatabase Apply(TextureDatabase data);
}
class Slide : ITextureEffect{
    //Acts as a memory store to allow for a reversing of the operation, to 
    public override int iterations{get; set;}
    Point sD;
    Point slideDirection{
        get{return sD;} 
        set{
            float Magnitude = (float)Math.Sqrt(value.X^2 + value.Y^2);
            sD = new Point((int)(value.X/Magnitude), (int)(value.Y/Magnitude));
        }}
    public override TextureDatabase Apply(TextureDatabase data){
        for(int cc = 0;cc < data.Count;cc++){
            data[cc] = (new Point(data[cc].p.X + (data[cc].p.X * slideDirection.X), data[cc].p.Y + (data[cc].p.Y * slideDirection.Y)), data[cc].c);
        }
        return data;
    }
}
