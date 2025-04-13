using System.Drawing;
using System.Text;
using System.Timers;

/// <summary>
///  This abstract class is created to act as a link between different component types.
/// </summary>
abstract class Rndrcomponent{
    public abstract int Size{get;}
    public abstract byte[] ToByte();
    public abstract Rndrcomponent FromByte(byte[] bytes);
    internal void Dispose(bool disposing){}
}
class Empty : Rndrcomponent{
    public override Rndrcomponent FromByte(byte[] bytes){return this;}
    public override int Size{get{return 0;}}
    public override byte[] ToByte(){return new byte[0];}

}
class Texturer : Rndrcomponent{
    public override int Size{get;}
    public override byte[] ToByte(){
        return Encoding.UTF8.GetBytes(this.file);
    }
    public override Texturer FromByte(byte[] bytes){
        return new Texturer(Encoding.UTF8.GetString(bytes));
    }
    
    public FileInfo finfo;
    byte[] img{get{try{return Image.FromFile(file).RawFormat.GUID.ToByte();}catch{return Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"Cache\Images\Grass Block.png").RawFormat.GUID.ToByte();}}}
    public (int width, int height) imgDimensions{get; private set;}
    string fileProp;
    public string file{get{return fileProp;} set{fileProp = StorageManager.filePath+value;}}
    public Texturer(){file = StorageManager.filePath + @"Cache\Images\Grass Block.png";}
    public Texturer(string? path = null){
        file = path == null? StorageManager.filePath+@"Cache\Images\Grass Block.png": path;
        Bitmap image = (Bitmap)Image.FromFile(file);
        imgDimensions = (image.Width, image.Height);
        img = File.ReadAllBytes(file);
    }
    public new void Dispose(bool disposing = true){
        if (disposing && (finfo != null)){
            finfo.Delete();
            img = null;
        }
        base.Dispose(disposing);
    }
    public void Reset(string Path){
        this.Dispose(true);
        this.file = Path;
        img = File.ReadAllBytes(file);
    }
    public Color[]? Texture(Polygon p){
        return Texture(p.UVPoints);
    }
    public Color[]? Texture(Point[] UVPoints){
        Color[] result = new Color[0];
        Point p = new Point();
        for(int cc =0;cc < UVPoints.Length;cc++){
            int index = (UVPoints[cc].X + (UVPoints[cc].Y*imgDimensions.Width))*4;
            result.Add(Color.FromARGB(img[index], img[index+1], img[index+2], img[index+3]));
        }
        return result;
    }
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
    public override RigidBdy FromByte(byte[] bytes){
        return new RigidBdy(BitConverter.ToInt32(bytes));
    }
    public override byte[] ToByte(){
        return BitConverter.GetBytes(this.Mass);
    }
}
static class CollisionManager{
    static CollisionManager(){
        Entry.TUpdate += Collider;
    }
    ///<summary>
    /// This returns a 2d list that contains the data of colliding gameObjs, 
    ///</summary>
    ///<returns>The list is in the structure where for every gameObj(the 1st item on the 2nd dimension), 
    /// all the recorded collisions are recorded on every entry below the 1st.</returns>
    ///<remarks>This property runs an internal function when getting is attempted, saving the return to a list is suggested.</remarks>
    public static List<List<(gameObj gameObject, int Mass, Vector3 velocity)>> CollidingObjs{
        get{
            List<List<(gameObj gameObject, int Mass, Vector3 velocity)>> list = new List<List<(gameObj gameObject, int Mass, Vector3 velocity)>>();
            gameObj scope;
            for(int cc = 0;cc < World.worldData.Count;cc++){
                scope = World.worldData[cc];
                RigidBdy pM = scope.GetComponent<RigidBdy>();
                (gameObj, int, Vector3) t =(scope, pM.Mass, pM.velocity);
                list.Add(new List<(gameObj, int, Vector3)>());
                list.Last().Add(t);
                foreach(gameObj gO in World.worldData){
                    if(Vector3.GetDistance(scope.Position, gO.Position) < gO.CollisionRange+scope.CollisionRange){
                        RigidBdy pM_ = gO.GetComponent<RigidBdy>();
                        list[cc].Add((gO, pM_.Mass, pM_.velocity));
                    }
                }
            }
            return list;
    }}
    public static void Collider(object? sender, ElapsedEventArgs e){
        //Much of the properties in this tuple list is for the physics itself.
        List<List<(gameObj gameObject, int Mass, Vector3 velocity)>> list = CollidingObjs;
        for(int cc = 0;cc < CollidingObjs.Count;cc++){
            for(int i = 0; i < CollidingObjs[cc].Count;i++){
                
            }
        }
        /*Next is to make a function that gets the angle(as Vector3) of the colliding polygon
        Ill do this by finding the polygon closest to the colliding object then ill invert the velocity with that polygon as the normal
		get Vector3 v (position of the colliding obj)
		new float array a[children.Length]
		for Polygon p in Children{
			a[cc] = p.GetDistance(v)
		}
		foreach(float f in a){
			float scope;
			if (f > scope){
				scope = f
			}
			when completed{
				p = Children[cc]
			}
		}
		this.invert Velocity(Vector3 angle = p.GetAngle)
        */
    }
}