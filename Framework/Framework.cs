using System.Numerics;

/// <summary>
///  A 3-dimensional point and rotation.
/// </summary> 
struct Vector3{
    /// <summary>
    ///  This property represents a Vector, which has it's x, y and z values set to 0.
    /// </summary> 
    public static readonly Vector3 zero = new Vector3();
    /// <summary>
    ///  This property represents a Vector, which has it's x, y and z values set to Float.infinity.
    /// </summary> 
    public static readonly Vector3 infinity = new Vector3(float.PositiveInfinity);
    /// <summary>
    ///  This property represents a Vector, which has it's x, y and z values set to Float.negativeInfinity.
    /// </summary> 
    public static readonly Vector3 negativeInfinity = new Vector3(float.NegativeInfinity);
    public static readonly Vector3 Up = new Vector3(0, 1, 0);
    public static readonly Vector3 Forward = new Vector3(0, 0, 1);
    public static readonly Vector3 Right = new Vector3(1, 0, 0);


    /// <summary>
    ///  The X co-ordinate of the Vector, (The Horizontal axis).
    /// </summary> 
    public float X{get; set;}
    /// <summary>
    ///  The Y co-ordinate of the vector, (The Vertical axis).
    /// </summary> 
    public float Y{get; set;}
    /// <summary>
    ///  The Z co-ordinate of the vector, (The Forward/Backward axis).
    /// </summary> 
    public float Z{get; set;}
    /// <summary>This is the length of this Vector3.</summary>
    public float Magnitude{get{return (float)Math.Sqrt((this.X*this.X) +(this.Y*this.Y) +(this.Z*this.Z));}}
    

