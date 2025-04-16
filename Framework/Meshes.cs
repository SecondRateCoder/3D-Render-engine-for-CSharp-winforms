using System.Collections;


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
    Point[] uPoints;
    public Point[] UVPoints{get{if(uPoints == null){return [];}else{return uPoints;}} private set{this.uPoints = value;}}
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
    public Polygon(Vector3 a, Vector3 b, Vector3 c, IEnumerable<Point> uVPoints){
        this.A = a;
        this.B = b;
        this.C = c;
        this.uPoints = uVPoints.ToArray();
    }
    public Polygon(Vector3 a, Vector3 b, Vector3 c){
        this.A = a;
        this.B = b;
        this.C = c;
        this.uPoints = new Point[0];
    }
    public Polygon(){
        this.A = new Vector3();
        this.B = new Vector3();
        this.C = new Vector3();
        this.uPoints = new Point[0];
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
    public Color Shade(Light light, Color? c = null){
        if(c == null){c = Color.Gray;}
        int ColorIntensity = (int)(Vector3.DProduct(this.Normal, light.Source.GetNormal())*(1/Vector3.GetDistance(this.origin, light.Source)));
        return Color.FromArgb(255* ColorIntensity* (1/light.Colour.A + 1/c.Value.A), 255* ColorIntensity* (1/light.Colour.R + 1/c.Value.R),
        255* ColorIntensity* (1/light.Colour.G + 1/c.Value.G), 255* ColorIntensity* (1/light.Colour.B + 1/c.Value.B));
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
class Mesh : IEnumerable{
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
    List<Polygon> mesh;
    public int Count{get{return this.mesh.Count;}}
    public float Volume{get{
        float buffer = 0f;
        foreach(Polygon p in this.mesh){buffer += p.Area;}
        return buffer;
    }}
	public void Add(Polygon poly){mesh.Add(poly);}
	public void AddRange(IEnumerable<Polygon> polygons){mesh.AddRange(polygons);}
	public void RemoveAt(int index){this.mesh.RemoveAt(index);}
	public List<Polygon> ToList(){return this.mesh;}
    

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
    public Polygon this[int index]{
        get{return mesh[index];}
        set{mesh[index] = value;}
    }

    
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
    public override bool Equals(object? obj){if(obj == null){return false;}else{return this == (Mesh)obj;}}
    public override int GetHashCode(){
        int result = 0;
        foreach(Polygon p in this){
            result += p.GetHashCode();
            result /= 2;
        }
        return result;
    }
    public static explicit operator Mesh(Polygon[] polygons){return new Mesh(polygons);}


    IEnumerator IEnumerable.GetEnumerator(){return (IEnumerator)GetEnumerator();}
    public MeshEnum GetEnumerator(){ return new MeshEnum(mesh);}
}
class MeshEnum : IEnumerator{
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