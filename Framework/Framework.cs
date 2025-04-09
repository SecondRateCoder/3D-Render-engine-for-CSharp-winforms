/// <summary>
/// Represents a 3-Dimensional object.
/// </summary>
class gameObj{
    public string Name;
    public Mesh Children;
    public Vector3 Position;
    private Vector3 rotation;
    /// <summary>
    ///  The list containing all of this gameObject's components
    /// </summary>
    List<(Type ogType, Rndrcomponent rC)> components;
    public int compLength{get{return this.components.Count;}}
    public int Size{
        get{
            int Vects = sizeof(float)*6*Children.Count;
            int Comps = 0;
            for(int cc =0;cc < (this.components.Count == 0? -1: this.components.Count);cc++){
                Comps += components[cc].rC.ToByte().Length;
            }
            return Comps+Vects;
        }
    }
    public Vector3 Rotation{
        get{
            return this.rotation;
        }
        set{
            this.rotation = this.Children != null && this.Children.Count > 0? value: Vector3.zero;
        }
    }
    public int CollisionRange{
        get{
            if(Children != null){
                float result = Children[0].Furthest(this.Position);
                for(int cc = 1; cc<Children.Count;cc++){
                    result = result > Children[cc].Furthest(this.Position)? result: Children[cc].Furthest(this.Position);
                }
                return (int)(result+.5);
            }else{
                return 0;
            }
            
        }
    }


    public void AddComponent<RComponent>(Type type, RComponent rC) where RComponent : Rndrcomponent{
        components.Add((type, rC));
    }
    public void AddComponents<RComponent>(IEnumerable<(Type type, RComponent rC)> rC) where RComponent : Rndrcomponent{
        foreach((Type type, RComponent rc) rc in rC){
            components.Add(rc);
        }
    }
    /// <summary>
    /// Get the type of the component at index.
    /// </summary>
    public Type GetComponentType(int index){
        return this.components[index].ogType;
    }
    /// <summary>
    ///  This is an overload of GetComponent<Component>()
    /// </summary>
    /// <typeparam name="Component">This is the RndrCcomponent type that should be returned.</typeparam>
    /// <returns>Returns the 1st instance of the expected component.</returns>
    public Component? GetComponent<Component>() where Component : Rndrcomponent, new(){
        for(int cc = 0; cc < components.Count;cc++){
            if(this.components[cc].ogType == typeof(Component)){
                return (Component?)this.components[cc].rC;
            }
        }
        return null;
    }

    public void newTexture(string? path = null){
        Texturer? t = this.GetComponent<Texturer>();
        if(t == null){return;}else{
            t.Reset(path == null?t.file: path);
        }
    }
    public void UpdateTexture(int index, Point[] uv){
        this.Children[index].UpdateTexture(uv);
    }
    public Color[] Texture(int index){
        return this.GetComponent<Texturer>().Texture(this.Children[index]);
    }
    public void Translate(Vector3 position, Vector3 rotation, bool PrivateTranslation = false){
        this.Position += position;
        this.Rotation += rotation;
        if(!PrivateTranslation){
            this.Children.Translate(this.Position - position, rotation);
        }
    }
    ///<summary>
    /// This method returns the Polygon closest to the Position parameter.
    ///</summary>
    ///<param name="Position">This is the parameter that the polygons will be compared against.</typeparam>
    ///<param name="LowerBd">This is the parameter that dictates how close polygons can get before the boolean isTouching is triggered.</typeparam>
    ///<returns>The polygon is the pPolygon that is closest to the contact position, whilst isTouching is whether the position is on the Polygon.</returns>
    public (Polygon polygon, bool isTouching) GetPolygon(Vector3 Position, float LowerBd = 0.5f){
        Polygon[] poly = new Polygon[this.Children.Count];
        Polygon scope = this.Children[0];
        float Dis = 0;
        for(int cc = 1; cc < poly.Length; cc++, Dis = Vector3.GetDistance(this.Children[cc].origin, Position)){
            if(Vector3.GetDistance(scope.origin, Position) < Dis){
                scope = this.Children[cc];
            }
        }
        Dis = Vector3.GetDistance(scope.origin, Position);
        return (scope, Dis <= LowerBd && Dis > 0? true: false);
    }


    public gameObj(Vector3 position, Vector3 rotation, IEnumerable<Polygon>? children = null, List<(Type, Rndrcomponent)>? Mycomponents = null, string? name = null){
        this.Position = position;
        if(children == null){
            this.Children.AddRange(Polygon.Mesh());
        }else{
            this.Children = new Mesh(children.ToList());
        }
        if(Mycomponents == null){
            this.components = new List<(Type ogType, Rndrcomponent rC)>();
        }else{
            this.components = Mycomponents;
        }
        this.Rotation = rotation;
        this.Name = name == null? $"{World.worldData.Count+1}": name;
        World.WorldOrient += this.Translate;
        World.worldData.Add(this);
    }
    

    public static gameObj operator +(gameObj parent, Polygon[] children){
        parent.Children.AddRange(children);
        return parent;
    }
    public static gameObj operator +(gameObj parent, Polygon child){
        parent.Children.Add(child);
        return parent;
    }
    /// <summary>
    ///  Adding two gameObjs together only appends the child obj's children to the parent obj's children.
    /// </summary>
    /// <param name="parent">The gameObj that will recieve the children.</param>
    /// <param name="child">The gameObj that will have it's children copied to the parent.</param>
    public static gameObj operator +(gameObj parent, gameObj child){
        parent.Children.AddRange(child.Children.ToList());
        return parent;
    }
}
class Camera{
    public Vector3 Position{get; private set;}
    public Vector3 Rotation{get; private set;}
	/// <summary>
	/// The resolution at which textures are applied to a mesh
	/// </summary>
	public int Resolution;
    /// <summary>
    ///  The tolerance for how small a polygon at the boundary of the viewport can be clipped before it's simply deleted.
    /// </summary>
    public float clipTol;
    ///<summary>The angle from the camera to the window.</summary>
    public float theta{get{return (float)(2*Math.Atan((ViewPort.boundary.r/2)/this.near_));}}
    float near_;
    ///<summary>The length from the camera to the near plane.</summary>
    public float near{get{return near_;} set{near_ = (near_ == 0 && value < far? value: near_);}}
    float fov_ = 0;
    ///<summary>
    /// The render distance.
    ///</summary>
    ///<remarks>This property cant be changed after being assigned.</remarks>
    public float far{get{return fov_;} set{fov_ = (fov_ == 0? value: fov_);}}
    private static Form form_;
    public static Form form{
        get{
            return form_;
        }
        set{
            while(Form.ActiveForm == null){
                Thread.Sleep(500);
            }
            form_ = Form.ActiveForm;
        }
    }
    public Camera(float Fov = 15f, Vector3? pos = null, Vector3? rot = null){
        this.far = Fov;
        this.Position = pos == null? Vector3.zero: pos.Value;
        this.Rotation = pos == null? Vector3.zero: pos.Value;
    }
    public void Translate(Vector3 position, Vector3 rotation){
        this.Position += position;
        this.Rotation += rotation;
    }
}



class Light{
    public Vector3 Source{get; private set;}
    public Color Colour{get; private set;}
    public int Radius;
    public int Intensity = 0;

    public Light(Vector3? source = null, Color? color = null, int mag = 10){
        this.Source = source == null? new Vector3(): source.Value;
        this.Colour = color == null? Color.WhiteSmoke: color.Value;
        this.Radius = (int)(mag*(3/4));
        this.Intensity = mag-Radius;
    }
}