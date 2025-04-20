using System.Collections;
using System.Text;

/// <summary>
///  This abstract class is created to act as a link between different component types.
/// </summary>
abstract class Rndrcomponent{
    public abstract int Size{get;}
    /// <summary>
    /// Encodes this instance into a byte array.
    /// </summary>
    /// <returns>An encoded array that describes this instance.</returns>
    public abstract byte[] ToByte();
    /// <summary>
    /// Assigns this instance the decoded byte array.
    /// </summary>
    /// <param name="bytes">the encoded array.</param>
    public abstract void FromBytes(byte[] bytes);
    /// <summary>Clear this instance, emptying all data stores to null or 0.</summary>
    /// <param name="disposing">Should this function Dispose this instance.</param>
    internal void Dispose(bool disposing){}
    /// <summary>Initialise this instance, filling it's data stores with the necessary information.</summary>
    public abstract void Initialise();
}
class Empty : Rndrcomponent{
    public override void FromBytes(byte[] bytes){return;}
    public override int Size{get{return 0;}}
    public override byte[] ToByte(){return new byte[0];}
    public override void Initialise(){return;}

}
class TextureDatabase : IEnumerable{
    List<(Point point, Color color)> td;
    public int Count{get{
        //If there's been a change the re-assign _c , otherwise move on. 
        // then return it.
        if(Unsignedchange){
            _c = td.Count;
        }
        return _c;
    }}
    int _c;
    bool Unsignedchange;
    public (Point p, Color c) this[int index]{
        get{return td[index];}
        set{td[index] = value;}
    }
    public TextureDatabase(List<(Point p, Color c)> data){
        this.td = data;
    }
    public TextureDatabase(){this.td = [];}
    public void Add((Point, Color) data)=> this.Append(data);
    public void Add(List<(Point p, Color c)> data) => this.Append(data);
    public void Append((Point, Color) data){this.Unsignedchange = true; td.Add(data);}
    public void Append(List<(Point p, Color c)> data){this.Unsignedchange = true; foreach((Point p, Color c) item in data){this.Append(item);}}
    public static implicit operator List<(Point point, Color color)>(TextureDatabase tD){return tD.td;}
    public static explicit operator TextureDatabase(List<(Point point, Color color)> data){return new TextureDatabase(data);}