    /// <summary>
    ///  This ctor initalises a copy of V.
    /// </summary>
    /// <param name="V">The original vector3.</param>
    public Vector3(Vector3 V){
        this = V;
    }
    ///<summary>
    /// This ctor creates a Vector3 from a series of bytes.
    ///</summary>
    public Vector3(byte[] bytes){
        if(bytes.Length != sizeof(int)*3){
            this = Vector3.zero;
        }else{
            byte[] buffer = new byte[sizeof(int)];
            int i = 0;
            while(i < 3){
                for(int cc=0;cc<sizeof(int);cc++){
                    buffer[cc] = bytes[cc+(i*sizeof(int))];
                }
                switch(i){
                    case 0:
                        this.X = BitConverter.ToInt32(buffer);
                        break;
                    case 1:
                        this.Y = BitConverter.ToInt32(buffer);
                        break;
                    case 2:
                        this.Z = BitConverter.ToInt32(buffer);
                        break;
                }
            }
        }
    }
    /// <summary>
    ///  This constructor takes three arguments to assign to the x, y, and z co-ordinates(respectively).
    /// </summary> 
    /// <param name="x"> This maps the X co-ordinate of the Vector. </param>
    /// <param name="y"> This maps to the Y co-ordinate of the Vector. </param>
    /// <param name="z"> This maps to the Z co-ordinate of the Vector. </param>
    public Vector3(float x, float y, float z){
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    /// <summary>
    ///  This constructor assigns all co-ordinates with the single float parameter.
    /// </summary> 
    /// <param name="a"> The float that all the co-ordinats will be assigned to. </param>
    public Vector3(float num){
        this.X = num;
        this.Y = num;
        this.Z = num;
    }
    /// <summary>
    ///  This constructor assigns the Vector co-ordinates with random values (If the boolean parameters are true).
    /// </summary> 
    /// <param name="rng"> This boolean is the condition for whether the output will be a Vector with random values or 0s. </params>
    public Vector3(bool rng = false){
        Random rng_ = new();
        if(rng){
            this.X = rng_.Next();
            this.Y = rng_.Next();
            this.Z = rng_.Next();
        }
        
    }

    public void Normalise(){
        this/=this.Magnitude;
    }
    /// <summary>
    ///  Rotate this vector around Origin, by Rotation.
    /// </summary>
    /// <param name="Rotation">The value that this vector will be rotated by.</param>
    /// <param name="Origin">The point this vector will rotate around.</param>
    /// <returns>This method returns a copy of the value.</returns>
    /// <remarks>This both sets this vector to the rotated version and returns a copy.</remarks>
    public Vector3 RotateAround(Vector3 Rotation, Vector3 Origin){
        Vector3 Offset = (this-Origin).Abs();
        Vector3 Rot = Vector3.GetRotation(this, Origin);
        this = new Vector3((float)(Offset.X/(Math.Sin(180-Rot.X)/2)),
        (float)(Offset.Y/(Math.Sin(180-Rot.Y)/2)),
        (float)(Offset.Z/(Math.Sin(180-Rot.Z)/2)));
        return this;
    }
    public Vector3 Abs(){
        this.X = Math.Abs(this.X);
        this.Y = Math.Abs(this.Y);
        this.Z = Math.Abs(this.Z);
        return this;
    }
    public byte[] ToBytes(){
        List<byte> result = [.. BitConverter.GetBytes(this.X)];
        result.AddRange(BitConverter.GetBytes(this.Y));
        result.AddRange(BitConverter.GetBytes(this.Z));
        return result.ToArray();
    }
    public Vector3 GetNormal(){
        Vector3 me = this;
        me.Normalise();
        return new Vector3(this.X/me.X, this.Y/me.Y, this.Z/me.Z);
    }
    public void Tangent(){
        this.X = (float)Math.Tan(this.X);
        this.Y = (float)Math.Tan(this.Y);
        this.Z = (float)Math.Tan(this.Z);
    }
    public Point ToPoint(){
        return new Point((int)(this.X/this.Z), (int)(this.Y/this.Z));
    }
    public List<float> ToList(){
        return new List<float>(){this.X, this.Y, this.Z};
    }
    ///<summary>
    /// Get the distance from 2 vectors, as a float;
    ///</summary>
    public static float GetDistance(Vector3 a, Vector3 b){
        float xDif = Math.Abs(a.X - b.X);
        float yDif = Math.Abs(a.Y - b.Y);
        float zDif = Math.Abs(a.Z - b.Z);
        double xy = xDif/Math.Cos(Math.Atan(yDif/xDif));
        double xz = xDif/Math.Cos(Math.Atan(zDif/xDif));
        double yz = zDif/Math.Cos(Math.Atan(yDif/zDif));
        return (float)(xy+xz+yz)/3;
    }
    ///<summary>Get the rotation from point a to b, with the apex being between them.</summary>
    public static Vector3 GetRotation(Vector3 a, Vector3 b){
        Vector3 origin = Vector3.CProduct(a, b);
        float A = Vector3.GetDistance(a, origin);
        float B = Vector3.GetDistance(b, origin);
        if(A > B){a = (a-b)/b;}else if(A < B){b = (b-a)/a;}
        float xDif = Math.Abs(a.X - b.X);
        float yDif = Math.Abs(a.Y - b.Y);
        float zDif = Math.Abs(a.Z - b.Z);
        double xy = Math.Atan(yDif/xDif);
        double xz = Math.Atan(zDif/xDif);
        double yz = Math.Atan(yDif/zDif);
        return new Vector3((float)xy, (float)xz, (float)yz);
    }
    public static Vector3 GetRotation(Polygon p){
        return GetRotation(p.A, p.B, p.C);
    }
    public static Vector3 GetRotation(Vector3 a, Vector3 b, Vector3 c){
        float A = Vector3.GetDistance(a, c);
        float B = Vector3.GetDistance(b, c);
        if(A > B){a = (a-b)/b;}else if(A < B){b = (b-a)/a;}
        float xDif = Math.Abs(a.X - b.X);
        float yDif = Math.Abs(a.Y - b.Y);
        float zDif = Math.Abs(a.Z - b.Z);
        double xy = Math.Atan(yDif/xDif);
        double xz = Math.Atan(zDif/xDif);
        double yz = Math.Atan(yDif/zDif);
        return new Vector3((float)xy, (float)xz, (float)yz);
    }
    public static Vector3 CProduct(Vector3 a, Vector3 b){
        return new Vector3((a.Y*b.Z)-(a.Z*b.Y), (a.Z*b.X)-(a.X*b.Z), (a.X*b.Y)-(a.Y*b.X));
    }
    public static float DProduct(Vector3 a, Vector3 b){
        a.Normalise();
        b.Normalise();
        return (a.X*b.X)+(a.Y*b.Y)+(a.Z*b.Z);
    }
    ///<summary>
    /// a to the power of b.
    ///</summary>
    public static Vector3 operator ^(Vector3 a, int b){
        Vector3 result = a;
        for(int cc = 0;cc < b; cc++){
            result.X *= a.X;
            result.Y *= a.Y;
            result.Z *= a.Z;
        }
        return result;
    }
    ///<summary>
    /// each of a's axis is multiplied by b's axis.
    ///</summary>
    public static Vector3 operator *(Vector3 a, Vector3 b){
        a.X *= b.X;
        a.Y *= b.Y;
        a.Z *= b.Z;
        return a;
    }
    public static Vector3 operator *(Vector3 a, float b){
        a.X *= b;
        a.Y *= b;
        a.Z *= b;
        return a;
    }
    public static Vector3 operator *(float b, Vector3 a){
        a.X *= b;
        a.Y *= b;
        a.Z *= b;
        return a;
    } 
    public static Vector3 operator /(Vector3 a, Vector3 b){
        a.X /= b.X;
        a.Y /= b.Y;
        a.Z /= b.Z;
        return a;
    } 
    public static Vector3 operator /(Vector3 a, float b){
        a.X /= b;
        a.Y /= b;
        a.Z /= b;
        return a;
    }
    public static Vector3 operator -(float a, Vector3 b){
        b.X = a - b.X;
        b.Y = a - b.Y;
        b.Z = a - b.Z;
        return b;
    }
    public static Vector3 operator -(Vector3 a, Vector3 b){
        a.X -= b.X;
        a.Y -= b.Y;
        a.Z -= b.Z;
        return a;
    }
    public static Vector3 operator +(Vector3 a, Vector3 b){
        a.X += b.X;
        a.Y += b.Y;
        a.Z += b.Z;
        return a;
    } 
    public static bool operator ==(Vector3 a, Vector3 b){
        if(!(a.X == b.X && a.X == b.Y && a.Z == b.Z)){
            return false;
        }else{
            return true;
        }
    }
    public static bool operator !=(Vector3 a, Vector3 b){
        if(!(a.X == b.X && a.X == b.Y && a.Z == b.Z)){
            return true;
        }else{
            return false;
        }
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()){
            return false;
        }
        return base.Equals (obj);
    }
    
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
struct Polygon{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector3 origin{get{
        return (A+B+C)/3;
    }}
    public float Area{get{
        //Get rotation from a to b around c
        //use rotation and length of AC and BC
        //Use 1/2(a*b)*Sin(Rotation) to get the area
        float ac = Vector3.GetDistance(this.A, this.C);
        float bc = Vector3.GetDistance(this.B, this.C);
        float ab = Vector3.GetRotation(this).Magnitude;
        return (float)(.5f * ac * bc * Math.Sin(ab));
    }}
    public Point[] UVPoints{get; private set;}
    public void UpdateTexture(Point[] uv){this.UVPoints = uv;}
    public Vector3 Normal{
        get{
            Vector3 a = Vector3.CProduct(this.A, this.B);
            Vector3 b = Vector3.CProduct(this.A, this.C);
            return Vector3.CProduct(a, b);}
    }
    public Vector3 Rotation{get{
        Vector3 a = Vector3.GetRotation(A, origin);
        a.Tangent();
        Vector3 b = Vector3.GetRotation(A, origin);
        b.Tangent();
        Vector3 c = Vector3.GetRotation(C, origin);
        c.Tangent();
        return (a*b)*c;
    }
    }
    public Polygon(Vector3 a, Vector3 b, Vector3 c){
        this.A = a;
        this.B = b;
        this.C = c;
    }
    public Polygon(){
        this.A = new Vector3();
        this.B = new Vector3();
        this.C = new Vector3();
    }
    public static Polygon PolyClip(Polygon target, Vector3 focus, float Range){
        (bool a, bool b, bool c) item_ = (true, true, true);
        (float aLength, float bLength, float cLength) item = (Vector3.GetDistance(target.A, focus), Vector3.GetDistance(target.B, focus), Vector3.GetDistance(target.C, focus));
        //Finds the Length closest to the focus and sets it to false
        if(item.aLength < item.bLength){
            if(item.aLength < item.cLength){
                item_.a = false;
            }else if(item.aLength == item.cLength){
                item_.a = false;
                item_.c = false;
            }
        }else if(item.aLength == item.bLength){
            item_.a = false;
            item_.b = false;
        }else{
            if(item.bLength < item.cLength){
                item_.b = false;
            }else if(item.bLength == item.cLength){
                item_.b = false;
                item_.c = false;
            }else{
                item_.c = false;
            }
        }
        target.A /= item_.a && item.aLength < Range? 1:(item.aLength-Range)/item.aLength;
        target.B /= item_.b&& item.bLength < Range? 1:(item.bLength-Range)/item.bLength;
        target.C /= item_.c&& item.cLength < Range? 1:(item.cLength-Range)/item.cLength;
        return target;
    }
    public void Translate (Vector3 PrePosition, Vector3 Position, Vector3 Rotation){
        // This will orient a child node by to its parent
        A.RotateAround(PrePosition, Rotation);
        A += Position;

        B.RotateAround(PrePosition, Rotation);
        B += Position;

        C.RotateAround(PrePosition, Rotation);
        C += Position;
    }
    public float Furthest(Vector3 origin){
        float result;
        float a = Vector3.GetDistance(A, origin);
        float b = Vector3.GetDistance(B, origin);
        float c = Vector3.GetDistance(C, origin);
        if(a>b){
            result = a>c?a:c;
        }else{
            result = b>c?b:c;
        }
        return result;
    }
    public Color Shade(Light light){
        float dp = Vector3.DProduct(this.Normal, light.Source.GetNormal());
        return Color.FromArgb((int)(dp*(1/Vector3.GetDistance(this.origin, light.Source))));
    }


