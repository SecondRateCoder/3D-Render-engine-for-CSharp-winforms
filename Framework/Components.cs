using System.Collections;
using System.Text;
/// <summary>
///  This abstract class is created to act as a link between different component types.
/// </summary>
abstract class Rndrcomponent{
    public unsafe abstract int Size{get;}
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
class EmptyComponent : Rndrcomponent{
    public static Rndrcomponent Empty = new EmptyComponent();
    public override void FromBytes(byte[] bytes){return;}
    public override unsafe int Size{get{return 0;}}
    public override byte[] ToByte(){return new byte[0];}
    public override void Initialise(){return;}

}

class Texturer : Rndrcomponent{
    public static TextureDatabase textureData = [];
    /// <summary>Store the image file in this before Initialising.</summary>
    Bitmap buffer;
    Path filePath;
    bool Initialised;
    int DataStart;
    int DataEnd;
#pragma warning disable CS8618
    public Texturer(){
        this.filePath = new Path(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\Grass Block.png", [".bmp", ".jpeg", ".png"], false);
        this.DataEnd =0;
        this.DataStart =0;
    }
#pragma warning restore CS8618
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
    /// Gets the TextureData from the static textureData property.
    /// </summary>
    /// <returns>TextureData.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <see cref="DataStart"/> or <see cref="DataEnd"/> hold invalid values</exception>
    public TextureDatabase Texture(){
        ObjectDisposedException.ThrowIf(!this.Initialised, this);
        if(0 > (this.DataStart|this.DataEnd) | textureData.Count < this.DataEnd | this.DataStart == 0 | this.DataEnd == 0 | this.DataStart < this.DataEnd){throw new ArgumentOutOfRangeException();}
        TextureDatabase result =[];
        for(int cc =0; cc < this.DataEnd - this.DataStart;cc++){
            result.Append(textureData[cc + DataStart]);
        }
        return result;
    }
    /// <summary>
    /// This generates a TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints. 
    /// </summary>
    /// <param name="UVpoints">The List of 3 Point elements that represent the space taken for the texture.</param>
    /// <param name="Append">Should this function append result to the static Texturer.texturerData buffer, <see cref="Texturer.textureData.Count"/></param>
    /// <returns>A TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints.</returns>
    public TextureDatabase Texture(List<Point[]> UVpoints, bool Append = true){
        ObjectDisposedException.ThrowIf(!this.Initialised, this);
        TextureDatabase result = new(new List<(Point p, Color c)>());
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
        if(Append){
            this.DataStart = Texturer.textureData.Count;
            Texturer.textureData.Append(result);
            this.DataEnd = Texturer.textureData.Count;
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
    public override unsafe int Size{get{return (0);}}
    public override byte[] ToByte(){
        List<byte> result = BitConverter.GetBytes(this.filePath.Get().Length).ToList();
        result.AddRange(Encoding.UTF8.GetBytes(this.filePath));
        result.AddRange(BitConverter.GetBytes(this.DataStart));
        result.AddRange(BitConverter.GetBytes(this.DataStart));
        return result.ToArray();
    }
    public override void FromBytes(byte[] bytes){
        this.filePath = (Path)StorageManager.ReadString(bytes, Encoding.Unicode, bytes.Length - (sizeof(int) * 2));
        this.DataStart = StorageManager.ReadInt32(bytes, bytes.Length - Encoding.Default.GetByteCount(filePath));
        this.DataEnd = StorageManager.ReadInt32(bytes, (bytes.Length - Encoding.Default.GetByteCount(filePath)) + sizeof(int));
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
}

class RigidBdy : Rndrcomponent{
    ///<summary>
    /// This is the size of the class 
    ///</summary>
    public override unsafe int Size {get{return sizeof(RigidBdy);}}
    public static int size {get{return new RigidBdy().Size;}}
    public RigidBdyMetadata MetaData{get; private set;}
    public int Mass;
    public int Speed;
    Vector3 _velocity;
    public Vector3 velocity{
        get{return _velocity;}
        set{_velocity = value;}
    }
    public Vector3 TrueVelocity{get{return (this.Mass/2) * (this._velocity^2);}}
    public RigidBdy(int m = 1, PhysicsMaterial? pM = null, Collider? cS = null, ForceMode? fM = null){
        this.MetaData = RigidBdyMetadata.Default;
        this.MetaData.pM = pM == null?PhysicsMaterial.GlazedWood: pM;
        this.MetaData.cS = cS == null?Collider.Cube: cS;
        this.MetaData.fM = fM == null? ForceMode.Impulse: fM;
        this.Mass = m;
    }
    public RigidBdy(){
        this.Mass = 100;
        this.MetaData = RigidBdyMetadata.Default;
        this.MetaData.pM = PhysicsMaterial.GlazedWood;
        this.MetaData.cS = Collider.Cube;
        this.MetaData.fM = ForceMode.Impulse;
    }
    public override void FromBytes(byte[] bytes){
        this.Mass =BitConverter.ToInt32(bytes);
    }
    public override byte[] ToByte(){
        return BitConverter.GetBytes(this.Mass);
    }
    public override void Initialise(){throw new NotImplementedException();}



    public class RigidBdyMetadata{
        public static readonly RigidBdyMetadata Default = new(PhysicsMaterial.GlazedWood, Collider.Cube, ForceMode.Impulse);
        public PhysicsMaterial? pM;
        public Collider? cS;
        public ForceMode? fM;
        internal RigidBdyMetadata(PhysicsMaterial pM, Collider cS, ForceMode fM){
            this.pM = pM;
            this.cS = cS;
            this.fM = fM;
            this.disposed = false;
        }
        public RigidBdyMetadata((int f, int m) pM, int cS, int fM){
            this.pM = (PhysicsMaterial)pM;
            this.cS = (Collider?)cS ?? Collider.Cube;
            this.fM = (ForceMode?)fM ?? ForceMode.Impulse;
            disposed = false;
        }

        public void DefaultInitialise(){
            this.pM = PhysicsMaterial.GlazedWood;
            this.cS = Collider.Cube;
            this.fM = ForceMode.Impulse;
            disposed = false;
        }
        bool disposed;
        public void Disposed(bool disposing = true){
            if(disposing){
                this.pM = null;
                this.cS = null;
                this.fM = null;
                disposed = true;
            }
        }

        public void Apply(RigidBdy rB, Vector3 v){
            ObjectDisposedException.ThrowIf(disposed, this);
            if(this.fM == null){this.fM = ForceMode.Impulse;}
            this.fM.Apply(rB, v);
        }
    }
}