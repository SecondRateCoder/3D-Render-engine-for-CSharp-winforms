using System.Collections;

class TextureDatabase : IEnumerable{
    List<(Point point, Color color)> td;
    public int Count{get{
        //If there's been a change the re-assign _c , otherwise move on. 
        // then return it.
        if(Unsignedchange){
            _c = td.Count;
        }
        return _c;
    }}
    int _c;
    bool Unsignedchange;
    public (Point p, Color c) this[int index]{
        get{return td[index];}
        set{td[index] = value;}
    }
    public TextureDatabase(List<(Point p, Color c)> data){
        this.td = data;
    }
    public TextureDatabase(){this.td = [];}
    public void Append((Point, Color) data){this.Unsignedchange = true; td.Add(data);}
    public void Append(List<(Point p, Color c)> data){this.Unsignedchange = true; foreach((Point p, Color c) item in data){this.Append(item);}}
    public static implicit operator List<(Point point, Color color)>(TextureDatabase tD){return tD.td;}
    public static explicit operator TextureDatabase(List<(Point point, Color color)> data){return new TextureDatabase(data);}

    public IEnumerator GetEnumerator() => td.GetEnumerator();
}
class CollisionDatabase : IEnumerable{
	List<(gameObj gameObject, int Mass, Vector3 velocity)>[] collidingObjects;
	public int Length{get{return collidingObjects.Length;}}
	public (gameObj gameObject, int Mass, Vector3 velocity) this[int Row, int Column]{
		get{
			return this.collidingObjects[Row][Column];
		}
		set{
			if(Row >= collidingObjects.Length | Column >= collidingObjects[Row].Count){throw new ArgumentOutOfRangeException("");}
			this.collidingObjects[Row][Column] = value;
		}
	}
    public List<(gameObj gameObject, int Mass, Vector3 velocity)> this[int Row]{
        get{return this.collidingObjects[Row];}
        set{if(Row > collidingObjects.Length){collidingObjects.Append(value);}
        else{collidingObjects[Row] = value;}}
    }
	public void SetOrAppend(Point p, (gameObj gameObject, int Mass, Vector3 velocity) data){
		if(p.X >= collidingObjects.Length){collidingObjects.Append([data]);}else
		if(p.Y >= collidingObjects[p.X].Count){collidingObjects[p.X].Append(data);}else
		{collidingObjects[p.X][p.Y] = data;}
	}
    public void Append(IEnumerable<(gameObj gameObject, int Mass, Vector3 velocity)> data){this.collidingObjects.Append(data);}
	public void Append(int position, (gameObj gameObject, int Mass, Vector3 velocity) data){this.collidingObjects[position].Append(data);}
	public void AddRange(int position, IEnumerable<(gameObj gameObject, int Mass, Vector3 velocity)> data){
        if(position > this.Length){return;}else{
            this.collidingObjects[position].AddRange(data);
        }
    }
	public IEnumerator GetEnumerator() => collidingObjects.GetEnumerator();
}