    public IEnumerator GetEnumerator() => td.GetEnumerator();
}
class Texturer : Rndrcomponent{
    public static TextureDatabase? textureData = [];
    /// <summary>Store the image file in this before Initialising.</summary>
    Bitmap buffer;
    Path filePath;
    bool Initialised;
    int DataStart;
    int DataEnd;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Texturer(){
        this.filePath = new Path(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\Grass Block.png", [".bmp", ".jpeg", ".png"], false);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Texturer(string? filePath = null){
        this.filePath = filePath == null? new Path(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\Grass Block.png", [".bmp", ".jpeg", ".png"], false):
        new Path(filePath, [".bmp", ".jpeg", ".png"], false);
        buffer = new Bitmap(1, 1);
    }

    public void Reset(string Path){
        this.Dispose(true);
        _Initialise();
    }
    /// <summary>
    /// This generates a TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints. 
    /// </summary>
    /// <param name="UVpoints">The List of 3 Point elements that represent the space taken for the texture.</param>
    /// <param name="Append">Should this function append result to the static Texturer.texturerData buffer, <see cref="Texturer.textureData.Count"/></param>
    /// <returns>A TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints.</returns>
    public TextureDatabase Texture(List<Point[]> UVpoints, bool Append = true){
        if(this.Initialised == false){throw new ObjectDisposedException("Texturer");}
        TextureDatabase result = new TextureDatabase([]);
        //Initialise this component.
        _Initialise();
        for(int cc =0; cc < UVpoints.Count;cc ++){
            //Find tY length of the 2d polygon then find the range
            int MinY = Texturer.Min([UVpoints[cc][0].Y, UVpoints[cc][1].Y, UVpoints[cc][2].Y]);
            int YRange = Texturer.Max([UVpoints[cc][0].Y, UVpoints[cc][1].Y, UVpoints[cc][2].Y]) - MinY;
            //The mid-point when the gradient of the line around 1 point changes, set on the line opposite it.
            Point _12mid = new Point((UVpoints[cc][1].X + UVpoints[cc][2].X)/2, (UVpoints[cc][1].Y + UVpoints[cc][2].Y)/2);
            //Point 0 (UVPoints[cc][0]) to Point 1 (UVPoints[cc][1])
            Equation p0_p1 = Equation.FromPoints(UVpoints[cc][0], UVpoints[cc][1]);
            //Point 1 (UVPoints[cc][1]) to the midPoint
            Equation p1_p12mid = Equation.FromPoints(UVpoints[cc][1], _12mid);
            //Point 1 (UVPoints[cc][1]) to Point 2 (UVPoints[cc][2])
            Equation p1_p2 = Equation.FromPoints(UVpoints[cc][1], UVpoints[cc][2]);
            for(int y =MinY; y < YRange;y++){
                //Iterate the y.
                if(y < _12mid.Y){
                    //Before change in Gradient
                    float xUpper = p1_p12mid.SolveX(y);
                    int x =(int)p0_p1.SolveX(y);
                    while(x <= xUpper){
                        result.Append((new Point(x, y), buffer.GetPixel(x, y)));
                        x++;
                    }
                }else{
                    //After change in Gradient
                    float xUpper = p1_p2.SolveX(y);
                    int x =(int)p1_p12mid.SolveX(y);
                    while(x <= xUpper){
                        result.Append((new Point(x, y), buffer.GetPixel(x, y)));
                        x++;
                    }
                }
            }
        }
        if(textureData == null && Append == true){
            Texturer.textureData = [];
            Texturer.textureData.Append(result);
        }
        return result;
    }
    public static int Max(int[] numbers){
        int scope = 0;
        foreach(int i in numbers){
            scope = i > scope? i : scope;
        }
        return scope;
    }
    public static int Min(int[] numbers){
        int scope = 0;
        foreach(int i in numbers){
            scope = i < scope? i : scope;
        }
        return scope;
    }

    //! RndrComponent overrides
    public override int Size{get{return 0;}}
    public override byte[] ToByte(){
        List<byte> result = BitConverter.GetBytes(this.filePath.Get().Length).ToList();
        result.AddRange(Encoding.UTF8.GetBytes(this.filePath));
        result.AddRange(BitConverter.GetBytes(this.DataStart));
        result.AddRange(BitConverter.GetBytes(this.DataStart));
        return result.ToArray();
    }
    public override void FromBytes(byte[] bytes){
        this.filePath = (Path)StorageManager.ReadString(bytes, Encoding.Unicode);
    }
    public override void Initialise(){_Initialise();}
    void _Initialise(){
        this.Initialised = true;
        FileInfo finfo = new FileInfo(filePath);
        buffer = (Bitmap)Image.FromFile(filePath);
    }
    public new void Dispose(bool disposing = true){
        base.Dispose(disposing);
        if (disposing){
            this.Initialised =false;
			buffer = new Bitmap(1, 1);
        }
    }













    /*
        public override int Size{get{return 0;}}
    public override byte[] ToByte(){
        return Encoding.UTF8.GetBytes(this.filePath);
    }
    public override Texturer FromByte(byte[] bytes){
        return new Texturer(Encoding.UTF8.GetString(bytes));
    }
    
    public static List<(Point[] points, Color[])> TextureData;
    public FileInfo finfo;
    /// <remarks>DO NOT ACCESS, , ALL WILLY NILLY.</remarks>
    byte[] buffer;
    /// <remarks>DO NOT ACCESS, ALL WILLY NILLY.</remarks>
    string buffer_;
    byte[] img{get; set;}
    int index;
    public (int width, int height) imgDimensions{get; private set;}
    Path filePath;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Texturer(){
		;
		Initialise();
	}

    public Texturer(string? path = null){
        filePath = path == null? (Path)(StorageManager.filePath+@"Cache\Images\Grass Block.png"): (Path)path;
        Bitmap image = (Bitmap)Image.FromFile(filePath);
        imgDimensions = (image.Width, image.Height);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Initialise(){
		buffer = this.img;
        if(filePath != buffer_){
			Bitmap bmp;
            using(MemoryStream mS = new MemoryStream()){
                try{
                    bmp = (Bitmap)Image.FromFile(filePath);
					bmp.Save(mS, ImageFormat.Bmp);
					this.imgDimensions = (bmp.Width, bmp.Height);
                }catch(FileNotFoundException){
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory+@"Cache\Images\");
                    File.Create(AppDomain.CurrentDomain.BaseDirectory+@"Cache\Images\GrassBlock.png");
                    this.filePath.Update(AppDomain.CurrentDomain.BaseDirectory+@"Cache\Images\GrassBlock.png");
					bmp = (Bitmap)Image.FromFile(AppDomain.CurrentDomain.BaseDirectory+@"Cache\Images\GrassBlock.png");
                    bmp.Save(mS, ImageFormat.Bmp);
                }
                this.img = mS.ToArray();
            }
        }else{this.img = buffer;}
		this.finfo = new FileInfo(filePath);
    }
    public new void Dispose(bool disposing = true){
        if (disposing){
			if(finfo != null){
            	finfo.Delete();
			}
			imgDimensions = (0, 0);
			img = [];
			buffer = [];
			buffer_ = "";
        }
        base.Dispose(disposing);
    }
    public void Reset(string Path){
        this.Dispose(true);
        Initialise();
    }
    public (Point p, Color c)[]? Texture(Polygon p){
        return Texture(p.UVPoints);
    }
    public (Point p, Color c)[] Texture(Point[] UVPoints){
        //Need to use PolyEquation to simulate the bounds of the Polygon on the image.
        (Point p, Color c)[] result = [];
        for(int cc =0;cc < UVPoints.Length;cc++){
            int index = (UVPoints[cc].X + (UVPoints[cc].Y*imgDimensions.width))*4;
            //TODO Need to iterate through the shape created.
            //TODO Use the new Polyequation and equation classes to constrain how Polygon data is recieved.
            //Split the triangle into 2, make 2 right angle triangles.
            //
        }
        return result;
    }
    */
}

class RigidBdy : Rndrcomponent{
    ///<summary>
    /// This is the size of the class 
    ///</summary>
    public override int Size {get{return (sizeof(int)*2)+(sizeof(float)*3);}}
    public static int size {get{return new RigidBdy().Size;}}
    public int Mass;
    public int Speed;
    public Vector3 velocity;
    public RigidBdy(int m = 1){
        this.Mass = m;
    }
    public RigidBdy(){
        this.Mass = 100;
    }
    public override void FromBytes(byte[] bytes){
        this.Mass =BitConverter.ToInt32(bytes);
    }
    public override byte[] ToByte(){
        return BitConverter.GetBytes(this.Mass);
    }
    public override void Initialise(){throw new NotImplementedException();}
}