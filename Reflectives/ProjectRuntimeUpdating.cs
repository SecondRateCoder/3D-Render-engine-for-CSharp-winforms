using System.Reflection;

class ReflectionUpdating{
    /// <summary>
    /// This class is to allow a class to be Read/Written to during RunTime.
    /// </summary>
    public class RuntimeManipulationInheritable{
        static Assembly ExtensionAssembly;
        /*
        static RuntimeManipulationInheritable(){
            Assembly? ExtensionAssembly_ = Assembly.GetAssembly(typeof(Default));
            //Quietly exit the Application.
            if(ExtensionAssembly_ == null){Application.Exit();}else{ExtensionAssembly = ExtensionAssembly_;}
        }
        */
    }


    public class Key{
        static Key(){
            _masterKey = new Key(AppDomain.CurrentDomain.BaseDirectory + "Cache/KeyData");
        }
        /// <summary>The master key for this application.</summary>
        public static Key masterKey{get{
            if(_masterKey == null){
                _masterKey = new Key(AppDomain.CurrentDomain.BaseDirectory + "Cache/KeyData");
            }
            return _masterKey;
        }}
        static Key? _masterKey;
        public int key_{get; private set;}
        /// <summary>
        /// Constructs a key from a Key.key and Key.mtf file.
        /// </summary>
        /// <param name="keysPath">The Directory where the Keys are stored.</param>
        /// <exception cref="TypeInitializationException">If the Key.key or Key.mtf were not found.</exception>
        public Key(string keysPath){
            if(Directory.Exists(keysPath)){
                DirectoryInfo di = new DirectoryInfo(keysPath);
                FileInfo[] finfo = [new(System.IO.Path.Combine(keysPath, "Key.key")), new(System.IO.Path.Combine(keysPath, "Key.mtf"))];
                if(!finfo[0].Exists){throw new TypeInitializationException(nameof(Key), new FileNotFoundException($"The file Key.key was not found at {keysPath}"));}
                if(!finfo[0].Exists){throw new TypeInitializationException(nameof(Key), new FileNotFoundException($"The file Key.mtf was not found at {keysPath}"));}
                int[] keyBytes = new int[finfo[0].Length];
                byte[] keyScrambler = new byte[finfo[1].Length];
                int ScramblePerIncrement;
                if(finfo[1].Length % finfo[0].Length == 0){
                    ScramblePerIncrement = (int)(finfo[1].Length / finfo[0].Length);
                }else{throw new TypeInitializationException(nameof(Key), new ArgumentOutOfRangeException(System.Reflection.ConstructorInfo.ConstructorName));}
                using(BinaryReader keyStream = new BinaryReader(File.OpenRead(System.IO.Path.Combine(keysPath, "Key.key")))){
                    for(int i =0; i < finfo[0].Length; i++){keyBytes[i] = keyStream.ReadByte();}
                    for(int i =0; i < finfo[1].Length; i++){keyScrambler[i] = keyStream.ReadByte();}
                    keyStream.Dispose();
                }
                int i_ = 1;
                int buffer =0;
                for(int i = 0; i < finfo[0].Length; i++, i_+= ScramblePerIncrement){
                    if(keyScrambler[i_] > keyScrambler[0]){
                        //Bit Shift Right.
                        for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(keyBytes[i] + buffer)) >> keyScrambler[cc];}
                    }else{
                        //Bit Shift Left.
                        for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(keyBytes[i] - buffer)) << keyScrambler[cc];}
                    }
                }
                this.key_ = buffer;
            }
        }
        public Key(int[] key, byte[] scrambleCode){
            if(scrambleCode.Length % key.Length != 0){throw new ArgumentOutOfRangeException(nameof(scrambleCode), "The scramble array must be divisible by the key length.");}
            int ScramblePerIncrement = scrambleCode.Length % key.Length;
            int i_ = 1;
            int buffer =0;
            for(int i = 0; i < key.Length; i++, i_+= ScramblePerIncrement){
                if(scrambleCode[i_] > scrambleCode[0]){
                    //Bit Shift Right.
                    for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(key[i] + buffer)) >> scrambleCode[cc];}
                }else{
                    //Bit Shift Left.
                    for(int cc = i_; cc < ScramblePerIncrement; cc++){buffer = (Math.Abs(key[i] - buffer)) << scrambleCode[cc];}
                }
            }
            this.key_ = buffer;
        }
        /// <summary>
        /// Creates a key from a key and a scramble array.
        /// </summary>
        /// <param name="key">The key to be encoded.</param>
        /// <param name="scrambleArray">The encoding array that the key is encoded by.</param>
        /// <param name="ScrambleCode">The integer that helps the algorithm decide whether to Bit shift forward or Backward.</param>
        /// <param name="ScramblePerDigit">the amount of scrambling that will occur per digit.</param>
        /// <returns>An integer array that encodes the key.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the ScramblePerDigit and the ScrambleArray are not compatible.</exception>
        /// <remarks>The ScrambleArray and the <see cref="int[]"/> must be retained for the original key to be restored.</remarks> 
        public static int[] CreateEncodedKey(int key, byte[] scrambleArray, int ScrambleCode = 0, int ScramblePerDigit = 4){
            if(scrambleArray.Length % ScramblePerDigit != 0){throw new ArgumentOutOfRangeException(nameof(scrambleArray), "The scramble array must be divisible by the ScramblePerDigit.");}
            int[] TrueKey = new int[scrambleArray.Length/ScramblePerDigit];
            if(ScrambleCode <= 0 | ScrambleCode > byte.MaxValue)ScrambleCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, byte.MaxValue);
            int i_ = scrambleArray.Length;
            //Work forward through the TrueKey array, 
            //i_ wotks backward through the ScrambleArray.
            //ScramblePerDigit is the number of bits to be scrambled per element in TrueKey.
            for(int i = TrueKey.Length; i >= 0; i--, i_-= ScramblePerDigit){
                    if (ScrambleCode > 0){
                        // Force Bit Shift Right
                        for(int cc = i + ScramblePerDigit; cc > i; cc--){TrueKey[i] = (key << scrambleArray[cc]) + key;}
                    }else{
                        // Force Bit Shift Left
                        for(int cc = i + ScramblePerDigit; cc > i; cc--){TrueKey[i] = (key >> scrambleArray[cc]) - key;}
                    }
            }
            return TrueKey;
        }
        new int GetHashCode(){return HashCode.Combine(this.GetType(), this.GetType().Name, this.key_);}
        public static bool operator ==(Key k1, Key k2){if(k1.key_ == k2.key_){return true;}else{return false;}}
        public static bool operator !=(Key k1, Key k2){return !(k1.key_ == k2.key_);}
    }
}