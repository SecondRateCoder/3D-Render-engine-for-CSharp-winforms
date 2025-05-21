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

abstract class ITextureEffect{
    public abstract int iterations{get; set;}
    public abstract TextureDatabase Apply(TextureDatabase data);
}
class Slide : ITextureEffect{
    //Acts as a memory store to allow for a reversing of the operation, to 
    public override int iterations{get; set;}
    PointF sD;
    PointF slideDirection{
        get{return sD;} 
        set{
            float Magnitude = (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
            sD = new PointF(value.X/Magnitude, value.Y/Magnitude);
        }}
    public override TextureDatabase Apply(TextureDatabase data){
        for(int cc = 0;cc < data.Count;cc++){
            data[cc] = (TextureDatabase.TexturePoint)(new PointF(data[cc].p.X + (data[cc].p.X * slideDirection.X), data[cc].p.Y + (data[cc].p.Y * slideDirection.Y)), data[cc].c);
        }
        return data;
    }
}
