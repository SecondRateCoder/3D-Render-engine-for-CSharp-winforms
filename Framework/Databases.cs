using System.Collections;

class TextureDatabase : IEnumerable{
    public delegate (Point point, Color color) ForEachDelegate((Point point, Color color) item);
    public delegate bool ForEachDelegateConditional((Point point, Color color) item);
    List<(Point point, Color color)> td;
    public bool isSorted{get; private set;}
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
    public TextureDatabase(){this.td = [];}
    public TextureDatabase(int count = 0){this.td = new List<(Point point, Color color)>();}
    public TextureDatabase(List<(Point p, Color c)> data){this.td = data; this.isSorted = false;}
    public TextureDatabase(IEnumerable<Point> points){
        this.td = [];
        foreach(Point p in points){td.Add((p, Color.White));}
    }
    public int AllThat(ForEachDelegateConditional fDC){
        int inc = 0;
        foreach((Point point, Color color) item in this.td){
            inc = fDC(item)? inc+1: inc;
        }
        return inc;
    }
    public void Append((Point p, Color c) item){
        this.Unsignedchange = true;
        td.Add(item);
        this.isSorted = this[Count-1].p.X<item.p.X && this[Count-1].p.Y<item.p.Y?true:false;
    }
    public void Foreach(ForEachDelegate fE){
        for(int cc = 0;cc < this.Count;cc++){
            this[cc] = fE(this[cc]);
        }
    }
    public void Append(List<(Point p, Color c)> data){this.Unsignedchange = true; foreach((Point p, Color c) item in data){this.Append(item);}}
    public static implicit operator List<(Point point, Color color)>(TextureDatabase tD){return tD.td;}
    public static explicit operator TextureDatabase(List<(Point point, Color color)> data){return new TextureDatabase(data);}
    ///<summary>Sort this List.</summary>
    public async void Sort(){
        bool swapped = false;
        await Task.Run(() => {
            do{
                //Hold the index at list.
                for(int cc =0; cc < this.Count;cc++){
                    swapped = false;
                    //Swap Ys first.
                    foreach(int _ in new int[0, 1, 2, 3]){
                        if(this[cc].p.Y > this[cc+1].p.Y){
                            swapped = true;
                            int cc_ = cc+1;
                            do{
                                cc_++;
                                if(this[cc].p.Y <= this[cc_].p.Y){
                                    (Point p, Color c) buffBuff = this[cc_];
                                    this[cc_] = this[cc];
                                    this[cc] = buffBuff;
                                    break;
                                }
                            }while((this[cc_].p.Y > this[cc_+1].p.Y) | cc_< this.Count);
                        }
                    }
                    //Swap Xs next
                    foreach(int _ in new int[0, 1, 2, 3]){
                        if((this[cc].p.X > this[cc+1].p.X) && (this[cc].p.Y == this[cc+1].p.Y)){
                            swapped = true;
                            (Point p, Color c) buffBuff = this[cc];
                            this[cc] = this[cc+1];
                            this[cc+1] = buffBuff;
                        }
                    }
                }
            }while(swapped);
        });
    }


    public IEnumerator GetEnumerator() => td.GetEnumerator();
}


class CollisionDatabase : IEnumerable{
    public delegate List<(gameObj gameObject, int Mass, Vector3 velocity)> ForEachDelegateExpandable (List<(gameObj gameObject, int Mass, Vector3 velocity)> item);
	List<List<(gameObj gameObject, int Mass, Vector3 velocity)>> collidingObjects = [];
	public int Length{get{return collidingObjects.Count;}}
    public void Foreach(ForEachDelegateExpandable fE){
        for(int cc = 0;cc < this.Length;cc++){
            this[cc] = fE(this[cc]);
        }
    }
	public (gameObj gameObject, int Mass, Vector3 velocity) this[int Row, int Column]{
		get{
			return this.collidingObjects[Row][Column];
		}
		set{
			if(Row >= collidingObjects.Count | Column >= collidingObjects[Row].Count){throw new ArgumentOutOfRangeException("");}
			this.collidingObjects[Row][Column] = value;
		}
	}
    public List<(gameObj gameObject, int Mass, Vector3 velocity)> this[int Row]{
        get{return this.collidingObjects[Row];}
        set{if(Row > collidingObjects.Count){collidingObjects.Append(value);}
        else{collidingObjects[Row] = value;}}
    }
    public CollisionDatabase(){this.collidingObjects = [];}
	public void SetOrAppend(Point p, (gameObj gameObject, int Mass, Vector3 velocity) data){
		if(p.X >= collidingObjects.Count){collidingObjects.Append([data]);}else
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