	public static Polygon operator +(Polygon a, Vector3 b){
		return new Polygon(a.A + b, a.B + b, a.C + b);
	}
	public static Polygon operator -(Polygon a, Vector3 b){
		return new Polygon(a.A - b, a.B - b, a.C - b);
	}
	/// <summary>
	///  Create a mesh.
	/// </summary>
	/// <param name="height">The height of the mesh.</param>
	/// <param name="width">Half the width of the mesh.</param>
    /// <param name="Bevel">The length at which the centre of the cross-section of the mesh meets the main length of the mesh</param>
	/// <returns>An array of polygons describing a mesh</returns>
	/// <remarks>This method programmatically generates a prism with the inputted number of sides.</remarks>
	public static Polygon[] Mesh(int Height =10, int bevel =0, int width =10, int sides =3){
		if(sides < 2){sides = 3;}
		List<Polygon> result = new List<Polygon>();
		Vector3 height = new Vector3(0, Height, 0);
		Vector3 PieceRotation = new Vector3(0, 0, 360/sides);
		Vector3 _buff = new Vector3(0, Height/2, width);
		for(int i = -width;i < sides;i++){
			Vector3 CSection = _buff;
			_buff.RotateAround(height/2, PieceRotation*i);
            //Add top face.
			result.Add(new Polygon(height/2, new Vector3(_buff.X, _buff.Y+_buff.Z-bevel, 0), new Vector3(_buff.X, _buff.Y-bevel, 0)));
            //Add cross-section.
			result.Add(new Polygon(new Vector3(_buff.X, _buff.Y-bevel, _buff.Z), new Vector3(CSection.X, CSection.Y-bevel, CSection.Z), CSection-height));
            //Add bottom face, adds the entry before the cross-section is added.
			result.Add(result[result.Count-1]-height);
		}
		return result.ToArray();
	}
}

/*Represents a 3-Dimensional object.*/
class gameObj{
    public string Name;
    internal delegate void Orient_(Vector3 PrePosition, Vector3 Position, Vector3 Rotation);
    public Orient_ orient_;
    public List<Polygon>? Children;
    public Vector3 Position;
    private Vector3 rotation;
    /// <summary>
    ///  The list containing all of this gameObject's components
    /// </summary>
    List<(Type ogType, Rndrcomponent rC)> components;
    public int compLength{get{return this.components.Count;}}
    public int Size{
        get{
            int Vects = sizeof(float)*6*Children.Count;
            int Comps = 0;
            for(int cc =0;cc < (this.components.Count == 0? -1: this.components.Count);cc++){
                Comps += components[cc].rC.ToByte().Length;
            }
            return Comps+Vects;
        }
    }
    public Vector3 Rotation{
        get{
            return this.rotation;
        }
        set{
            this.rotation = this.Children != null && this.Children.Count > 0? value: Vector3.zero;
        }
    }
    public int CollisionRange{
        get{
            if(Children != null){
                float result = Children[0].Furthest(this.Position);
                for(int cc = 1; cc<Children.Count;cc++){
                    result = result > Children[cc].Furthest(this.Position)? result: Children[cc].Furthest(this.Position);
                }
                return (int)(result+.5);
            }else{
                return 0;
            }
            
        }
    }


