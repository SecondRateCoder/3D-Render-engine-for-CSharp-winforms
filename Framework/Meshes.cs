using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[DebuggerDisplay("A: {A}, B: {B}, C: {C}")]
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
    public static unsafe int Size{get{return (Vector3.Size * 3) + sizeof(Point) * 3;}}
    Point[] uPoints;
    public Point[] UVPoints{get{if(uPoints == null){return [];}else{return uPoints;}} private set{if(value.Length > 3){UpdateTexture(value);}else{this.uPoints = value;}}}
    public void UpdateTexture(Point[] uv){uPoints = new Point[3]; uPoints[0] = uv[0]; uPoints[1] = uv[1]; uPoints[2] = uv[2];}
    public Vector3 Normal{
        get{
            Vector3 a = Vector3.CProduct(this.A, this.B);
            Vector3 b = Vector3.CProduct(this.A, this.C);
            return Vector3.CProduct(a, b);}
    }
    public Vector3 Rotation{get{return Vector3.GetRotation(this);}}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Polygon(Vector3 a, Vector3 b, Vector3 c, IEnumerable<Point> uVPoints){
        this.A = a;
        this.B = b;
        this.C = c;
        this.UpdateTexture(uVPoints.ToArray());
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Polygon(Vector3 a, Vector3 b, Vector3 c){
        this.A = a;
        this.B = b;
        this.C = c;
        this.uPoints = new Point[0];
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Polygon(byte[] bytes){
        if(bytes.Length < Vector3.Size * 3){throw new TypeInitializationException("Polygon", new ArgumentOutOfRangeException());}
        List<byte[]> buffer = bytes.Chunk(Vector3.Size).ToList();
        this.A = new Vector3(buffer[0]);
        this.B = new Vector3(buffer[1]);
        this.C = new Vector3(buffer[2]);
        int cc = Vector3.Size * 3;
        this.UVPoints = [StorageManager.ReadPoint(bytes, cc), 
        StorageManager.ReadPoint(bytes, cc + (sizeof(int) * 2)), 
        StorageManager.ReadPoint(bytes, cc + (sizeof(int) * 4))];
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Polygon(){
        this.A = new Vector3();
        this.B = new Vector3();
        this.C = new Vector3();
        this.uPoints = new Point[0];
    }
    public byte[] ToBytes(){
        List<byte> bytes = [..this.A.ToBytes()];
        bytes.AddRange(this.B.ToBytes());
        bytes.AddRange(this.C.ToBytes());
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[0].X));
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[0].Y));
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[1].X));
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[1].Y));
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[2].X));
        bytes.AddRange(BitConverter.GetBytes(this.uPoints[2].Y));
        return bytes.ToArray();
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
    public Vector3 Furthest(Vector3 origin){
        Vector3 result;
        float a = Vector3.GetDistance(A, origin);
        float b = Vector3.GetDistance(B, origin);
        float c = Vector3.GetDistance(C, origin);
        if(a>b){
            result = a>c?this.A:this.C;
        }else{
            result = b>c?this.B:this.C;
        }
        return result;
    }
    public Color Shade(Light light, Color? c = null){
        if(c == null){c = Color.Gray;}
        return Color.FromArgb((int)(c.Value.A * Math.Max(0, Vector3.DProduct(this.Normal, light.Source))));
        /*
        int ColorIntensity = (int)(Vector3.DProduct(this.Normal, light.Source.GetNormal())*(1/Vector3.GetDistance(this.origin, light.Source)));
        return Color.FromArgb(255* ColorIntensity* (1/light.Colour.A + 1/c.Value.A), 255* ColorIntensity* (1/light.Colour.R + 1/c.Value.R),
        255* ColorIntensity* (1/light.Colour.G + 1/c.Value.G), 255* ColorIntensity* (1/light.Colour.B + 1/c.Value.B));
        */
    }
    /// <summary>
    /// Shade a polygon according to the color parameter an the light.
    /// </summary>
    /// <param name="lights">The lights to be taken into consideration.</param>
    /// <param name="color">The color the polygon should be shaded to.</param>
    /// <returns>A color shade.</returns>
    public Color Shade(IEnumerable<Light> lights, Color color){
        Color C = color;
        foreach(Light light in lights){
            C = this.Shade(light, C);
        }
        Color result = Color.Gray;
        return Color.FromArgb(1/(result.A+C.A), 1/(result.R+C.R), 1/(result.G+C.G), 1/(result.B+C.B));
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
    public override bool Equals(object? obj){if(obj == null){return false;}else{return this == (Polygon)obj;}}
    public override int GetHashCode(){return (this.A.GetHashCode() + this.B.GetHashCode() + this.C.GetHashCode())/32;}
	/// <summary>
	///  Create a mesh.
	/// </summary>
	/// <param name="height">The height of the mesh.</param>
	/// <param name="width">Half the width of the mesh.</param>
    /// <param name="Bevel">The length at which the centre of the cross-section of the mesh meets the main length of the mesh</param>
	/// <returns>An array of polygons describing a mesh</returns>
	/// <remarks>This method programmatically generates a prism with the inputted number of sides.</remarks>
	public static Polygon[] Mesh(int Height =10, int width =10, int bevel =0, int sides =3){
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
[DebuggerDisplay("Position: {Position}, Rotation: {Rotation}, Count: {Count}")]
class Mesh : Rndrcomponent, IEnumerable{
    public delegate Polygon ForEachDelegate(Polygon p);
    public static Mesh Empty{get{return new Mesh();}}
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
    List<Polygon> mesh;
    public int Count{get{return this.mesh.Count;}}
    public Polygon[] Get(){return this.mesh.ToArray();}
	public void Add(Polygon poly){mesh.Add(poly);}
	public void AddRange(IEnumerable<Polygon> polygons){mesh.AddRange(polygons);}
	public void RemoveAt(int index){this.mesh.RemoveAt(index);}
    public void Remove(Polygon p){this.mesh.Remove(p);}
	public List<Polygon> ToList(){return this.mesh;}
    public Mesh(){
        this.mesh = [];
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Mesh(byte[] bytes){
        this.FromBytes(bytes);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
    public void Foreach(ForEachDelegate fE){
        for(int cc = 0;cc < this.Count;cc++){
            this[cc] = fE(this[cc]);
        }
    }
    public Mesh ViewPortClip(){
        Camera cam = World.cams[World.camIndex];
        Mesh m = this;
        foreach(Polygon p in m){
            float Sim = Vector3.DProduct(p.Normal, cam.Position * cam.Rotation.GetNormalised());
            if(Sim < 0){this.mesh.Remove(p);}
        }
        return m;
    }
    public Polygon this[int index]{
        get{return mesh[index];}
        set{mesh[index] = value;}
    }

    /*
    ! IEnumerable overrides.
    */
    public override bool Equals(object? obj){if(obj == null){return false;}else{return this == (Mesh)obj;}}
    public override int GetHashCode(){
        int result = 0;
        foreach(Polygon p in this){
            result += p.GetHashCode();
            result /= 2;
        }
        return result;
    }

    /*
    !RndrComponents overrides.
    */
    /// <summary>Dispose this Mesh.</summary>
    /// <param name="disposing">Should this Mesh be completely ERASED, True if no, False to save to a temporary file.</param>
    public void Dispose(bool disposing, bool Collider = false){
        if(!disposing){
            byte[] me = this.ToByte();
            AsyncCallback? callback = (ar) => {if(!ar.IsCompleted){throw new ArgumentException();}};
            File.Create(AppDomain.CurrentDomain.BaseDirectory + 
            @"Cache\Temp" + 
            Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Temp").Count()+1 + (Collider? ".cll": ".msh")).
            BeginWrite(me, 0, me.Length, callback, Thread.CurrentThread.Name);
        }
        this.mesh = [];
    }
    public override int Size{get{return (Polygon.Size * this.Count) + (Vector3.Size * 2);}}
    public override byte[] ToByte(){
        List<byte> bytes = [.. BitConverter.GetBytes(this.Count)];
        foreach(Polygon p in this){
            bytes.AddRange(p.ToBytes());
        }
        bytes.AddRange(this.Position.ToBytes());
        bytes.AddRange(this.Rotation.ToBytes());
        return bytes.ToArray();
    }
    public override void FromBytes(byte[] bytes){
        int Count = StorageManager.ReadInt32(bytes, 0);
        this.mesh = new List<Polygon>(Count);
        int cc =0;
        int pointer = sizeof(int);
        for(;pointer < Count;cc++ ,pointer += Polygon.Size){
            byte[] buffer = new byte[Polygon.Size];
            for(int i =0; i < Polygon.Size;i++){buffer[i] = bytes[pointer + i];}
            mesh[cc] = new Polygon(buffer);
        }
        this.Position = new Vector3(bytes, cc);
        this.Rotation = new Vector3(bytes, cc + Vector3.Size);
    }
    public override void Initialise(){throw new NotImplementedException();}

    /*
    !Static functions and stuff
    */

    /// <summary>
    ///  Checks how similar two meshes are, measuring it as percentage of the mesh sizes.
    /// </summary>
    /// <param name="m">The 1st mesh to be compared to.</param>
    /// <param name="m2">The 2nd mesh to be compared to.</param>
    /// <returns>A bool.</returns>
	/// <remarks>Both meshes must be same sized.</remarks>
    public static bool operator ==(Mesh? m, Mesh? m2){
        int sQ = 0;
        if(object.Equals(m, null) | object.Equals(m2, null)){return false;}
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
    ///  Checks how similar two meshes are, measuring it as percentage of the mesh sizes.
    /// </summary>
    /// <param name="m">The 1st mesh to be compared to.</param>
    /// <param name="m2">The 2nd mesh to be compared to.</param>
	/// <param name="Tolerance">This is the parameter the the similarity Quotient will be compared to</param>
    /// <returns>A bool.</returns>
	/// <remarks>Both meshes must be same sized.</remarks>
    public static bool operator ==(Mesh? m, Mesh? m2, float Tolerance){
		Tolerance = Tolerance > 1? .9f: Tolerance;
 		Tolerance = Tolerance < 0? .1f: Tolerance;
        int sQ = 0;
        if(object.Equals(m, null) | object.Equals(m2, null)){return false;}
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
        return sQ > Tolerance? true: false;
	}
	/// <summary>
	/// Checks how similar two meshes are and returns the inverse of that value.
	/// </summary>
	/// <param name="m">The 1st mesh to be compared.</param>
	/// <param name="m2">The 2nd mesh to be compared.</param>
	/// <returns>A boolean value.</returns>
	/// <remarks>Both meshes must be same-sized.</remarks>
	public static bool operator !=(Mesh? m, Mesh? m2){
		return !(m == m2);
	}
    public static explicit operator Mesh(Polygon[] polygons){return new Mesh(polygons);}
    public static explicit operator Polygon[](Mesh mesh){return mesh.Get();}

    ///<summary>Translates this mesh by the Vector3 v.</summary>
    public static Mesh operator +(Mesh m, Vector3 v){
        m.Translate(v, Vector3.Zero);
        return m;
    }

    ///<summary>Scales the Mesh by the Vector3 v</summary>
    public static Mesh operator *(Mesh m, Vector3 v){
        m.Translate(m.Position - (m.Position * v), Vector3.Zero);
        return m;
    }

    public IEnumerator GetEnumerator() => mesh.GetEnumerator();
}
