using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Gtk;
static class StorageManager{
	public static string ApplicationPath{
		get{
			if(aP_ == null){
				Span<char> span = new Span<char>(Directory.GetCurrentDirectory().ToCharArray());
				string s = "";
				for(int cc =span.Length-1; cc >= -1;cc--){
					if(cc < 0){throw new FileNotFoundException($"The working Directory of this Application was not found.");}
					if((span[cc-4] == 'f') && (span[cc-3] == 'o') && (span[cc-2] == 'r') && (span[cc-1] == 'm') && (span[cc] == 's')){
						s = span.Slice(0, cc+2).ToString();
						break;
					}
				}
				aP_ = s;
			}
			return aP_;
		}
	}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static string aP_;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    ///  This saves the World.worldData to the Binary file in StorageAttempt<seealso cref="fileSpecific"/>
    /// </summary>
    /// <remarks>This algorithm saves date in the pattern:	
    /// each gameObj's Position then their Rotation, then an number detailing the number of it's children,
    /// after which each child's A, B and C position is then stored.
    /// </remarks>
    // public static void Save(string Method = "Save/1.bin"){
	// 	fileSpecific = Method;
	// 	List<byte> fileContent = [.. BitConverter.GetBytes(World.worldData.Count)];
	// 	foreach(gameObj gO in World.worldData){
	// 		fileContent.AddRange(BitConverter.GetBytes(gO.Size));
	// 		fileContent.AddRange((byte[])gO.Position);
	// 		fileContent.AddRange((byte[])gO.Rotation);
	// 		fileContent.AddRange(BitConverter.GetBytes(gO.Children.Count));
	// 		int cc = 0;
	// 		//The children are encoded.
	// 		foreach(Polygon pO in gO.Children){
	// 			fileContent.AddRange((byte[])gO.Children[cc].A);
	// 			fileContent.AddRange((byte[])gO.Children[cc].B);
	// 			fileContent.AddRange((byte[])gO.Children[cc].C);
	// 			cc++;
	// 		}
	// 		/*
	// 		for(int cc =0;cc < gO.compLength;cc++){
	// 			fileContent.AddRange(BitConverter.GetBytes(sizeof(char)*gO.GetComponentType(cc).Name.Count));
	// 			foreach(char c in gO.GetComponentType(cc).Name){
	// 				fileContent.AddRange(BitConverter.GetByte(c));
	// 			}
	// 		fileContent.AddRange(BitConverter.GetBytes(gO.GetComponentType(cc).Name));
	// 		fileContent.AddRange(gO.GetComponent(cc).ToBytes());
	// 		*/
	// 		}
	// 	}
	// static gameObj[] Load(){
	// 	gameObj[]? World = null;
	// 	using(BinaryReader sR = new BinaryReader(File.Open(PathProp+fileSpecific, FileMode.Open))){
	// 		int worldSize = GetFloat(sR);
	// 		World = new gameObj[worldSize];
	// 		for(int cc=0;cc<worldSize;cc++){
	// 			int childSize = GetFloat(sR)-(sizeof(float)*6);
	// 			Vector3 Pos = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
	// 			Vector3 Rot = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
	// 			List<Polygon> polys = new List<Polygon>();
	// 			for(int i = 0;i < childSize;i++){
	// 				polys.Append(new Polygon(new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
	// 				new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
	// 				new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR))));
	// 			}
	// 			World[cc] = new gameObj(Pos, Rot, true, polys);
	// 			int siZe = GetFloat(sR);
	// 			List<byte> content = new List<byte>();
	// 			for(int i =0;i < siZe;i++){
	// 				content.Append(sR.ReadByte());
	// 			}
	// 		}
	// 	}
	// 	return World;
	// }
	static int GetFloat(BinaryReader sR){
		byte[] buffer = new byte[sizeof(float)];
		for(int cc=0;cc < sizeof(float);cc++){
			buffer[cc] = sR.ReadByte();
		}
		return BitConverter.ToInt32(buffer);
	}
	// static List<string> typeNames = ["Rndrcomponent", "RigidBdy", "Vector", "Polygon", "gameObj", "Camera"];
	// static string GetTypeName(byte[] content, ref int Mem){
	// 	string result = "";
	// 	for(int cc = Mem; cc < content.Length;cc++){
	// 		byte[] buffer = new byte[sizeof(char)];
	// 		for(int i = 0; i < buffer.Length;i++){
	// 			buffer[i] = content[i+(cc*sizeof(char))];
	// 		}
	// 		result += BitConverter.ToChar(buffer);
	// 		if(typeNames.Contains(result)){
	// 			return result;
	// 		}
	// 		Mem = cc;
	// 	}
	// 	throw new Exception("CorruptedSaveFile", new FileFormatException("The file was been saved in a specific format, if this pattern has been broken then the "));
	// }



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
	public static PointF ReadPointF(byte[] bytes, int startFrom =0){
		return new PointF(ReadInt32(bytes), ReadInt32(bytes, sizeof(int)));
	}
	public static string FindFileFolder(string FileFolderName, bool Directory, string BeginFrom = ""){
		BeginFrom = string.IsNullOrEmpty(BeginFrom) ? ApplicationPath : BeginFrom;
		if(!File.Exists(BeginFrom)){return "";}
		if (Directory){
            // Search for directories
            foreach (string dir in System.IO.Directory.EnumerateDirectories(BeginFrom, FileFolderName, SearchOption.AllDirectories)){if((new FileInfo(dir).Directory ?? new DirectoryInfo(GetParentDirectory(dir))).Name == FileFolderName){return dir;}}
        }else{
            // Search for files
            foreach (string file in System.IO.Directory.EnumerateFiles(BeginFrom, FileFolderName, SearchOption.AllDirectories)){if(new FileInfo(file).Name == FileFolderName){return file;}}
        }
		return "";
	}
	public static string GetParentDirectory(string fullPath){
		char seperator = System.IO.Path.DirectorySeparatorChar;
		StringBuilder sB = new();
		char c = 'c';
		for(int cc =0; cc < fullPath.Length; cc++, c = fullPath[cc]){
			if(c == seperator){return sB.ToString();}
			sB.Append(c);
		}
		return sB.ToString();
	}
}
class Path{
	static Path(){ Empty = new("", [], false); }
	public static Path Empty;
	string filePath;
	string[] fileExtensions;
	bool Directory;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Path(string filePath, string? fileExtension, bool Directory){
		//Check if there is something at the specified path.
		if(!Exists(filePath)){throw new TypeInitializationException($"Path: {filePath}", new ArgumentException());}
		//Check if the item there is a directory.
		if(Directory && (string.IsNullOrEmpty(fileExtension) | string.IsNullOrWhiteSpace(fileExtension))){
			this.Directory = Exists(filePath) | System.IO.Directory.Exists(filePath);		
			this.filePath = Directory? filePath: throw new TypeInitializationException($"Path: {filePath}", new ArgumentException());}else
		if(!Directory && !(string.IsNullOrEmpty(fileExtension) | string.IsNullOrWhiteSpace(fileExtension))){
			this.Directory = false;
			this.filePath = File.Exists(filePath)? filePath: throw new TypeInitializationException($"Path: {filePath}", new ArgumentException());
		}else{
			throw new TypeInitializationException($"Path: {filePath}", new FileNotFoundException());
		}
	}
    public Path(string filePath, IEnumerable<string>? fileExtensions, bool Directory){
		if(string.IsNullOrWhiteSpace(filePath) & ((fileExtensions != null && !fileExtensions.Any()) | fileExtensions == null) & !Directory){
			this.filePath = filePath;
			this.fileExtensions = [.. (fileExtensions ?? [])];
			this.Directory = false;
		}
		//Check if the item there is a directory.
		if(Directory && fileExtensions == null){
			this.Directory = Exists(filePath) | System.IO.Directory.Exists(filePath);		
			this.filePath = Directory? filePath: throw new TypeInitializationException($"Path: {filePath}", new ArgumentException());}else
		if(fileExtensions != null && !Directory && fileExtensions.Any()){
			this.Directory = false;
			this.filePath = File.Exists(filePath)? filePath: throw new TypeInitializationException($"Path", new ArgumentException());
			this.fileExtensions = [];
			foreach(string s in fileExtensions){
				this.fileExtensions.Append(new FileInfo(filePath).Extension != s? new FileInfo(filePath).Extension: s);
			}
		}else{throw new TypeInitializationException($"Conflicting parameters caused this .ctor to throw an error.", new ArgumentException());}
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public bool Update(string newPath){
		if(System.IO.Directory.Exists(newPath) && Directory){this.filePath = newPath;	return true;}else 
		if(File.Exists(newPath) &&(this.fileExtensions.Contains(new FileInfo(newPath).Extension) | this.fileExtensions.Contains(new FileInfo(newPath).Extension))){this.filePath = newPath;	return true;}else{return false;}
	}
	public string Get(){return this.filePath;}
	bool Exists(string path){
		if(File.Exists(path)){return true;}else 
		if(System.IO.Directory.Exists(path)){return true;}else{return false;}
	}
	public static Path operator +(Path p, string s){
		if(p != null){
			if((File.Exists(p.filePath+s) && p.fileExtensions.Contains(new FileInfo(p.filePath + s).Extension))| (System.IO.Directory.Exists(p.filePath+s) && p.Directory == true)){
				p.filePath += s;
			}
		}
		return p ?? new Path("", [], false);
	}
	public static implicit operator string(Path p){return p.Get();}
	public static explicit operator Path(string s){return new Path(
		s, 
		File.Exists(s) | System.IO.Directory.Exists(s)? new FileInfo(s).Extension: throw new FileNotFoundException($"The file at: {s} was not found"), 
		System.IO.Directory.Exists(s));}
}
static class ExtensionHandler{
	static readonly string Start = "SetUp";
	static readonly string TUpdate = "TimedUpdate";
	static readonly string Update = "Update";
	static readonly string Closing = "Closing";
	const int MaxTries = 20;
	/// <summary>A enum to differentiate beteen the various Method extension types.</summary>
	public enum MethodType{
		/// <summary>Runs with <see cref="Entry.Start"/>.</summary>
		Start = 0,
		/// <summary>Runs with <see cref="Entry.Update"/>.</summary>
		Update = 1,
		/// <summary>Runs with <see cref="Entry.TUpdate"/>.</summary>
		TimedUpdate = 2,
		/// <summary>Runs when the application ends.</summary>
		Closing = 3,
		/// <summary>This means the method runs outside the rendering loop, the method types are not protected against and can damage the application.</summary>
		/// <remarks>UNIMPLEMENTED, DO NOT USE.</remarks>
		Workspace = 4
	}
	static ExtensionHandler() { extensions = []; IndexToName = []; }
	/// <summary>Convert from a Priority key to an Extension name, mainly for Exception logging.</summary>
	static Dictionary<int, string> IndexToName;
	/// <summary>Stores all the data needed for the system to interface with the method and understand it for what it needs to do.</summary>
	static Dictionary<int, (int ErrorCalls, MethodType type, MethodInfo method)> extensions;
	/// <summary>Loads extensions from the Cache\Saves\Extensions.txt cache, allowing extensions to be automatically loaded from start-up.</summary>
	public static bool[] PreLoadExtensions() {
		List<(string Path, string Message)> ErrorReturns = [];
		bool[] result = Task.Run(() => {
			bool[] result = [];
			try{
				string[] ExtensionFilePaths = File.ReadAllLines(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Saves\Extensions.txt")).ToList();
				result = new bool[ExtensionFilePaths.Length];
				List<string> validPaths = [];
				int cc =0;
				foreach(string s in ExtensionFilePaths){
					if (BuildJsonObject(s).Result){
						validPaths.Add(s);
						result[cc] = true;
					}else{
						result[cc] = false;
						ErrorReturns.Add((s, "File was not in the same Format as expected of a Json extension"));
					}
					cc++;
				}
				File.WriteAllLines(StorageManager.FindFileFolder("Extensions.txt", false, StorageManager.ApplicationPath), validPaths);
			}catch(FileNotFoundException ex){
				List<string> ExtensionFilePaths = File.ReadAllLines(StorageManager.FindFileFolder("Extensions.txt", false, StorageManager.ApplicationPath)).ToList();
				result = new bool[ExtensionFilePaths.Length];
				List<string> validPaths = [];
				int cc =0;
				ErrorReturns.Add(("...", ex.Message));
				foreach(string s in ExtensionFilePaths){
					if(BuildJsonObject(s).Result){
						validPaths.Add(s);
						result[cc] = true;
					}else{
						result[cc] = false;
						ErrorReturns.Add((s, "File was not in the same Format as expected of a Json extension"));
					}
					cc++;
				}
				File.WriteAllLines(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Saves\Extensions.txt"), validPaths);
			}
			MessageBox.Show("The path and the Message line-up, where there is a \"...\" Message that means that a more impactful error was thrown\n\n" + 				CustomFunctions.ToString(CustomFunctions.GetTypeParamT(ErrorMessage)) + "\n" + CustomFunctions.ToString(CustomFunctions.GetTypeParamR(ErrorMessage), "Error PreLoading error: Debugged info", MessageBoxButtons.Ok, MessageBoxIcons.Warning);
			return result;
		}).Result;
		return result;
	}
	//!This is the only point where the Extension loading process it allowed to be started (aside from PreloadExtensions), it retrieves all the data it needs locally so it doesnt need any parameters.
	/// <summary>Begins the process of attaching an extension, doing it's logic checks and validation locally.</summary>
	/// <remarks>Open a <see cref="Gtk.FileChooserDialog"/> and use it to select a folder containing a Metadata.json file.</remarks>
	/// <returns>Was the folder containing viable data for an extension attaching.</returns>
	public static async Task<bool> AttachExtension(){
		//!Use OpenFileDialog!!!
		string folderPath = "";
		bool Suceeded = true;
		Gtk.Application.Init();
		await Task.Run(() => {
			using(OpenFDialog fD = new()){
				    openFileDialog.Title = "Open Metadata.json";
				    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
				    openFileDialog.FileName = "Metadata.json";
				    openFileDialog.CheckFileExists = true;
				    openFileDialog.CheckPathExists = true;
				if(fD.ShowDialog() == DialogResult.OK){
					Suceeded = BuildJsonObject(fD.Filename).Result;
				}else{ Suceeded = false; }
			}
		});
		if (MessageBox.Show("Should the extension be Pre-Loaded on startup of application?", "Pre-load extensions?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.OK){
			using(FileStream fS = File.OpenWrite(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Saves\Extensions.txt"))){
				byte[] array = Encoding.Default.GetBytes(folderPath);
				bool completed = true;
				int num = 0;
				do{
					try{
						fS.Write(array, 0, array.Length);
						completed = true;
					}catch(Exception ex){
						if(num < MaxTries){
							if (MessageBox.Show($"The extension could not be written to {System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Saves\Extensions.txt")}\nRetry?",
								$"Error Message:\n{ex.Message}",
								MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry) { completed = false; num++; }else{break;}
							await Task.Delay(1000);
						}
					}
				}while(!completed);
				if(completed){_ = MessageBox.Show("Extension loaded successfully.", null, MessageBoxButtons.OK, MessageBoxIcon.Information);}
			}
		}
		return Suceeded;
	}
	/// <summary>Builds and stores the Json file object, checking if the File contains the right properties.</summary>
	/// <param name="filePath">The file path of the Json.</param>
	/// <returns>Was the Json file of the right format?</returns>
	static async Task<bool> BuildJsonObject(string filePath){
		string Jsonbody = "";
		try{
			Jsonbody = filePath.Contains("Metadata.json")? File.ReadAllText(filePath):File.ReadAllText(filePath + "Metadata.json");
		}catch(FileNotFoundException){
			MessageBox.Show($"The file: Metadata.json was not found at: {filePath}", "ExtensionDataNotFoundException handled.", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		//foreach (char c in Jsonbody) { if (!(char.IsLetterOrDigit(c) | char.IsPunctuation(c))) { throw new JsonFormatexception($"The file at: {filePath + "MetaData.json"} had the wrong format and is incompatible"); } }
		Dictionary<string, object> Jsondata = JsonSerializer.Deserialize<Dictionary<string, object>>(Jsonbody, JsonSerializerOptions.Default) ?? throw new ArgumentException();
		//Code file Path
		//Error handling.
		//The ClassName.
		string Name = "";
		if(Jsondata.TryGetValue("ClassName", out object? classname) && classname != null){
			Name = classname is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{extensions.Count}": throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}else if(Jsondata.TryGetValue("Name", out object? name) && name != null){
			Name = name is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{extensions.Count}": throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}else{throw new FileFormatException("File was not in the same Format as expected of a Json extension");}
		//Priority of the extension.
		_ = Jsondata.TryGetValue("Priority", out object? priority) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		_ = priority ?? (MessageBox.Show($"The Json at {filePath} does not have a Priority of expected value, changing value of Priority to: {extensions.Count}", null, MessageBoxButtons.OK, MessageBoxIcon.Warning));
		int Priority = priority is JsonElement je ? Convert.ToInt32(je.ToString()) : throw new Exception("Priority is not a JsonElement");
		bool Start = await LoadToMemory(Priority, Name, Jsondata, MethodType.Start);
		if(!Start){ return false; }
		bool Update = await LoadToMemory(filePath, Priority, Name, Jsondata, MethodType.Update);
		bool TimedUpdate = await LoadToMemory(filePath, Priority, Name, Jsondata, MethodType.TimedUpdate);
		bool OnClosing = await LoadToMemory(filePath, Priority, Name, Jsondata, MethodType.Closing);
		return Start && true | Update | TimedUpdate | OnClosing;
	}
	/// <summary>Invoke all the extension functions of type: <paramref name="type"/>.</summary>
	/// <param name="type">The extension type that is being called.</param>
	/// <param name="parameters">The parameters needed to run the program</param>
	/// <returns>Where methids invoked?</returns>
	/// <remarks>If the type is an action the parameters don't matter as a new parameter list would be passed for invocation anyways.</remarks>
	public static bool InvokeExtensionMethods(MethodType type, params object[] parameters){
		bool Invoked = false;
		int cc = 0;
		foreach(var item in extensions){
			int errorCalls = item.Value.ErrorCalls;
			if(item.Value.type == type){
				try{
					if(item.Value.method.GetParameters().Length > 0){
						item.Value.method.Invoke(null, BindingFlags.Static, null, type == MethodType.Update? []: parameters, CultureInfo.InvariantCulture);
					}else{item.Value.method.Invoke(null, BindingFlags.Static, null, [], CultureInfo.InvariantCulture);}
				}catch(Exception ex){
					if(item.Value.ErrorCalls > (int)(MaxTries/item.Key)){
						MessageBox.Show(ex.Message + $"{IndexToName[item.Key]} has called too many errors({(int)(MaxTries/item.Key)} Exceptions) and has been removed from the Application to maintain performance.", $"Error in Extension: {IndexToName[item.Key]}", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}else{
						DialogResult dR = MessageBox.Show(ex.Message + $"Should this extension be removed? \n(It's possible that it will be removed later if it calls {(int)(MaxTries/item.Key)} Exceptions.)", $"Error in Extension: {IndexToName[item.Key]}", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if(dR == DialogResult.OK){ extensions.Remove(item.Key); continue; }else{ errorCalls++; }
					}
				}
				extensions[item.Key] = (errorCalls, item.Value.type, item.Value.method);
				cc++;
				Invoked = true;
			}
		}
		return Invoked;
		
	}
	/// <summary>This overload of <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/> uses some built-in helper methods to allow for a level of abstraction, it's workflow is similar to <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/>.</summary>
	/// <param name="Priority">The extension's Priority level.</param>
	/// <param name="ClassName">The name of the class that the extension will be compiled to.</param>
	/// <param name="JsonDeserialised">The raw Json that contains the object.</param>
	/// <param name="methodType">The type of the method.</param>
	/// <returns></returns>
	static async Task<bool> LoadToMemory(string PathToExtension, int Priority, string ClassName, Dictionary<string, object> JsonDeserialised, MethodType methodType) {
		return await LoadToMemory(Priority, ClassName, NameFromType(methodType), GetFunctionBody(PathToExtension, NameFromType(methodType), JsonDeserialised), methodType);
	}
	static string NameFromType(MethodType methodType) {
		switch (methodType){
			case MethodType.Start:
				return Start;
			case MethodType.Update:
				return Update;
			case MethodType.TimedUpdate:
				return TUpdate;
			case MethodType.Closing:
				return Closing;
			default:
				throw new ArgumentException();
		}
	}
	/// <summary>Saves a singular extension function to allow it to be runnable.</summary>
	/// <param name="Priority">The extionsion function's property.</param>
	/// <param name="ClassName">The name of the function's parent extension.</param>
	/// <param name="functionName">The name of the function.</param>
	/// <param name="FunctionBody">The actual logic of the function.</param>
	/// <param name="methodType">The type of extension function it is, selected from <see cref="ExtensionHandler.MethodType"/></param>
	/// <returns>Was the function processed successfully.</returns>
	/// <exception cref="Exception">Did the compilation of the function fail? then call the Exception.</exception>
	static async Task<bool> LoadToMemory(int Priority, string ClassName , string functionName, string FunctionBody, MethodType methodType){
		return await Task.Run(() => {
			ScriptOptions scriptOptions = ScriptOptions.Default
				.WithReferences(typeof(object).Assembly)
				.WithImports("System");
			FunctionBody = "class " + ClassName + "{" + FunctionBody + @"}";
			Script<object> script = CSharpScript.Create(FunctionBody, scriptOptions);
			Compilation compilation = script.GetCompilation();
			using(MemoryStream ms = new()){
				if (!compilation.Emit(ms).Success){throw new Exception("Compilation failed.");}
				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = Assembly.Load(ms.ToArray());
				try{
					Type type = assembly.GetType(ClassName) ?? throw new ArgumentNullException("");
					object instance = Activator.CreateInstance(type) ?? throw new ArgumentNullException();
				//There is already an entry with the same Priority, give the function a lower Priority until it can be put into the Dictionary.
				if (extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) value) && (value.type == methodType)){
						bool skip = true;
						if(!extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) _value) && (_value.type == methodType)){skip = false;}else{
							DialogResult dR = MessageBox.Show(
								"Should all other Priorities be shifted upwards to accomodate this extension(CONTINUE)?\nShould this extension be given the next available Priority allotment(TRY)?\n(THIS MAY AFFECT HOW THE EXTENSION INTERACTS WITH THE APPLICATION AND IT MAY CRASH!)\n\nShould the extension not be added(CANCEL)?", $"ExtensionPriorityConflictionException: There are already an extension with Priority {Priority}", 
								MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Warning
							);
							if (dR == DialogResult.Continue){
								// Create a new dictionary with incremented keys
								Dictionary<int, (int ErrorCalls, MethodType type, MethodInfo method)> updatedExtensions = [];
								Dictionary<int, string> updatedIndexToName = [];
								int sameShift = 0;
								foreach (KeyValuePair<int, (int ErrorCalls, MethodType type, MethodInfo method)> item in extensions){
									if(item.Key > Priority){
										updatedExtensions.Add(item.Key + 1, item.Value);
										if (IndexToName.TryGetValue(item.Key, out string? name)){
											updatedIndexToName.Add(item.Key + 1, name);
										}
									}else if(item.Key == Priority){
										sameShift++;
										updatedExtensions.Add(item.Key + sameShift, item.Value);
										if (IndexToName.TryGetValue(item.Key, out string? name)){
											updatedIndexToName.Add(item.Key + sameShift, name);
										}
									}
								}
								extensions = updatedExtensions;
								IndexToName = updatedIndexToName;
							}else if(dR == DialogResult.TryAgain){
								while((extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) value_) && (value_.type == methodType)) | !skip){
									Priority++;
								}
							}else{return false;}
						}
					}
					_ =extensions.TryAdd(Priority,(0, methodType, type.GetMethod(functionName) ?? throw new ArgumentNullException()));
					_ = IndexToName.TryAdd(Priority, ClassName);
					ms.Dispose();
				}catch(ArgumentNullException){
					ms.Dispose();
					return false;
				}
			}
			return true;
		});
	}
	/// <summary>Returns the Method body of a function from a Json.</summary>
	/// <param name="functionName">The name of the function.</param>
	/// <param name="Json">The Dictionary that describes the original json object for te function.</param>
	/// <param name="CodeText">The whole text that the Function Body resides in.</param>
	/// <returns>A string containing the function body</returns>
	/// <exception cref="FileFormatException">If the .json wasn't formatted correctly, then <see cref="FileFormatException"/> will be called.</exception>
	static string GetFunctionBody(string PathToExtension, string FunctionName, Dictionary<string, object> Json){
		bool FunctionFound = Json.TryGetValue(FunctionName, out var Function) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		(int Start, int End) FunctionBounds = (0, 0);
		string CodeText = "";
		if (FunctionFound && Function is JsonElement element){
			FunctionBounds = (Convert.ToInt32(element.GetProperty("StartInFile").GetString()), Convert.ToInt32(element.GetProperty("EndInFile").GetString()));
			_ = element.TryGetProperty("RelativeFilePath", out JsonElement element_) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
			CodeText = File.ReadAllText(PathToExtension + element_.GetString() ?? throw new FileFormatException("File was not in the same Format as expected of a Json extension"));
		}
		return CustomFunctions.ToString<char>(new Span<char>(CodeText.ToArray()).Slice(FunctionBounds.Start, FunctionBounds.End).ToArray(), false);
	}
}