    public void AddComponent<RComponent>(Type type, RComponent rC) where RComponent : Rndrcomponent{
        components.Add((type, rC));
    }
    public void AddComponents<RComponent>(IEnumerable<(Type type, RComponent rC)> rC) where RComponent : Rndrcomponent{
        foreach((Type type, RComponent rc) rc in rC){
            components.Add(rc);
        }
    }
    /// <summary>
    /// Get the type of the component at index.
    /// </summary>
    public Type GetComponentType(int index){
        return this.components[index].ogType;
    }
    /// <summary>
    ///  This is an overload of GetComponent<Component>()
    /// </summary>
    /// <typeparam name="Component">This is the RndrCcomponent type that should be returned.</typeparam>
    /// <returns>Returns the 1st instance of the expected component.</returns>
    public Component? GetComponent<Component>() where Component : Rndrcomponent, new(){
        for(int cc = 0; cc < components.Count;cc++){
            if(this.components[cc].ogType == typeof(Component)){
                return (Component?)this.components[cc].rC;
            }
        }
        return null;
    }

    public void newTexture(string? path = null){
        Texturer? t = this.GetComponent<Texturer>();
        if(t == null){return;}else{
            t.Reset(path == null?t.file: path);
        }
    }
    public void UpdateTexture(int index, Point[] uv){
        this.Children[index].UpdateTexture(uv);
    }
    public Color[] Texture(int index){
        return this.GetComponent<Texturer>().Texture(this.Children[index]);
    }
    public void Translate(Vector3 position, Vector3 rotation, bool PrivateTranslation = false){
        this.Position += position;
        this.Rotation += rotation;
        if(!PrivateTranslation){
            this.orient_(this.Position - position, position, rotation);
        }
    }
    ///<summary>
    /// This method returns the Polygon closest to the Position parameter.
    ///</summary>
    ///<param name="Position">This is the parameter that the polygons will be compared against.</typeparam>
    ///<param name="LowerBd">This is the parameter that dictates how close polygons can get before the boolean isTouching is triggered.</typeparam>
    ///<returns>The polygon is the pPolygon that is closest to the contact position, whilst isTouching is whether the position is on the Polygon.</returns>
    public (Polygon polygon, bool isTouching) GetPolygon(Vector3 Position, float LowerBd = 0.5f){
        Polygon[] poly = new Polygon[this.Children.Count];
        Polygon scope = this.Children[0];
        float Dis = 0;
        for(int cc = 1; cc < poly.Length; cc++, Dis = Vector3.GetDistance(this.Children[cc].origin, Position)){
            if(Vector3.GetDistance(scope.origin, Position) < Dis){
                scope = this.Children[cc];
            }
        }
        Dis = Vector3.GetDistance(scope.origin, Position);
        return (scope, Dis <= LowerBd && Dis > 0? true: false);
    }


