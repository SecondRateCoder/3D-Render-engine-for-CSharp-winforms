using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
/// <summary>
///  A 3-dimensional point and rotation.
/// </summary> 
[DebuggerDisplay("x: {X}, y: {Y}, z: {Z}")]
struct Vector3{
    /// <summary>
    ///  This property represents a Vector, which has it's x, y and z values set to 0.
    /// </summary> 
    public static readonly Vector3 Zero = new Vector3();
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
    public static readonly Vector3 One = new Vector3(1, 1, 1);

    public static unsafe int Size{get{return sizeof(Vector3);}}
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
    public Vector3(byte[] bytes, int startFrom =0){
        if(bytes.Length != sizeof(int)*3){
            this = Vector3.Zero;
        }else{
            this.X = StorageManager.ReadFloat(bytes, 0 +startFrom);
            this.Y = StorageManager.ReadFloat(bytes, sizeof(float) +startFrom);
            this.Z = StorageManager.ReadFloat(bytes, (sizeof(float) * 2) +startFrom);
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
    public Vector3 Tangent(){
        this.X = (float)Math.Tan(this.X);
        this.Y = (float)Math.Tan(this.Y);
        this.Z = (float)Math.Tan(this.Z);
        return this;
    }
    /*
    void Vector3_Rotate(Vector3 *self, Vector3 change, Vector3 Around){
    float Xrot[3][3] = {
        {1, 0, 0}, 
        {0, cos(change.X), 0 - sin(change.X)}, 
        {0, sin(change.X), cos(change.X)}
    };
    float Yrot[3][3] = {
        {cos(change.Y), 0, sin(change.Y)}, 
        {0, 1, 0 - 0}, 
        {0 - sin(change.Y), 0, cos(change.Y)}
    };
    float Zrot[3][3] = {
        {cos(change.Z), 0 - sin(change.Z), 0}, 
        {sin(change.Z), cos(change.Z), 0}, 
        {0, 0, 1}
    };
    self->X = self->X - Around.X;
    self->Y = self->Y - Around.Y;
    self->Z = self->Z - Around.Z;
    MassMultiply(self, Xrot, Yrot, Zrot);
}
void MassMultiply(Vector3 *self, float m1[3][3], float m2[3][3], float m3[3][3]){Vector3_MatrixMultiply(&self, Matrix_MatrixMultiply(Matrix_MatrixMultiply(m1, m2), m3));}
void Vector3_MatrixMultiply(Vector3 *self, float Matrix[3][3]){
    __m128 result;
    __m128 Column = _mm_loadu_ps((float[4]){(self->X * Matrix[0][0]), (self->Y * Matrix[1][0]), (self->Y * Matrix[2][0]), 0});
    result = _mm_add_ps(Column, _mm_loadu_ps((float[4]){(self->X * Matrix[0][1]), (self->Y * Matrix[1][1]), (self->Y * Matrix[2][1]), 0}));
    result = _mm_add_ps(result, _mm_loadu_ps((float[4]){(self->X * Matrix[0][2]), (self->Y * Matrix[1][2]), (self->Y * Matrix[2][2]), 0}));
    float* res = (float*)&result;
    self->X = res[0]; self->Y = res[1]; self->Z = res[2];
}
    */
    /// <summary>
    ///  Rotate this vector around Origin, by Rotation.
    /// </summary>
    /// <param name="Rotation">A Vector3 value that this vector will be rotated by.</param>
    /// <param name="Origin">The point this vector will rotate around.</param>
    /// <returns>This method returns a copy of the value.</returns>
    /// <remarks>This both sets this vector to the rotated version and returns a copy.</remarks>
    public Vector3 RotateAround(Vector3 Rotation, Vector3 Origin){
        if(Rotation == Vector3.Zero){return this;}
        this -= Origin;
        float XCOS = (float)Math.Cos(Rotation.X), XSIN = (float)Math.Sin(Rotation.X);
        float YCOS = (float)Math.Cos(Rotation.Y), YSIN = (float)Math.Sin(Rotation.Y);
        float ZCOS = (float)Math.Cos(Rotation.Z), ZSIN = (float)Math.Sin(Rotation.Z);
        float[,] Xrot = new float[3, 3]{
            {1, 0, 0}, 
            {0, XCOS, -XSIN}, 
            {0, XSIN, XCOS}};
        float[,] Yrot = new float[3, 3]{
            {YCOS, 0, YSIN}, 
            {0, 1, 0}, 
            {-YSIN, 0, YCOS}};
        float[,] Zrot = new float[3, 3]{
            {ZCOS, -ZSIN, 0}, 
            {ZSIN, ZCOS, 0}, 
            {0, 0, 1}};
        this = Mass_MatrixMultiply(this, Xrot, Yrot, Zrot);
        this += Origin;
        return this;
    }
    public static Vector3 Mass_MatrixMultiply(Vector3 v, params float[][,] Matrices) {
        int cc = 0;
        while (cc < Matrices.Length){
            if(Matrices[cc].GetLength(0) >= 3 && Matrices[cc].GetLength(1) >= 3){ v = MatrixMultiply(v, Matrices[cc]); }
            cc++;
        }
        return v;
    }
    public static Vector3 MatrixMultiply(Vector3 v, float[,] Matrix){
        /*
        |---------->|       | | |
        |           |       | | |
        |           |       | V |
        */
        v.X = (Matrix[0, 0] * v.X) + (Matrix[1, 0] * v.Y) + (Matrix[2, 0] * v.Z);
        v.Y = (Matrix[0, 1] * v.X) + (Matrix[1, 1] * v.Y) + (Matrix[2, 1] * v.Z);
        v.Z = (Matrix[0, 2] * v.X) + (Matrix[1, 2] * v.Y) + (Matrix[2, 2] * v.Z);
        return v;
    }
    public Vector3 Abs(){
        this.X = Math.Abs(this.X);
        this.Y = Math.Abs(this.Y);
        this.Z = Math.Abs(this.Z);
        return this;
    }
    public void Normalise(){
        this/=this.Magnitude;
    }
    public Vector3 GetNormalised(){
        Vector3 v = this;
        this.Normalise();
        this = v;
        return v;
    }
    public static float Regulate(float value, int iFUnacceptable = 0){
        if(value == float.PositiveInfinity | value == float.NegativeInfinity){return iFUnacceptable;}
        else{return value;}
    }
    public byte[] ToBytes(){
        List<byte> result = [..BitConverter.GetBytes(this.X)];
        result.AddRange(BitConverter.GetBytes(this.Y));
        result.AddRange(BitConverter.GetBytes(this.Z));
        return [.. result];
    }
    
    public static float ComputeDot(Vector3 vector, Vector3 lightPosition){
        Vector3 normal = Vector3.CProduct(vector, lightPosition).GetNormalised();
        lightPosition.Normalise();
        return Math.Max(0, Vector3.DProduct(normal, lightPosition - vector));
    }
    public override bool Equals(object? obj){if(obj == null){return false;}else{return this == (Vector3)obj;}}
    static byte[] ComputeHmacSha1Hash(string rawData, string key){
        using (HMACSHA1 hmacSha1 = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(key))){
            byte[] bytes = hmacSha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            hmacSha1.Dispose();
            return bytes;
        }
    }
    public override int GetHashCode(){return BitConverter.ToInt32(ComputeHmacSha1Hash($"{this.X} {this.Y} {this.Z}", ""));}
    public static explicit operator PointF(Vector3 v){if(v.Z == 0){return new PointF((int)v.X, (int)v.Y);}else{return new PointF((int)(v.X/v.Z), (int)(v.Y/v.Z));}}
    public static explicit operator Vector3(PointF p){return new Vector3(p.X, p.Y, 0);}
    public static explicit operator List<float>(Vector3 v){return new List<float>(){v.X, v.Y, v.Z};}
    public static explicit operator byte[](Vector3 v){
        List<byte> result = [.. BitConverter.GetBytes(v.X)];
        result.AddRange(BitConverter.GetBytes(v.Y));
        result.AddRange(BitConverter.GetBytes(v.Z));
        return [.. result];
    }
    ///<summary>
    /// Get the distance from 2 vectors, as a float;
    ///</summary>
    public static float GetLength(Vector3 v1, Vector3 v2){return MathF.Sqrt((v1.X-v2.X)*(v1.X-v2.X) + (v1.Y-v2.Y)*(v1.Y-v2.Y) + (v1.Z-v2.Z)*(v1.Z-v2.Z));}
    ///<summary>Get the rotation from point a to b, with the apex being between them.</summary>
    public static Vector3 GetRotation(Vector3 a, Vector3 b){return GetRotation(a, Vector3.Zero, b);}
    public static Vector3 GetRotation(Polygon p){
        return GetRotation(p.A, p.origin, p.C);
    }
    public static Vector3 GetRotation(Vector3 a, Vector3 b, Vector3 Centre){
        a -= Centre;
        float ALength = Vector3.GetLength(a, Vector3.Zero);
        b -= Centre;
        float BLength = Vector3.GetLength(b, Vector3.Zero);
        if (BLength > ALength){
            b *= ALength / BLength;
        }else if (ALength < BLength){
            a *= BLength / ALength;
        }
        Vector3 k = CProduct(a, b);
        float angle = MathF.Acos(DProduct(a, b) / (Vector3.GetLength(a, Vector3.Zero) * Vector3.GetLength(b, Vector3.Zero)));
        float angCos = MathF.Cos(angle);
        float decangCos = 1 - angCos;
        float angSin = MathF.Sin(angle);
        return MatrixMultiply(Vector3.One, new float[3, 3]{
            {angCos+(k.X*k.X*decangCos), (k.X*k.Y*decangCos)-(k.Z*angSin), (k.X*k.Z*decangCos)+(k.Y*angSin)},
            {(k.Y*k.X*decangCos)+(k.Z*angSin), angCos+(k.Y * k.Y * decangCos), (k.Y*k.Z*decangCos)-(k.X*angSin)},
            {(k.Z*k.X*decangCos)-(k.Y*angSin), (k.Z*k.Y*decangCos)+(k.X*angSin), angCos+(k.X*k.X*decangCos)}});
        // [Changed vector] = [Matrix] * [Vector]
        // float A = Vector3.GetDistance(a, c);
        // float B = Vector3.GetDistance(b, c);
        // if(A > B){a = (a-b)/b;}else if(A < B){b = (b-a)/a;}
        // float xDif = Math.Abs(a.X - b.X);
        // float yDif = Math.Abs(a.Y - b.Y);
        // float zDif = Math.Abs(a.Z - b.Z);
        // double xy = Math.Atan(yDif/xDif);
        // double xz = Math.Atan(zDif/xDif);
        // double yz = Math.Atan(yDif/zDif);
        // return new Vector3((float)xy, (float)xz, (float)yz);

    }
    public static Vector3 CProduct(Vector3 a, Vector3 b){
        return new Vector3((a.Y*b.Z)-(a.Z*b.Y), (a.Z*b.X)-(a.X*b.Z), (a.X*b.Y)-(a.Y*b.X));
    }
    public static float DProduct(Vector3 a, Vector3 b){
        a.Normalise();
        b.Normalise();
        return (a.X*b.X)+(a.Y*b.Y)+(a.Z*b.Z);
    }
    public static Vector3 Transform(Vector3 value, System.Numerics.Quaternion rotation){
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector3(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2)
        );
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
    public static Vector3 operator *(Vector3 a, PointF b){
        float aMag = a.Magnitude;
        a.Normalise();
        b = new PointF((int)(b.X/aMag), (int)(b.Y/aMag));
        return new Vector3(a.X/a.Z*b.X, a.Y/a.Z*b.Y, 0);
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

}
/// <summary>Represents a 3-Dimensional object.</summary>
[DebuggerDisplay("Position: {Positon}, Rotation = {Rotation}", Name = "{Name}")]
class gameObj{
    public static gameObj Empty{get{return new gameObj(Vector3.Zero, Vector3.Zero, false, [], [], "");}}
    public gameObj Copy(){return new gameObj(this.Position, this.Rotation, false, (Polygon[])this.Children, this.components, this.Name + "(1)");}
    public string Name;
    public Mesh Children{
#pragma warning disable CS8603 // Possible null reference return.
        get{if(this.HasComponent<Mesh>()){return this.GetComponent<Mesh>();}else{return Mesh.Empty;}}
#pragma warning restore CS8603 // Possible null reference return.
    }
    public Vector3 Position;
    private Vector3 rotation;
    public bool isEmpty{get{if(this.Position == Vector3.Zero &&this.Rotation == Vector3.Zero 
    &&components.Count == 0 &&Children.Count == 0){return true;}else{return false;}}}
    /// <summary>
    ///  The list containing all of this gameObject's components
    /// </summary>
    List<(Type ogType, Rndrcomponent rC)> components;
    public int compLength{get{return this.components.Count;}}
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public unsafe int Size{get{return sizeof(gameObj) + this.Children.Size;}}
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public Vector3 Rotation{
        get{return this.rotation;}
        set{this.rotation = this.Children.Count > 0? value: Vector3.Zero;}
    }
    public int CollisionRange{
        get{
            float result = Vector3.GetLength(this.Position, Children[0].Furthest(this.Position));
            for(int cc = 1; cc<Children.Count;cc++){
                float distance = Vector3.GetLength(this.Position, Children[0].Furthest(this.Position));
                result = result > distance? result: distance;
            }
            return (int)(result+.5);
            
        }
    }

    public void AddComponent<RComponent>(RComponent? rC = null) where RComponent : Rndrcomponent, new(){
        _ = rC ?? throw new TypeInitializationException(nameof(gameObj), new ArgumentNullException());
        if(this.components == null){
            this.components = [(typeof(RComponent), rC)];
        }else{this.components.Add((typeof(RComponent), rC));}
    }
    public bool HasComponent<RComponent>()where RComponent : Rndrcomponent, new(){
        if(GetComponent<RComponent>() == null){return false;}else{return true;}
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
    public Component GetComponent<Component>() where Component : Rndrcomponent, new(){
        for(int cc = 0; cc < components.Count;cc++){
            if(this.components[cc].ogType == typeof(Component)){
                return (Component)this.components[cc].rC;
            }
        }
        return (Component)EmptyComponent.Empty;
    }
    public void Translate(Vector3 position, Vector3 rotation, bool PrivateTranslation = false){
        this.Position += position;
        this.Rotation += rotation;
        if(!PrivateTranslation){
            this.Children.Translate(this.Position - position, rotation);
        }
    }
    public TextureDatabase Texture(TextureStyles? tS = null){
        if(HasComponent<Texturer>()){
            if(tS != null){this.GetComponent<Texturer>().UpdateTextureStyle(tS);}
            Texturer t = this.GetComponent<Texturer>();
            if(t.isEvenTextured){
                return t.Texture(this.Children);
            }
        }
        return TextureDatabase.Empty;
    }
    public bool ChangeTextureStyle(TextureStyles tS){if(this.HasComponent<Texturer>()){this.GetComponent<Texturer>().UpdateTextureStyle(tS);return true;}else{return false;}}
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
        for(int cc = 1; cc < poly.Length; cc++, Dis = Vector3.GetLength(this.Children[cc].origin, Position)){
            if(Vector3.GetLength(scope.origin, Position) < Dis){
                scope = this.Children[cc];
            }
        }
        Dis = Vector3.GetLength(scope.origin, Position);
        return (scope, Dis <= LowerBd && Dis > 0? true: false);
    }

    public static gameObj Create(Vector3 position, Vector3 rotation, IEnumerable<Polygon>? children = null, List<(Type, Rndrcomponent)>? Mycomponents = null, string? name = null){
        World.Add(new gameObj(position, rotation, false, children, Mycomponents, name));
        return World.world[World.world.Count -1];
    }
    public gameObj(Vector3 position, Vector3 rotation, bool Create = true, IEnumerable<Polygon>? children = null, List<(Type t, Rndrcomponent rC)>? Mycomponents = null, string? name = null){
        if(Mycomponents != null){this.components = Mycomponents;}else{ this.components = []; }
        if(Mycomponents == null){this.components = [];}
        else{this.components = Mycomponents;}
        if(children != null){this.AddComponent<Mesh>((Mesh)children.ToList());}
        this.Position = Vector3.Zero;
        this.Rotation = Vector3.Zero;
        this.Translate(position, rotation, false);
        this.Name = name == null? $"{World.world.Count+1}": name;
        if(Create){World.Add(this);}
        return;
    }

    public static gameObj operator +(gameObj parent, Polygon[] children){
        parent.Children.AddRange(children);
        return parent;
    }
    public static gameObj operator +(gameObj parent, Polygon child){
        parent.Children.Add(child);
        return parent;
    }
    /// <summary>
    ///  Adding two gameObjs together only appends the child obj's children to the parent obj's children.
    /// </summary>
    /// <param name="parent">The gameObj that will recieve the children.</param>
    /// <param name="child">The gameObj that will have it's children copied to the parent.</param>
    public static gameObj operator +(gameObj parent, gameObj child){
        parent.Children.AddRange(child.Children.ToList());
        return parent;
    }
}

