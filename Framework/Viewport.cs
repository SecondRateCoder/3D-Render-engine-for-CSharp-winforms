
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    public static List<gameObj> worldData = new List<gameObj>();
    public static List<Camera> cams = new List<Camera>(){new Camera()};
    public static int camIndex{get; private set;}
    public static List<Light> lights = new List<Light>(){new Light(Vector3.Zero, Color.White, 15)};
    public static List<int> activeLights = [0];
    public static bool TrySetCam(int index){
        if(index < cams.Count && index >= 0){
            camIndex = index;
            return true;
        }
        return false;
    }
    public static void AddLight(int light){
        activeLights.Add(light);
    }
    public static void Call(){
        Vector3 camPos = 0f-cams[camIndex].Position;
        Vector3 camRot = 0f-cams[camIndex].Rotation;
        foreach(gameObj gO in worldData){
            gO.Translate(camPos, camRot, false);
        }
    }
}

/// <summary>
///  This class represents the viewport of the user, the Camera property is what will be used to select the viewpoint.
/// </summary> 
static class ViewPort{
    static byte[] backBuffer= [];
    static WriteableBitmap bmp = WriteableBitmap.Empty;
    public static Bitmap Get(){return bmp.Get();}
    public static void Update(WriteableBitmap Bmp){
        bmp = Bmp;
        backBuffer = new byte[bmp.pixelWidth * bmp.pixelHeight * 4];
    }
    public static void Clear(Color c){
        for(int cc =0;cc < bmp.Count;cc+=4){
            backBuffer[cc] = c.A;
            backBuffer[cc+1] = c.R;
            backBuffer[cc+2] = c.G;
            backBuffer[cc+3] = c.B;
        }
    }
    public static void Present(){
        bmp.Update(backBuffer);
    }
    public static Point[] DrawBLine(Point a, Point b){
        int dx = Math.Abs(b.X - a.X);
        int dy = Math.Abs(b.Y - a.Y);
        int sx = (a.X < b.X)? 1 : -1;
        int sy = (a.Y < b.Y)? 1: -1;
        int err = dx - dy;
        List<Point> points =[];
        while(true){
            points.Add(a);
            if((a.X == b.X) && (a.Y == b.Y)){break;}
            int e2 = err*2;
            if(e2 > -dy){err -= dy;     a.X += sx;}
            if(e2 < dx){err += dx;      a.Y += sy;}
        }
        return [.. points];
    }
    /// <summary>
    ///  This property represents the bounds of the form.
    /// </summary>
    ///<remarks>The left(l) and bottom(b) properties are set to 0, 
    /// whilst the right(r) and top(t) properties hold the window width and height, respectively.</remarks>
    public static readonly (float r, float l, float b, float t) boundary = (Entry.f.DisplayRectangle.Width, 0, 0, Entry.f.DisplayRectangle.Height);
    /// <summary>
    ///  This property represents the 4x4 Matrix that will convert all viewable vector positions to Perspective projection.
    /// </summary>
    static float[,] PPMatrix {get{
        if(_ppMatrixDirty){
            _ppMatrix = new float[4, 4]{
                {(float)(1/(boundary.r/boundary.t)*Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f, 0f},
                {0f, (float)(1/Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f},
                {0f, 0f, World.cams[World.camIndex].far/(World.cams[World.camIndex].far-World.cams[World.camIndex].near), -World.cams[World.camIndex].far*World.cams[World.camIndex].near/(World.cams[World.camIndex].far-World.cams[World.camIndex].near)},
                {0f, 0f, 1f, 0f}};
            _ppMatrixDirty = false;
        }
        return _ppMatrix;
    }}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static float[,] _ppMatrix;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static bool _ppMatrixDirty;
    public static void MarkPPMatrixDirty(){
        _ppMatrixDirty = true;
    }



    /// <summary>
    ///  This takes the gameObj data from the static World class.
    /// </summary> 
    /// <remarks>If <see cref="World.worldData.Count"/> == 0, then it creates a cube to be rendered.</remarks>
    public static (Point p, Color c)[] TransformToScreenSpace_Normalised(){
        (TextureDatabase textureData, Polygon[] polygonData, int[] positionData) buffer = GetTextures_PolygonData();
        Polygon[] MultipliedPolygons = new Polygon[buffer.polygonData.Length];
        int pointer = 0;
        foreach(Polygon p in buffer.polygonData){
            pointer++;
            MultipliedPolygons[pointer] = Multiply(p);
        }
        return [];
        
    }
    static (TextureDatabase, Polygon[], int[]) GetTextures_PolygonData(){
        TextureDatabase TextureData = [];
        List<Polygon> buffer = [];
        List<int> positionData = [0];
        if(World.worldData.Count == 0){
            //If there is nothing to be drawn.
            gameObj gO = new gameObj(Vector3.Zero, Vector3.Zero, true, Polygon.Mesh(1, 0, 1, 4));
            gO.AddComponent(typeof(Texturer), new Texturer(@"C:\Users\olusa\OneDrive\Documents\GitHub\3D-Render-engine-for-CSharp-winforms\Cache\Images\GrassBlock.png"));
            buffer = ((Polygon[])gO.Children.ViewPortClip()).ToList();
            gO.Texture();
            buffer =[];
            positionData = [0];
        }else{
            foreach(gameObj gO_ in World.worldData){
                //Copy out the gameObj, 
                // using the multiply func to normalise it, 
                // then texture with that.
                gameObj gO = gO_.Copy();
                gO.Children.ViewPortClip();
                gO.Children.Foreach(Multiply);
                gO.Texture();
            }
            ViewPort.bmp.Initialise(TextureData, Entry.f.Width, Entry.f.Height);
        }
        return (TextureData, buffer.ToArray(), positionData.ToArray());
    }

    /// <summary>
    /// Will return a polygon whose vector co-ordinaates can be directly converted to points.
    /// </summary>
    /// <returns>A polygon whose vector co-ordinaates can be directly converted to points.</returns>
    public static Polygon Multiply(Polygon polygon){
        return new Polygon(Multiply(polygon.A), Multiply(polygon.B), Multiply(polygon.C));
    }
    static Vector3 Multiply(Vector3 v){
        v = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 0],
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 1],
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 2]
            );
        float wC = PPMatrix[0, 3] * v.X + PPMatrix[1, 3] * v.Y + PPMatrix[2, 3] * v.Z + PPMatrix[3, 3];
        v /= wC;
        //Normalise then multiply by the form dimensions to scale it.
        v.Normalise();
        v= new Vector3(v.X/v.Z * Entry.f.Bounds.Width, v.Y/v.Z  * Entry.f.Bounds.Height, 0);
        return v;
    }
}