    public gameObj(Vector3 position, Vector3 rotation,IEnumerable<Point>? UVpoints = null, IEnumerable<Polygon>? children = null, List<Rndrcomponent>? Mycomponents = null, string? name = null){
        this.Position = position;
        if(children == null){
            this.orient_ = new Polygon().Translate;
        }else{
            this.Children = children.ToList();
            for(int cc = 0;cc < children.Count();cc++){
                this.orient_ += children.ElementAt(cc).Translate;
            }
        }
        if(UVPoints == null){
            this.UVPoints = UVpoints;
        }
        if(Mycomponents == null){
            this.components = new List<Rndrcomponent>();
        }else{
            this.components = Mycomponents;
        }
        this.Rotation = rotation;
        this.Name = name == null? $"{World.worldData.Count+1}": name;
        World.orientToWorld += this.Translate;
        World.worldData.Add(this);
    }
    

    public static gameObj operator +(gameObj parent, Polygon[] children){
        foreach(Polygon p in children){
            parent.Children.Add(p);
            parent.orient_ += p.Translate;
        }
        return parent;
    }
    public static gameObj operator +(gameObj parent, Polygon child){
        parent.Children.Add(child);
        parent.orient_ += child.Translate;
        return parent;
    }
    /// <summary>
    ///  Adding two gameObjs together only appends the child obj's children to the parent obj's children.
    /// </summary>
    /// <param name="parent">The gameObj that will recieve the children.</param>
    /// <param name="child">The gameObj that will have it's children copied to the parent.</param>
    public static gameObj operator +(gameObj parent, gameObj child){
        for(int cc = 0; cc < child.Children.Count;cc++){
            parent.Children.Add(child.Children[cc]);
            parent.orient_ += child.Children[cc].Translate;
        }

        return parent;
    }
    public static gameObj operator -(gameObj parent, Polygon child){
        try{
            //Use try because if the parent doesn't have the child it will catch,
            //if children is already empty or unassigned it will catch,
            parent.Children.Remove(child);
            parent.orient_ -= child.Translate;
        }
        finally{
            if(parent.Children != null && parent.Children.Count == 0){
                parent.Children = null;
            }
        }
        return parent;
    }
}
class Camera{
    public delegate void Orient_(Vector3 PrePosition, Vector3 Position, Vector3 Rotation);
    public Orient_ orientToCam;
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
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
    private static Form form;
    public static Form form_{
        get{
            return form;
        }
        set{
            Form? item_ = Form.ActiveForm;
            while(item_ == null){
                Task.Delay(500);
                item_ = Form.ActiveForm;
            }
            form = item_;
        }
    }
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