using System.Drawing.Imaging;
using System.Text;

/// <summary>
///  This abstract class is created to act as a link between different component types.
/// </summary>
abstract class Rndrcomponent{
    public abstract int Size{get;}
    public abstract byte[] ToByte();
    public abstract Rndrcomponent FromByte(byte[] bytes);
    internal void Dispose(bool disposing){}
    public abstract void Initialise();
}
class Empty : Rndrcomponent{
    public override Rndrcomponent FromByte(byte[] bytes){return this;}
    public override int Size{get{return 0;}}
    public override byte[] ToByte(){return new byte[0];}
    public override void Initialise(){return;}

}
class Equation{
    public float Gradient;
    public float Y_Intercept;
    public Equation(Point a, Point b){
        this.Gradient = (b.Y - a.Y)/(b.X - a.X);
        //Y = mx+c
        //a.Y - Gradient * a.X = c
        this.Y_Intercept = a.Y - (this.Gradient * a.X);
    }
}
class Texturer : Rndrcomponent{
    public override int Size{get{return 0;}}
    public override byte[] ToByte(){
        return Encoding.UTF8.GetBytes(this.filePath);
    }
    public override Texturer FromByte(byte[] bytes){
        return new Texturer(Encoding.UTF8.GetString(bytes));
    }
    
    public FileInfo finfo;
    /// <remarks>DO NOT ACCESS, , ALL WILLY NILLY.</remarks>
    byte[] buffer;
    /// <remarks>DO NOT ACCESS, ALL WILLY NILLY.</remarks>
    string buffer_;
    byte[] img{get; set;}
    public (int width, int height) imgDimensions{get; private set;}
    Path filePath;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Texturer(){
		filePath = new Path(StorageManager.filePath + @"Cache\Images\Grass Block.png", [".png", ".bmp", ".jpeg"], false);
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
        this.filePath = (Path)Path;
        img = File.ReadAllBytes(filePath);
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
    public override void Initialise(){throw new NotImplementedException();}
}