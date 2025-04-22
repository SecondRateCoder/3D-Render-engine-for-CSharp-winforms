using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
static class StorageManager{
    static string PathProp = "";
    public static string filePath{set{
        PathProp = value + @"";
    } get{return PathProp;}}
    static string fileSpecific = @"\Cache\Saves\1.bin";
	/// <summary>
	///  This saves the World.worldData to the Binary file in StorageAttempt<seealso cref="fileSpecific"/>
	/// </summary>
	/// <remarks>This algorithm saves date in the pattern:	
	/// each gameObj's Position then their Rotation, then an number detailing the number of it's children,
	/// after which each child's A, B and C position is then stored.
	/// </remarks>
    public static void Save(string Method = "Save/1.bin"){
		fileSpecific = Method;
		List<byte> fileContent = [.. BitConverter.GetBytes(World.worldData.Count)];
		foreach(gameObj gO in World.worldData){
			fileContent.AddRange(BitConverter.GetBytes(gO.Size));
			fileContent.AddRange((byte[])gO.Position);
			fileContent.AddRange((byte[])gO.Rotation);
			fileContent.AddRange(BitConverter.GetBytes(gO.Children.Count));
			int cc = 0;
			//The children are encoded.
			foreach(Polygon pO in gO.Children){
				fileContent.AddRange((byte[])gO.Children[cc].A);
				fileContent.AddRange((byte[])gO.Children[cc].B);
				fileContent.AddRange((byte[])gO.Children[cc].C);
				cc++;
			}
			/*
			for(int cc =0;cc < gO.compLength;cc++){
				fileContent.AddRange(BitConverter.GetBytes(sizeof(char)*gO.GetComponentType(cc).Name.Count));
				foreach(char c in gO.GetComponentType(cc).Name){
					fileContent.AddRange(BitConverter.GetByte(c));
				}
			fileContent.AddRange(BitConverter.GetBytes(gO.GetComponentType(cc).Name));
			fileContent.AddRange(gO.GetComponent(cc).ToBytes());
			*/
			}
		}
	static gameObj[] Load(){
		gameObj[]? World = null;
		using(BinaryReader sR = new BinaryReader(File.Open(PathProp+fileSpecific, FileMode.Open))){
			int worldSize = GetFloat(sR);
			World = new gameObj[worldSize];
			for(int cc=0;cc<worldSize;cc++){
				int childSize = GetFloat(sR)-(sizeof(float)*6);
				Vector3 Pos = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
				Vector3 Rot = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
				List<Polygon> polys = new List<Polygon>();
				for(int i = 0;i < childSize;i++){
					polys.Append(new Polygon(new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
					new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
					new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR))));
				}
				World[cc] = new gameObj(Pos, Rot, true, polys);
				int siZe = GetFloat(sR);
				List<byte> content = new List<byte>();
				for(int i =0;i < siZe;i++){
					content.Append(sR.ReadByte());
				}
			}
		}
		return World;
	}
	static int GetFloat(BinaryReader sR){
		byte[] buffer = new byte[sizeof(float)];
		for(int cc=0;cc < sizeof(float);cc++){
			buffer[cc] = sR.ReadByte();
		}
		return BitConverter.ToInt32(buffer);
	}
	static List<string> typeNames = ["Rndrcomponent", "RigidBdy", "Vector", "Polygon", "gameObj", "Camera"];
	static string GetTypeName(byte[] content, ref int Mem){
		string result = "";
		for(int cc = Mem; cc < content.Length;cc++){
			byte[] buffer = new byte[sizeof(char)];
			for(int i = 0; i < buffer.Length;i++){
				buffer[i] = content[i+(cc*sizeof(char))];
			}
			result += BitConverter.ToChar(buffer);
			if(typeNames.Contains(result)){
				return result;
			}
			Mem = cc;
		}
		throw new Exception("CorruptedSaveFile", new FileFormatException("The file was been saved in a specific format, if this pattern has been broken then the "));
	}



	/// <summary>Read a byte array from <see cref="startFrom"/> argument up until <see cref="startFrom + sizeof(int)"/>.</summary>
	/// <param name="bytes">The array containing the int32.</param>
	/// <param name="startFrom">The integer that the decoder should start from in the byte array.</param>
	/// <returns>An Int32.</returns>
	public static int ReadInt32(byte[] bytes, int startFrom =0){
		byte[] integer = new byte[sizeof(int)];
		for(int cc =0;cc < sizeof(int);cc++){
			integer[cc] = bytes[cc +startFrom];
		}
		return BitConverter.ToInt32(integer);
	}
	/// <summary>Read a byte array from <see cref="startFrom"/> argument up until <see cref="startFrom + sizeof(float)"/>.</summary>
	/// <param name="bytes">The array containing the float.</param>
	/// <param name="startFrom">The integer that the decoder should start from in the byte array.</param>
	/// <returns>A float.</returns>
	public static float ReadFloat(byte[] bytes, int startFrom =0){
		byte[] float_ = new byte[sizeof(float)];
		for(int cc =0;cc < sizeof(float);cc++){
			float_[cc] = bytes[cc +startFrom];
		}
		return BitConverter.ToSingle(float_);
	}
	/// <summary>
	/// Convert the byte array to a string.
	/// </summary>
	/// <param name="bytes">The array to be decoded.</param>
	/// <param name="encoding">The encoding the result string will be formated to.</param>
	/// <returns></returns>
	public static string ReadString(byte[] bytes, Encoding encoding, int endAt, int startFrom = 0){
		byte[] buffer = new byte[endAt - startFrom];
		for(int cc = 0;cc < endAt - startFrom;cc++){
			buffer[cc] = bytes[cc+startFrom];
		}
		return BitConverter.ToString(Encoding.Convert(Encoding.Default, encoding, buffer));
	}
	public static Point ReadPoint(byte[] bytes, int startFrom =0){
		return new Point(ReadInt32(bytes), ReadInt32(bytes, sizeof(int)));
	}
}
class Path{
	string filePath;
	string? fileExtension;
	string[] fileExtensions;
	bool Directory;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Path(string filePath, string? fileExtension, bool Directory){
		//Check if there is something at the specified path.
		if(!Exists(filePath)){throw new TypeInitializationException("Path", new ArgumentException());}
		//Check if the item there is a directory.
		if(Directory && (string.IsNullOrEmpty(fileExtension) | string.IsNullOrWhiteSpace(fileExtension))){
			this.Directory = Exists(filePath) | System.IO.Directory.Exists(filePath);		
			this.filePath = Directory? filePath: throw new TypeInitializationException("Path", new ArgumentException());
			this.fileExtension = null;}else
		if(!Directory && !(string.IsNullOrEmpty(fileExtension) | string.IsNullOrWhiteSpace(fileExtension))){
			this.Directory = false;
			this.fileExtension = new FileInfo(filePath).Extension != fileExtension || string.IsNullOrEmpty(fileExtension)? new FileInfo(filePath).Extension: fileExtension;
			this.filePath = File.Exists(filePath)? filePath: throw new TypeInitializationException("Path", new ArgumentException());
		}else{
			throw new TypeInitializationException("Path", new FileNotFoundException());
		}
	}
    public Path(string filePath, string[]? fileExtensions, bool Directory){
		//Check if there is something at the specified path.
		if(!Exists(filePath)){throw new TypeInitializationException("Path", new ArgumentException());}
		//Check if the item there is a directory.
		if(Directory && fileExtensions == null){
			this.Directory = Exists(filePath) | System.IO.Directory.Exists(filePath);		
			this.filePath = Directory? filePath: throw new TypeInitializationException("Path", new ArgumentException());
			this.fileExtension = null;}else
		if(fileExtensions != null && !Directory && fileExtensions.Count() > 0){
			this.Directory = false;
			this.filePath = File.Exists(filePath)? filePath: throw new TypeInitializationException("Path", new ArgumentException());
			this.fileExtensions = [];
			foreach(string s in fileExtensions){
				this.fileExtensions.Append(new FileInfo(filePath).Extension != s || string.IsNullOrEmpty(fileExtension)? new FileInfo(filePath).Extension: s);
			}
		}
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public bool Update(string newPath){
		if(System.IO.Directory.Exists(newPath) && Directory){this.filePath = newPath;	return true;}else 
		if(File.Exists(newPath) &&( new FileInfo(newPath).Extension == this.fileExtension | this.fileExtensions.Contains(new FileInfo(newPath).Extension))){this.filePath = newPath;	return true;}else{return false;}
	}
	public string Get(){return this.filePath;}
	bool Exists(string path){
		if(File.Exists(path)){return true;}else 
		if(System.IO.Directory.Exists(path)){return true;}else{return false;}
	}
	public static Path operator +(Path p, string s){
		if(File.Exists(p+s) && (new FileInfo(p+s).Extension == p.fileExtension | p.fileExtensions.Contains(new FileInfo(p+s).Extension)) | System.IO.Directory.Exists(p+s) && p.Directory == true){
			p.filePath += s;}return p;}
	public static implicit operator string(Path p){return p.Get();}
	public static explicit operator Path(string s){return new Path(s, File.Exists(s)? new FileInfo(s).Extension: throw new FileNotFoundException($"The file at: {s} was not found"), false);}
}