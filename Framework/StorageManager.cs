using System.ComponentModel.DataAnnotations;
using System.IO;
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
				World[cc] = new gameObj(Pos, Rot, polys);
				string typeName = "";
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
}
class Path{
	public string this[string newPath]{
		get{return this.filePath;}
		set{this.Update(value);}
	}
	string filePath;
	string buffer;
	string? fileExtension;
	bool Directory;
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
			this.filePath = File.Exists(filePath)? filePath: throw new TypeInitializationException("Path", new ArgumentException());
			this.fileExtension = new FileInfo(filePath).Extension != fileExtension || string.IsNullOrEmpty(fileExtension)? new FileInfo(filePath).Extension: fileExtension;
		}
	}
	public bool Update(string newPath){
		if(System.IO.Directory.Exists(newPath) && Directory){this.filePath = newPath;	return true;}else 
		if(File.Exists(newPath) && new FileInfo(newPath).Extension == this.fileExtension){this.filePath = newPath;	return true;}else{return false;}
	}
	bool Exists(string path){
		if(File.Exists(path)){return true;}else 
		if(System.IO.Directory.Exists(path)){return true;}else{return false;}
	}
}