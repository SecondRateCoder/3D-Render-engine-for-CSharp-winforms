
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    public static List<gameObj> worldData = new List<gameObj>();
    public static List<Camera> cams = new List<Camera>(){new Camera()};
    public static int camIndex{get; private set;}
    public static List<Light> lights = new List<Light>(){new Light(Vector3.Zero, Color.White, 15)};
    public static List<int> activeLights = [0];
    public static void setCam(int index){
        if(index < cams.Count){
            camIndex = index;
        }
    }
    public static void SetLights(IEnumerable<int> lights){
        activeLights = lights.ToList();
    }
    public static void AddLight(int light){
        activeLights.Append(light);
    }
    public static void Call(){
        Vector3 camPos = 0f-cams[camIndex].Position;
        Vector3 camRot = 0f-cams[camIndex].Rotation;
        List<gameObj> buffer = [];
        foreach(gameObj gO in worldData){
            gameObj gO_ = gO.Copy();
            gO_.Translate(camPos, camRot, false);
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
            backBuffer[cc] = c.R;
            backBuffer[cc] = c.G;
            backBuffer[cc] = c.B;
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
    static float[,] PPMatrix {get;} = new float[4, 4]{
        {(float)(1/(boundary.r/boundary.t)*Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f, 0f},
        {0f, (float)(1/Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f},
        {0f, 0f, World.cams[World.camIndex].far/(World.cams[World.camIndex].far-World.cams[World.camIndex].near), -World.cams[World.camIndex].far*World.cams[World.camIndex].near/(World.cams[World.camIndex].far-World.cams[World.camIndex].near)},
        {0f, 0f, 1f, 0f}};




    /// <summary>
    ///  This takes the gameObj data from the static World class.
    /// </summary> 
    /// <remarks>If <see cref="World.worldData.Count"/> == 0, then it creates a cube to be rendered.</remarks>
    public static (Point p, Color c)[] TransformToScreenSpace_Normalised(){
        (TextureDatabase textureData, Polygon[] polygonData, Polygon[] positionData) buffer = GetTextures_PolygonData();
        Polygon[] MultipliedPolygons = new Polygon[buffer.polygonData.Count]
        int pointer = 0;
        foreach(Polygon p in buffer.polygonData, pointer++){
            MultipliedPolygons[pointer] = Multiply(p);
        }
        return [];
        
    }
    static (TextureDatabase, List<Polygon>, List<int>) GetTextures_PolygonData(){
        TextureDatabase TextureData = [];
        List<Polygon> buffer = [];
        List<int> positionData = [0];
        if(World.worldData.Count == 0){
            //If there is nothing to be drawn.
            gameObj gO = new gameObj(Vector3.Zero, Vector3.Zero, true, Polygon.Mesh(1, 0, 1, 4));
            gO.AddComponent(typeof(Texturer), new Texturer(@"C:\Users\olusa\OneDrive\Documents\GitHub\3D-Render-engine-for-CSharp-winforms\Cache\Images\GrassBlock.png"));
            buffer = ((Polygon[])gO.Children.ViewPortClip()).ToList();
            for(int i = 0;i < buffer.Count;i++){gO.GetComponent<Texturer>().Texture([buffer[i].UVPoints]);}
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
        return (TextureData, buffer);
    }

    /// <summary>
    /// Will return a polygon whose vector co-ordinaates can be directly converted to points.
    /// </summary>
    /// <returns>A polygon whose vector co-ordinaates can be directly converted to points.</returns>
    public static Polygon Multiply(Polygon polygon){
        Polygon CalcBuffer = new Polygon();
        //For Point A
        CalcBuffer.A = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+PPMatrix[3, 0],
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+PPMatrix[3, 1],
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+PPMatrix[3, 2]
            );
        float wA = PPMatrix[0, 3] * CalcBuffer.A.X + PPMatrix[1, 3] * CalcBuffer.A.Y + PPMatrix[2, 3] * CalcBuffer.A.Z + PPMatrix[3, 3];
        CalcBuffer.A /= wA;
        //Normalise then multiply by the form dimensions to scale it.
        CalcBuffer.A.Normalise();
        CalcBuffer.A = new Vector3(CalcBuffer.A.X/CalcBuffer.A.Z * Entry.f.Bounds.Width, CalcBuffer.A.Y/CalcBuffer.A.Z  * Entry.f.Bounds.Height, 0);
        //For Point B
        CalcBuffer.B = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+PPMatrix[3, 0],
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+PPMatrix[3, 1],
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+PPMatrix[3, 2]
            );
        float wB = PPMatrix[0, 3] * CalcBuffer.B.X + PPMatrix[1, 3] * CalcBuffer.B.Y + PPMatrix[2, 3] * CalcBuffer.B.Z + PPMatrix[3, 3];
        CalcBuffer.B /= wB;
        //Normalise then multiply by the form dimensions to scale it.
        CalcBuffer.B.Normalise();
        CalcBuffer.B = new Vector3(CalcBuffer.B.X/CalcBuffer.B.Z * Entry.f.Bounds.Width, CalcBuffer.B.Y/CalcBuffer.B.Z  * Entry.f.Bounds.Height, 0);
        //For Point C
        CalcBuffer.C = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+PPMatrix[3, 0],
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+PPMatrix[3, 1],
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+PPMatrix[3, 2]
            );
        float wC = PPMatrix[0, 3] * CalcBuffer.C.X + PPMatrix[1, 3] * CalcBuffer.C.Y + PPMatrix[2, 3] * CalcBuffer.C.Z + PPMatrix[3, 3];
        CalcBuffer.C /= wC;
        //Normalise then multiply by the form dimensions to scale it.
        CalcBuffer.C.Normalise();
        CalcBuffer.C= new Vector3(CalcBuffer.C.X/CalcBuffer.C.Z * Entry.f.Bounds.Width, CalcBuffer.C.Y/CalcBuffer.C.Z  * Entry.f.Bounds.Height, 0);
        return CalcBuffer;
    }
}