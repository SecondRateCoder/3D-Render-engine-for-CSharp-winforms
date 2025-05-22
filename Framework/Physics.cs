using System.Timers;
using NUnit.Framework;

/// <summary>
/// A Collider
/// </summary>
class Collider{
    /// <summary>Get the TrueCollider cast of this instance.</summary>
    /// <remarks>If it's null, then this collider is custom.</remarks> 
    public TrueCollider.Colliders? Type{get{if(this.isCustom){return null;}else{return (TrueCollider.Colliders)this._type;}}}
    //The underlying store for this class' TrueCollider convert.
    int _type;
    /// <summary>Get the Mesh that describes the bounds of this Collider.</summary>
    /// <remarks>Will always return a Mesh.</remarks>
    public Mesh Shape{
        get{
            if(this.Buffer != null){
                return Buffer;
            }
            if(this.isCustom){
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
        return [.. result];
    }

    public static CollisionDatabase FineCollisions{
        get{
            return new CollisionDatabase();
        }
    }
    /// <summary>How long should <see cref="HandleCollisions"/> be allowed to run for before being forcibly ended.</summary>
    public static int ColliderCheckTime;
    public static void HandleColliderCheckTime(){ColliderCheckTime = 1 / Entry.selfDelay;}
    public static void HandleCollisions(object? sender, ElapsedEventArgs e) {
        CollisionDatabase cD = LooseCollisions;
        CancellationTokenSource cts = new();
        //For now just apply the Normal between the colliding polygons as a force.
        cts.CancelAfter(ColliderCheckTime);
        int x = 0;
        int y = 0;
        for (y = 0; y < cD.Length | !cts.IsCancellationRequested; y++) {
            for (x = 1; x < cD[y].Count | !cts.IsCancellationRequested; x++) {
                cD[y][0].gameObject.GetComponent<RigidBdy>().velocity =
                    Vector3.CProduct(cD[y][0].gameObject.Position,
                        cD[y][x].gameObject.Position * cD[y][x].gameObject.GetComponent<RigidBdy>().TrueVelocity) / 10;
            }
        }
    }
}