
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    public delegate void Orient_(Vector3 position, Vector3 rotation, bool PrivateTranslation = false);
    public static Orient_ WorldOrient;
    public static List<gameObj> worldData = new List<gameObj>();
    public static List<Camera> cams = new List<Camera>(){new Camera()};
    public static int camIndex{get; private set;}
    public static List<Light> lights = new List<Light>(){new Light(Vector3.zero, Color.White, 15)};
    public static List<int> lightIndex = [0];
    public static void setCam(int index){
        if(index < cams.Count){
            camIndex = index;
        }
    }
    public static void SetLights(IEnumerable<int> lights){
        lightIndex = lights.ToList();
    }
    public static void AddLight(int light){
        lightIndex.Append(light);
    }
}

/// <summary>
///  This class represents the viewport of the user, the Camera property is what will be used to select the viewpoint.
/// </summary> 
static partial class ViewPort{
    /// <summary>
    ///  This property represents the bounds of the form.
    /// </summary>
    ///<remarks>The left(l) and bottom(b) properties are set to 0, 
    /// whilst the right(r) and top(t) properties hold the window width and height, respectively.</remarks>
    public static readonly (float r, float l, float b, float t) boundary = (Camera.form.DisplayRectangle.Width, 0, 0, Camera.form.DisplayRectangle.Height);
    /// <summary>
    ///  This property represents the 4x4 Matrix that will convert all viewable vector positions to Perspective projection.
    /// </summary>
    static float[,] PPMatrix {get;} = new float[4, 4]{
        {(float)(1/(boundary.r/boundary.t)*Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f, 0f},
        {0f, (float)(1/Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f},
        {0f, 0f, World.cams[World.camIndex].far/(World.cams[World.camIndex].far-World.cams[World.camIndex].near), -World.cams[World.camIndex].far*World.cams[World.camIndex].near/(World.cams[World.camIndex].far-World.cams[World.camIndex].near)},
        {0f, 0f, 1f, 0f}};




    /// <summary>
    /// <summary>
    ///  This takes the gameObj data from the static World class.
    /// </summary> 
    /// <remarks>If <seealso cref="World.worldData.Count" == 0, then it creates a cube to be rendered./></remarks>
    public async static Task<(Point p, Color c)[]> Convert_(){
        List<Polygon> buffer = [];
        //The points in TextureData that represent where a new ogject's data starts, 
        //it soud align with the gameObjs indexing in World.worldData.
        List<int> positionData = [0];
        List<(Point p, Color c)> TextureData = [];
        if(World.worldData.Count == 0){
            gameObj gO = new gameObj(Vector3.zero, Vector3.zero, true, Polygon.Mesh(1, 0, 1, 4));
            gO.AddComponent(typeof(Texturer), new Texturer(@"C:\Users\olusa\OneDrive\Documents\GitHub\3D-Render-engine-for-CSharp-winforms\Cache\Images\GrassBlock.png"));
            await Task.Run(() => {
                buffer = ((Polygon[])gO.Children.ViewPortClip()).ToList();
                for(int i = 0;i < buffer.Count;i++){TextureData = gO.GetComponent<Texturer>().Texture(buffer[i]).ToList();}
            });
            buffer =[];
            positionData = [0];
        }else{
            await Task.Run(() => {
                foreach(gameObj gO in World.worldData){
                    Polygon[] buffer_ = (Polygon[])gO.Children.ViewPortClip();
                    buffer.Concat((Polygon[])gO.Children.ViewPortClip());
                    int Position = TextureData.Count+1;
                    bool HasTexture = gO.HasComponent<Texturer>();
                    for(int i = 0;i < buffer_.Length;i++){
                        if(HasTexture){positionData.Append(TextureData.Count+1);}
                        if(HasTexture){TextureData.Concat(gO.GetComponent<Texturer>().Texture(buffer_[i]));}else{continue;}
                    }
                }
            });
        }
        return [];
        //TextureData has been fully filled.
        //Transform each Polygon to screen-space and using th normalised point and normalised Polygon to get the position of the color point;
        
    }
        /*
        
        
    
    /// <summary>
    /// <summary>
    ///  This overload takes the parameter data from the static World class.
    /// </summary> 
    /// <remarks>If <seealso cref="World.worldData.Count" == 0, then it creates a cube to be rendered./></remarks>
    static (Point p, Color color)[] Convert_(){
        if(World.worldData.Count == 0){gameObj gO = new gameObj(Vector3.zero, Vector3.zero, Polygon.Mesh(1, 0, 1, 4));}
        List<(Color[], Polygon)> parameter = List<(Color[], Polygon)>(gO.Children.Count);
        Color[] colors = new Color[(int)gO.Children.Volume];
        for(int cc_=0;cc_ < colors.Length;cc_++){
            colors[cc_] = Color.Grey;
        }
        for(int cc =0;cc < parameter.Count;cc++){
            parameter.Add((colors, gO.Children[cc]));
        }
        return Convert_(parameter);
    }
    /// <summary>This overload turns Gameobjs into polygons then runs the main overload(Convert_()).</summary> 
    public static (Point p, Color color)[] Convert_(List<gameObj> objs = null){
        List<(Color[], Polygon)> parameter = new List<(Color[], Polygon)>();
        Color[] colors = new Color[(int)gO.Children.Volume];
        for(int cc_=0;cc_ < colors.Length;cc_++){
            colors[cc_] = Color.Grey;
        }
        for(int cc = 0;cc < objs.Count;cc++){
            for(int cc_ = 0; cc_ < objs[cc].Children.Count;cc_++){
                parameter.Add((colors, objs[cc].Children[cc_]));
            }
        }
        return Convert_(parameter);
    }


    /// <summary>The main Convert_() overload, This function converts the world 3d enviroment into a 2d representation.</summary> 
    public static (Point p, Color color)[] Convert_(List<(Color[] texture, Polygon poly)> polygons){
        (Point p, Color color)[] result = new (Point p, Color color)[polygons.Count];
        if(polygons == null){
            throw new Exception("Can't be null, you Monkey. \nDo you know how much fricking code I had to write to implement texturing \nDon't fucing do this");
        }else{
            //Apply camera transform and clip each polygon.
            for(int cc = 0; cc < polygons.Count; cc++){
                if(Vector3.GetRotation(polygons[cc].poly.Normal, World.cams[World.camIndex].Position).Magnitude<90){
                    polygons[cc].poly.Translate(World.cams[World.camIndex].Position, 0f-World.cams[World.camIndex].Position, 0f-World.cams[World.camIndex].Rotation);
                    polygons[cc] = Multiply(polygons[cc]);
                    Point[] array = DrawBLine((Point)polygons[cc].A, (Point)polygons[cc].B);
                    array.AddRange(DrawBLine((Point)polygons[cc].B, (Point)polygons[cc].C));
                    array.AddRange(DrawBLine((Point)polygons[cc].C, (Point)polygons[cc].A));
                    foreach(Point p in array){
                        result[cc] = (p, Color.Grey);
                    }
                }else{
                    cc++;
                }
                polygons[cc] = (polygons[cc].texture, Polygon.PolyClip(polygons[cc].poly, Vector3.zero, World.cams[World.camIndex].far));
            }
        }
        return result;
    }
        
        */

    /// <summary>
    /// Will return a polygon whose vector co-ordinaates can be directly converted to points.
    /// </summary>
    /// <returns>A polygon whose vector co-ordinaates can be directly converted to points.</returns>
    static Polygon Multiply(Polygon polygon){
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
        CalcBuffer.A = new Vector3(CalcBuffer.A.X/CalcBuffer.A.Z * Camera.form.Bounds.Width, CalcBuffer.A.Y/CalcBuffer.A.Z  * Camera.form.Bounds.Height, 0);
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
        CalcBuffer.B = new Vector3(CalcBuffer.B.X/CalcBuffer.B.Z * Camera.form.Bounds.Width, CalcBuffer.B.Y/CalcBuffer.B.Z  * Camera.form.Bounds.Height, 0);
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
        CalcBuffer.C= new Vector3(CalcBuffer.C.X/CalcBuffer.C.Z * Camera.form.Bounds.Width, CalcBuffer.C.Y/CalcBuffer.C.Z  * Camera.form.Bounds.Height, 0);
        return CalcBuffer;
    }




        static byte[] backBuffer;
    static WriteableBitmap bmp;
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
}