using System.Diagnostics;

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
    ///<summary>
    /// The rotation to map a unto b, with the apex being a point of reference.
    ///</summary>
    ///<
    public static Vector3 GetRotation(Vector3 a, Vector3 b){
        float xDif = Math.Abs(a.X - b.X);
        float yDif = Math.Abs(a.Y - b.Y);
        float zDif = Math.Abs(a.Z - b.Z);
        double xy = Math.Atan(yDif/xDif);
        double xz = Math.Atan(zDif/xDif);
        double yz = Math.Atan(yDif/zDif);
        return new Vector3((float)xy, (float)xz, (float)yz);
    }
    public Vector3 Rotate(Vector3 Rotation){
        Vector3 rotated = (new Vector3(
            this.X,
            (float)(this.Y*(Math.Cos(Rotation.Y)-Math.Sin(Rotation.Y))),
            (float)(this.Z*(Math.Cos(Rotation.Z)+Math.Sin(Rotation.Z))))+
        new Vector3(
            (float)(this.X*(Math.Cos(Rotation.X)-Math.Sin(Rotation.X))),
            this.Y,
            (float)(this.Z*(Math.Cos(Rotation.Z)-Math.Sin(Rotation.Z))))+
        new Vector3(
            (float)(this.X*(Math.Cos(Rotation.X)-Math.Sin(Rotation.X))),
            (float)(this.Z*(Math.Cos(Rotation.Z)+Math.Sin(Rotation.Z))),
            this.Z
            ))/3;
        this = rotated;
        return rotated;
    }
    /// <summary>
    ///  Rotate this vector around Origin, by Rotation.
    /// </summary>
    /// <param name="Rotation">The value that this vector will be rotated by.</param>
    /// <param name="Origin">The point this vector will rotate around.</param>
    /// <returns>This method returns a copy of the value.</returns>
    public Vector3 RotateAround(Vector3 Rotation, Vector3 Origin){
        Vector3 Offset = this-Origin;
        Offset.Abs();
        Vector3 Rot = Vector3.GetRotation(this, Origin);
        Vector3 nPos = new Vector3((float)(Offset.X/(Math.Sin(180-Rot.X)/2)),
        (float)(Offset.Y/(Math.Sin(180-Rot.Y)/2)),
        (float)(Offset.Z/(Math.Sin(180-Rot.Z)/2)));
        this = nPos;
        return nPos;
    }
    public void Abs(){
        this.X = Math.Abs(this.X);
        this.Y = Math.Abs(this.Y);
        this.Z = Math.Abs(this.Z);
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
    public static Vector3 CProduct(Vector3 a, Vector3 b){
        Vector3 c = new Vector3();
        c.X = (a.Y*b.Z)-(a.Z*b.Y);
        c.Y = (a.Z*b.X)-(a.X*b.Z);
        c.Z = (a.X*b.Y)-(a.Y*b.X);
        return c;
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
    ///<summary>
    /// b is minused from all axis of a.
    ///</summary>
    public static Vector3 operator -(Vector3 a, int b){
        if(b != 0){
            return a;
        }else{
            a.X -= b;
            a.Y -= b;
            a.Z -= b;
            return a;
        }
    }
    public static Vector3 operator +(Vector3 a, Vector3 b){
        a.X += b.X;
        a.Y += b.Y;
        a.Z += b.Z;
        return a;
    } 
    public static Vector3 operator -(Vector3 a, Vector3 b){
        a.X -= b.X;
        a.Y -= b.Y;
        a.Z -= b.Z;
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
    public void Orient (Vector3 Position, (Vector3 Position, Vector3 Rotation) PostOrientation){
        // This will orient a child node by to its parent
        A += PostOrientation.Position-Position;
        A.RotateAround(PostOrientation.Rotation, PostOrientation.Position);
        B += PostOrientation.Position-Position;
        B.RotateAround(PostOrientation.Rotation, PostOrientation.Position);
        C += PostOrientation.Position-Position;
        C.RotateAround(PostOrientation.Rotation, PostOrientation.Position);
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
        /*
        float a = Vector3.GetDistance(A, light.Source);
        float b = Vector3.GetDistance(B, light.Source);
        float c = Vector3.GetDistance(C, light.Source);
        float mag;
        Vector3 mid = (A+B+C)/3;
        if(a > b){
            mag = a>c? a: c;
        }else{
            mag = b>c? b: c;
        }
        int T = (int)Vector3.GetDistance(mid, light.Source);
        int Multiplier = (int)(1/3*Math.PI*light.Radius*(light.Intensity/T));
        return Color.FromArgb(light.Color.R/T*Multiplier, light.Color.G/T*Multiplier, light.Color.B/T*Multiplier);
        */
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
/*Represents a 3-Dimensional object.*/
class gameObj{
    public int Size{
        get{
            int Vects = sizeof(float)*6*Children.Count;
            int Comps = 0;
            for(int cc =0;cc < (this.components.Count == 0? -1: this.components.Count);cc++){
                Comps += components[cc].content.Length;
            }
            return Comps+Vects;
        }
    }
    /// <summary>
    ///  The list containing all of this gameObject'scomponents
    /// </summary>
    List<(string typeName, byte[] content)> components;
    public int compLength{get{return this.components.Count;}}
    public void AddComponentFBytes<RComponent>(string typeName, byte[] Content) where RComponent : Rndrcomponent{
        components.Add((typeName, Content));
    }
    public void AddComponent<RComponent>(RComponent rC) where RComponent : Rndrcomponent{
        components.Add((rC.GetType().Name, rC.ToByte()));
    }
    public void AddComponents<RComponent>(IEnumerable<RComponent> rC) where RComponent : Rndrcomponent{
        foreach(RComponent rc in rC){
            components.Add((rC.GetType().Name, rc.ToByte()));
        }
    }
    public Type GetComponentType<RComponent>(int index) where RComponent : Rndrcomponent{
        switch(this.components[index].typeName){
            case "RigidBdy":
            return typeof(RigidBdy);
            default:
            return typeof(Rndrcomponent);
        }
    }
    /// <summary>
    ///  This goes through the gameObject's components property and returns everything of the Inputted type.
    /// </summary>
    /// <typeparam name="Component">This is the RndrCcomponent type that should be returned.</typeparam>
    /// <returns>Returns a list of all the expected components.</returns>
    public RComponent[] GetComponents<RComponent>() where RComponent : Rndrcomponent, new(){
        List<RComponent> result = new List<RComponent>();
        int cc = 0;
        RComponent c = new RComponent();
        for(string item= ""; cc < components.Count;cc++, item = components[cc].typeName){
            if(item == typeof(RComponent).Name){
                result.Add(c.FromByte(components[cc].content));
            }
        }
        return result.ToArray();
    }
    /// <summary>
    ///  This is an overload of GetComponent<Component>()
    /// </summary>
    /// <typeparam name="Component">This is the RndrCcomponent type that should be returned.</typeparam>
    /// <returns>Returns the 1st instance of the expected component</returns>
    public Component GetComponent<Component>() where Component : Rndrcomponent, new(){
        int cc = 0;
        for(string item= ""; cc < components.Count;cc++, item = components[cc].typeName){
            if(item == typeof(Component).Name){
                return new Component().FromByte(components[cc].content);
            }
        }
        return null;
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
    internal delegate void Orient_(Vector3 Position, (Vector3 Position, Vector3 Rotation) PostOrientation);
    public Orient_ orient_;
    public List<Polygon>? Children;
    public Vector3 Position;
    private Vector3 rotation;
    public Vector3 Rotation{
        get{
            return this.rotation;
        }
        set{
            this.rotation = this.Children != null && this.Children.Count > 0? value: Vector3.zero;
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
    public gameObj(Vector3 position, Vector3 rotation, List<Polygon>? children = null, List<(string name, byte[] content)>? Mycomponents = null){
        this.Position = position;
        if(children != null){
            this.orient_ = new Polygon().Orient;
        }else{
            this.Children = children;
            for(int cc = 0;cc < children.Count;cc++){
                this.orient_ += children[cc].Orient;
            }
        }
        if(Mycomponents == null){
            this.components = new List<(string name, byte[] content)>();
        }else{
            this.components = Mycomponents;
        }
        this.Rotation = rotation;
    }

    public void Rotate(Vector3 rotation, bool PrivateTranslation = false){
        Vector3 Rotated = new Vector3(this.Position).Rotate(rotation);
        if(!PrivateTranslation){
            (Vector3, Vector3) Translation = (this.Position, this.Rotation + rotation);
            this.orient_(this.Position, Translation);
        }
        this.Position = Rotated;
    }
    public static gameObj operator +(gameObj parent, Polygon child){
        parent.Children.Add(child);
        parent.orient_ += child.Orient;
        return parent;
    }
    /// <summary>
    ///  Adding two gameObjs together only appends the child obj's children to the parent obj's children.
    /// </summary>
    /// <param name="parent">The gameObj that will recieve the children.</param>
    /// <param name="child">The gameObj that will have it's children copied to the parent.</param>
    /// <returns></returns>
    public static gameObj operator +(gameObj parent, gameObj child){
        for(int cc = 0; cc < child.Children.Count;cc++){
            parent.Children.Add(child.Children[cc]);
            parent.orient_ += child.Children[cc].Orient;
        }

        return parent;
    }

    public static gameObj operator -(gameObj parent, Polygon child){
        try{
            //Use try because if the parent doesnt have the child it will catch,
            //if children is already empty or unassigned it will catch,
            parent.Children.Remove(child);
            parent.orient_ -= child.Orient;
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
    public delegate void Orient_(Vector3 PreOrient, (Vector3, Vector3) PostOrientation);
    public Orient_ orientToCam;
    public Vector3 position{get; set;}
    public Vector3 rotation{get; set;}
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
        this.position = pos == null? Vector3.zero: pos.Value;
        this.rotation = pos == null? Vector3.zero: pos.Value;
    }
}
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    public static readonly Vector3 origin = Vector3.zero;
    public static List<gameObj> worldData = new List<gameObj>();
    public static Vector3 WorldRotation{
        get{return ViewPort.cams[ViewPort.camIndex].rotation;}
    }
}
/// <summary>
///  This class represents the viewport of the user, the Camera property is what will be used to select the viewpoint.
/// </summary> 
static class ViewPort{
    /// <summary>
    ///  This property represents the bounds of the form.
    /// </summary>
    ///<remarks>The left(l) and bottom(b) properties are set to 0, 
    /// whilst the right(r) and top(t) properties hold the window width and height, respectively.</remarks>
    public static readonly (float r, float l, float b, float t) boundary = (Camera.form_.DisplayRectangle.Width, 0, 0, Camera.form_.DisplayRectangle.Height);
    public static List<Camera> cams = new List<Camera>(){new Camera()};
    public static int camIndex;
    /// <summary>
    ///  This property represents the 4x4 Matrix that will convert all viewable vector positions to Perspective projection.
    /// </summary> 
    static float[,] PPMatrix {get;} = new float[4, 4]{
        {(float)(1/(boundary.r/boundary.t)*Math.Tan(cams[camIndex].theta/2)), 0f, 0f, 0f},
        {0f, (float)(1/Math.Tan(cams[camIndex].theta/2)), 0f, 0f},
        {0f, 0f, cams[camIndex].far/(cams[camIndex].far-cams[camIndex].near), -cams[camIndex].far*cams[camIndex].near/(cams[camIndex].far-cams[camIndex].near)},
        {0f, 0f, 1f, 0f}};
    /*
        {
        {(float)(1/(boundary.r/boundary.t)*Math.Tan(cams[camIndex].theta/2)), 0f, 0f, 0f},
        {0f, (float)(1/Math.Tan(cams[camIndex].theta/2)), 0f, 0f},
        {0f, 0f, cams[camIndex].far/(cams[camIndex].far-cams[camIndex].near), -cams[camIndex].far*cams[camIndex].near/(cams[camIndex].far-cams[camIndex].near)},
        ,{0f, 0f, 1f, 0f}}
    */
    static void setCam(int value){
        if(value >0 && value < cams.Count){
            cams[value].orientToCam = cams[camIndex].orientToCam;
            camIndex = value;
        }
    }
    /// <summary>
    ///  This overload turns Gameobjs into polygons then runs the main overload(Convert_()).
    /// </summary> 
    public static (Point A, Point B, Point C, Color color)[] Convert_(List<gameObj> objs){
        List<Polygon> polygons = new List<Polygon>();
        if(objs == null){
            (Point A, Point B, Point C, Color color)[] item = [(Point.Empty, Point.Empty, Point.Empty, Color.Empty)];
            return item;
        }
        for(int cc = 0;cc < objs.Count;cc++){
            for(int cc_ = 0; cc_ < objs[cc].Children.Count;cc_++){
                polygons.Add(objs[cc].Children[cc_]);
            }
        }
        return Convert_(polygons);
    }
    /// <summary>
    ///  This overload takes the parameter data from the static World class.
    /// </summary> 
    public static (Point A, Point B, Point C, Color color)[] Convert_(){
        if(World.worldData == null){
            (Point A, Point B, Point C, Color color)[] item = [(Point.Empty, Point.Empty, Point.Empty, Color.Empty)];
            return item;
        }else{
            List<Polygon> polygons = new List<Polygon>();
            for(int cc = 0;cc < World.worldData.Count;cc++){
                for(int cc_ = 0; cc_ < World.worldData[cc].Children.Count;cc_++){
                    if(Vector3.DProduct(World.worldData[cc].Children[cc_].origin, cams[camIndex].position) < 0){
                        polygons.Add(World.worldData[cc].Children[cc_]);
                    }
                }
            }
            return Convert_(polygons);
        }
        
    }
    /// <summary>
    ///  The main Convert_() overload, This function converts the world 3d enviroment into a 2 representation.
    /// </summary> 
    public static (Point A, Point B, Point C, Color color)[] Convert_(List<Polygon> polygons){
        (Point a, Point b, Point c, Color color)[] result = new (Point a, Point b, Point c, Color color)[polygons.Count];
        if(polygons == null){
            gameObj gO = Entry.BuildWorld();
            polygons = gO.Children;
        }else{
        for(int cc = 0; cc < polygons.Count; cc++){
            if(Vector3.GetDistance(polygons[cc].A, World.origin) <= cams[camIndex].clipTol && Vector3.GetDistance(polygons[cc].B, World.origin) <= cams[camIndex].clipTol && Vector3.GetDistance(polygons[cc].C, World.origin) <= cams[camIndex].clipTol){
                continue;
            }else{
                polygons[cc] = Polygon.PolyClip(polygons[cc], Vector3.zero, cams[camIndex].far);
            }
        }
        }
        for(int cc = 0 ;cc < polygons.Count; cc++){
            Polygon CalcBuffer = Multiply(polygons[cc]);
            result[cc] = (CalcBuffer.A.ToPoint(), CalcBuffer.B.ToPoint(), CalcBuffer.C.ToPoint(), polygons[cc].Shade(new Light()));
        }
        return result;
    }
    static Polygon Multiply(Polygon polygon){
        Polygon CalcBuffer = new Polygon();
        float wA = 1;
        float wB = 1;
        float wC = 1;
        //For Point A
        CalcBuffer.A = new Vector3(
            (PPMatrix[0, 0]*cams[camIndex].far/4 * polygon.A.X/cams[camIndex].far)+(PPMatrix[1, 0]*cams[camIndex].far/4 * polygon.A.Y/cams[camIndex].far)+(PPMatrix[2, 0]*cams[camIndex].far/4 * polygon.A.Z/cams[camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*cams[camIndex].far/4 * polygon.A.X/cams[camIndex].far)+(PPMatrix[1, 1]*cams[camIndex].far/4 * polygon.A.Y/cams[camIndex].far)+(PPMatrix[2, 1]*cams[camIndex].far/4 * polygon.A.Z/cams[camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*cams[camIndex].far/4 * polygon.A.X/cams[camIndex].far)+(PPMatrix[1, 2]*cams[camIndex].far/4 * polygon.A.Y/cams[camIndex].far)+(PPMatrix[2, 2]*cams[camIndex].far/4 * polygon.A.Z/cams[camIndex].far)+(PPMatrix[3, 2])
            );
        wA = (PPMatrix[0, 3]*CalcBuffer.A.X)+(PPMatrix[1, 3]*CalcBuffer.A.Y)+(PPMatrix[2, 3]*CalcBuffer.A.Z)+PPMatrix[3, 3];
        CalcBuffer.A /= wA;
        //For Point B
        CalcBuffer.B = new Vector3(
            (PPMatrix[0, 0]*cams[camIndex].far/4 * polygon.B.X/cams[camIndex].far)+(PPMatrix[1, 0]*cams[camIndex].far/4 * polygon.B.Y/cams[camIndex].far)+(PPMatrix[2, 0]*cams[camIndex].far/4 * polygon.B.Z/cams[camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*cams[camIndex].far/4 * polygon.B.X/cams[camIndex].far)+(PPMatrix[1, 1]*cams[camIndex].far/4 * polygon.B.Y/cams[camIndex].far)+(PPMatrix[2, 1]*cams[camIndex].far/4 * polygon.B.Z/cams[camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*cams[camIndex].far/4 * polygon.B.X/cams[camIndex].far)+(PPMatrix[1, 2]*cams[camIndex].far/4 * polygon.B.Y/cams[camIndex].far)+(PPMatrix[2, 2]*cams[camIndex].far/4 * polygon.B.Z/cams[camIndex].far)+(PPMatrix[3, 2])
            );
        wB = (PPMatrix[0, 3]*CalcBuffer.B.X)+(PPMatrix[1, 3]*CalcBuffer.B.Y)+(PPMatrix[2, 3]*CalcBuffer.B.Z)+(PPMatrix[3, 3]);
        CalcBuffer.B /= wB;
        //For Point C
        CalcBuffer.C = new Vector3(
            (PPMatrix[0, 0]*cams[camIndex].far/4 * polygon.C.X/cams[camIndex].far)+(PPMatrix[1, 0]*cams[camIndex].far/4 * polygon.C.Y/cams[camIndex].far)+(PPMatrix[2, 0]*cams[camIndex].far/4 * polygon.C.Z/cams[camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*cams[camIndex].far/4 * polygon.C.X/cams[camIndex].far)+(PPMatrix[1, 1]*cams[camIndex].far/4 * polygon.C.Y/cams[camIndex].far)+(PPMatrix[2, 1]*cams[camIndex].far/4 * polygon.C.Z/cams[camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*cams[camIndex].far/4 * polygon.C.X/cams[camIndex].far)+(PPMatrix[1, 2]*cams[camIndex].far/4 * polygon.C.Y/cams[camIndex].far)+(PPMatrix[2, 2]*cams[camIndex].far/4 * polygon.C.Z/cams[camIndex].far)+(PPMatrix[3, 2])
            );
        wC = (PPMatrix[0, 3]*CalcBuffer.C.X)+(PPMatrix[1, 3]*CalcBuffer.C.Y)+(PPMatrix[2, 3]*CalcBuffer.C.Z)+(PPMatrix[3, 3]);
        CalcBuffer.C /= wC;
        return CalcBuffer;
    }
}