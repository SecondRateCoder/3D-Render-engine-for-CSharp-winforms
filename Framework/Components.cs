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
    /// <summary>Initialise this instance, filling it's data stores with the necessary information.</summary>
    public abstract void Initialise();


    bool disposed;
    public void Dispose(){
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    /// <summary>Clear this instance, emptying all data stores to null or 0.</summary>
    /// <param name="disposing">Should this function Dispose this instance.</param>
    protected virtual void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                // Release managed resources here (if any)
            }

            // Release unmanaged resources here (if any)

            disposed = true;
        }
    }
    ~Rndrcomponent(){
        if(!this.disposed){
            Dispose(false); // Dispose only unmanaged resources
            this.disposed = true;
        }
    }
}
class EmptyComponent : Rndrcomponent{
    public static Rndrcomponent Empty = new EmptyComponent();
    public override void FromBytes(byte[] bytes){return;}
    public override unsafe int Size{get{return 0;}}
    public override byte[] ToByte(){return new byte[0];}
    public override void Initialise(){return;}

}
//TODO Atp i might as well just move the texturer into a whole new file cus ts is extensive.
class Texturer : Rndrcomponent{
    static Texturer(){textureData = [];}
    public static TextureDatabase textureData;
    TextureStyles tS;
    public void UpdateTextureStyle(TextureStyles tS){this.tS = tS;}
    /// <summary>Store the image file in this before Initialising.</summary>
    Bitmap buffer;
    Path filePath;
    bool Initialised;
    bool _Textured;
    public bool isEvenTextured{get{return _Textured;} set{if(value){_Textured = true;}else{this.Dispose(true);  this._Textured = false;}}}
    int DataStart;
    int DataEnd;
#pragma warning disable CS8618
    public Texturer(){
        this.filePath = new Path(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\GrassBlock.png", [".bmp", ".jpeg", ".png"], false);
        this.DataEnd =0;
        this.tS = TextureStyles.Empty;
        this.DataStart =0;
    }
#pragma warning restore CS8618
    public Texturer(string? filePath = null, TextureStyles? tS = null){
        this.filePath = filePath == null? new Path(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\GrassBlock.png", [".bmp", ".jpeg", ".png"], false):
        new Path(filePath, [".bmp", ".jpeg", ".png"], false);
        this.tS = tS?? TextureStyles.Empty;
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
    public TextureDatabase Texture(Mesh m, bool Append = true){
        List<PointF[]> uvPoint = new List<PointF[]>(m.Count);
        for(int cc =0; cc < m.Count;cc++){
            uvPoint[cc] = m[cc].UVPoints;
        }
        return this.Texture(uvPoint, m, Append);
    }
    /// <summary>
    /// This generates a TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints. 
    /// </summary>
    /// <param name="UVpoints">The List of 3 Point elements that represent the space taken for the texture.</param>
    /// <param name="Append">Should this function append result to the static Texturer.texturerData buffer, <see cref="Texturer.textureData.Count"/></param>
    /// <returns>A TextureData dataset which contains the texture data of each set of 3 Points in each element in UVPoints.</returns>
    /// <remarks>This function is CPU intensive and should be used sparingly.</remarks>
    public TextureDatabase Texture(List<PointF[]> UVpoints, Mesh m, bool Append = true){
        ObjectDisposedException.ThrowIf(!this.Initialised, this);
        TextureDatabase result = [];
        //Does this component have a texture, if not then just return the empty database.
        if(!this.isEvenTextured){return result;}
        //!If this component has already appended it's data to the static textureData list then that is lovely, all this intensive math can chill.
        if((this.DataStart != 0) && (this.DataEnd != 0)){
            return this.Texture();
        }else{
            Equation? p0_p1 = null;
            Equation? p1_p2 = null;
            Equation? p0_p2 = null;
            //!If this component has not already appended it's data to the static textureData list then that is peak, ur stuff gon hafta tank ts.
            for(int cc =0; cc < UVpoints.Count;cc ++){
                (int a, int b) Section = (result.Count, 0);
                UVpoints[cc] = [.. CustomSort.SortPointArray_ByY(UVpoints[cc]).Result];
                //Find the length of the 2d polygon then find the range
                float YRange = (int)(UVpoints[cc][2].Y -UVpoints[cc][0].Y);
                //Point 0 (UVPoints[cc][0]) to Point 1 (UVPoints[cc][1])
                p0_p1 = Equation.FromPoints(UVpoints[cc][0], UVpoints[cc][1]);
                //Point 1 (UVPoints[cc][1]) to the midPoint
                p1_p2 = Equation.FromPoints(UVpoints[cc][1], UVpoints[cc][2]);
                //Point 1 (UVPoints[cc][1]) to Point 2 (UVPoints[cc][2])
                p0_p2 = Equation.FromPoints(UVpoints[cc][0], UVpoints[cc][2]);
                for(float y =UVpoints[cc][0].Y; y < YRange;y++){
                    //Iterate the y.
                    if(y < UVpoints[cc][1].Y){
                        //The furthest, closest to 0.
                        float xLower = p0_p1.Value.SolveX(y);
                        //The bounding limit of the line.
                        int xUpper =(int)p0_p2.Value.SolveX(y);
                        while(xLower <= xUpper){
                            result.Append((TextureDatabase.TexturePoint)(new PointF((int)xLower, y), buffer.GetPixel((int)xLower, (int)y)));
                            xLower++;
                        }
                    }else{
                        //After change in Gradient
                        float xUpper = p1_p2.Value.SolveX(y);
                        int x =(int)p0_p2.Value.SolveX(y);
                        while(x <= xUpper){
                            result.Append((TextureDatabase.TexturePoint)(new PointF(x, y), buffer.GetPixel(x, (int)y)));
                            x++;
                        }
                    }
                }
                Section.b = result.Count;
                result.AttachSectionBounds(Section);
            }
#pragma warning disable CS8629 // Nullable value type may be null.
            result = TextureStyles.Apply(tS, result, m, p0_p1.Value, p0_p2.Value, p1_p2.Value);
#pragma warning restore CS8629 // Nullable value type may be null.
        }
        if(Append){
            this.DataStart = Texturer.textureData.Count;
            Texturer.textureData.Append(result);
            this.DataEnd = Texturer.textureData.Count;
        }
        return result;
    }
    public static float Max(float[] numbers){
        int scope = 0;
        foreach(int i in numbers){
            scope = i > scope? i : scope;
        }
        return scope;
    }
    public static float Min(float[] numbers){
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
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public override unsafe int Size => sizeof(RigidBdy);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
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