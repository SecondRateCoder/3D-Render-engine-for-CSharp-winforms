using System.Collections;

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
	public static bool operator ==(Polygon a, Polygon b){
		if(a.A == b.A && a.B == b.B && a.C == b.C){return true;}else{return false;}
	}
	public static bool operator !=(Polygon a, Polygon b){
		return !(a == b);
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
    public static Vector3[] ToVector3(Polygon poly){
        Vector3[] points = new Vector3[(int)poly.Area];
        Vector3 p = new Vector3();
		//Just assign p and the loop will automattically assign it to the array/
        for(int cc = 0;cc < points.Length;cc++, points[cc] = p){
            
        }
		return points;
    }
}
struct Mesh : IEnumerable{
    public Mesh(IEnumerable<Polygon> p){
        this.mesh = p.ToList();
    }
    public async void Translate(Vector3 position, Vector3 rotation){
        this.Position += position;
        this.Rotation += rotation;
		Mesh m = this;
		await Task.Run(() => {
			for(int cc = 0;cc < m.Count; cc++){
				m[cc].Translate(m.Position - position, position, rotation);
			}
		});
    }
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
    List<Polygon> mesh;
    public int Count{get{return this.mesh.Count;}}
	public void Add(Polygon poly){mesh.Add(poly);}
	public void AddRange(IEnumerable<Polygon> polygons){mesh.AddRange(polygons);}
	public List<Polygon> ToList(){return this.mesh;}
	public void RemoveAt(int index){this.mesh.RemoveAt(index);}

    
    /// <summary>
    ///  Checks how similar two meshes are, measuring it as percentage of the mesh sizes.
    /// </summary>
    /// <param name="m">The 1st mesh to be compared to.</param>
    /// <param name="m2">The 2nd mesh to be compared to.</param>
	/// <param name="Tolerance">This is the parameter the the similarity Quotient will be compared to</param>
    /// <returns>A bool.</returns>
	/// <remarks>Both meshes must be same sized.</remarks>
    public static bool operator ==(Mesh m, Mesh m2){
        int sQ = 0;
        if(m.Count != m2.Count){
            return false;
        }else{
			int increment = 1/m.Count;
            for(int cc = 0; cc < m.Count; cc++){
				if(m[cc] == m2[cc]){
					sQ += increment;
				}
			}
            }
        return sQ > .9f? true: false;
	}
	/// <summary>
	/// Checks how similar two meshes are and returns the inverse of that value.
	/// </summary>
	/// <param name="m">The 1st mesh to be compared.</param>
	/// <param name="m2">The 2nd mesh to be compared.</param>
	/// <returns>A boolean value.</returns>
	/// <remarks>Both meshes must be same-sized.</remarks>
	public static bool operator !=(Mesh m, Mesh m2){
		return !(m == m2);
	}
    IEnumerator IEnumerable.GetEnumerator(){return (IEnumerator)GetEnumerator();}
    public MeshEnum GetEnumerator(){ return new MeshEnum(mesh);}
    public Polygon this[int index]{
        get{
            return mesh[index];
        }
        set{
            mesh[index] = value;
        }
    }
}
struct MeshEnum : IEnumerator{
    public Polygon[] mesh;
    int position = -1;
    public MeshEnum(IEnumerable<Polygon> mesh){
        this.mesh = mesh.ToArray();
    }
    public bool MoveNext(){
        position++;
        return(position < mesh.Length);
    }
    public void Reset(){
        position = -1;
    }
    object IEnumerator.Current{
        get{
            return Current;
        }
    }
    public Polygon Current{
        get{
            try{
                return this.mesh[this.position];
            }catch(IndexOutOfRangeException){
                throw new InvalidOperationException();
            }
        }
    }

}