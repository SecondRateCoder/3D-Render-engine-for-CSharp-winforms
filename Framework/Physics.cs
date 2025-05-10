using System.Timers;
using NUnit.Framework;

/// <summary>
/// A Collider
/// </summary>
class Collider{
    /// <summary>Get the TrueCollider cast of this instance.</summary>
    /// <remarks>If it's null, then this collider is custom.</remarks> 
    public TrueCollider? Type{get{if(TrueCollider.IsCollider(this._type)){return (TrueCollider)this._type;}else{return null;}}}
    //The underlying store for this class' TrueCollider convert.
    int _type;
    /// <summary>Get the Mesh that describes the bounds of this Collider.</summary>
    /// <remarks>Will always return a Mesh.</remarks>
    public Mesh Shape{
        get{
            if(this.Buffer != null){
                return Buffer;
            }
            if (TrueCollider.IsCollider(_type)) {
                return (Mesh)TrueCollider.GetMesh((TrueCollider)_type);
            }
            throw new InvalidOperationException("Invalid Collider state.");
        }
    }
    //The underlying Mesh that stores a custom Mesh.
    Mesh? Buffer;
    //Is this Collider custom?
    bool isCustom;
    public Collider(TrueCollider? tC = null){
        if(tC == null){
            this.isCustom = true;
            this.Buffer = (Mesh)Polygon.Mesh();
        }else{
            this._type = tC.type;
            this.isCustom = false;
        }
    }
    public void Initialise(){
        if (Buffer != null) {
            Buffer.Dispose(true); // Dispose the existing buffer to avoid memory leaks
        }
        Buffer = (Mesh)TrueCollider.GetMesh(Type);
    }
    public void Dispose(bool disposing = true){
        if(disposing && this.isCustom && (this.Buffer != null)){
            Buffer.Dispose(true, true);
            Buffer = null;
        }
    }
    public static explicit operator Collider(int tyPe){return new Collider((TrueCollider)tyPe);}
    public static explicit operator Collider(TrueCollider tC){return new Collider(tC);}
    public static readonly Collider Cube = (Collider)TrueCollider.Cube;
    public static readonly Collider Capsule = (Collider)TrueCollider.Capsule;

}

/// <summary>
/// The static class that controls how gameObjs interact.
/// </summary>
static class CollisionManager{
    ///<summary>A property with a nested iterative function.</summary>
    ///<returns>A CollisionDatabase describing the collisions occuring in the program at the current frame.</returns>
    ///<remarks>This property runs an internal function when getting is attempted, assigning the return to a CollisionDatabase variable is best practice.</remarks>
    public static unsafe CollisionDatabase LooseCollisions{
        get{
            CollisionDatabase list = new CollisionDatabase();
            for(int cc = 1;cc < World.worldData.Count;cc++){
                gameObj scope = World.worldData[cc];
                if(!scope.HasComponent<RigidBdy>()){continue;}
                (gameObj gO, int Mass, Vector3 Velocity)[] collisions = SubSearch(scope);
                if(collisions.Length > 0){
                    list.Append(collisions);
                }
            }
            return list;
    }}
    ///<summary>Searches in the World.worldData list for any objects colliding with gameObj</summary>
    static (gameObj gO, int Mass, Vector3 Velocity)[] SubSearch(gameObj scope){
        List<(gameObj gO, int Mass, Vector3 Velocity)> result = [];
        foreach(gameObj gO in World.worldData){
            if(Vector3.GetDistance(scope.Position, gO.Position) < gO.CollisionRange+scope.CollisionRange){
                if(!gO.HasComponent<RigidBdy>()){continue;}
                RigidBdy? rG = gO.GetComponent<RigidBdy>();
                if(rG == null){continue;}else{
                    result.Append((gO, rG.Mass, rG.velocity));
                }
            }
        }
        return result.ToArray();
    }

    public static CollisionDatabase FineCollisions{
        get{
            return new CollisionDatabase();
        }
    }
    public static void Collider(object? sender, ElapsedEventArgs e){
        CollisionDatabase cD = LooseCollisions;
        //For now just apply the Normal between the colliding polygons as a force.
        for(int y =0; y < cD.Length;y++){
            for(int x =1; x < cD[y].Count;x++){
                cD[y][0].gameObject.GetComponent<RigidBdy>().velocity = 
                    Vector3.CProduct(cD[y][0].gameObject.Position, 
                        cD[y][x].gameObject.Position * cD[y][x].gameObject.GetComponent<RigidBdy>().TrueVelocity)/10;
            }
        }
    }
}