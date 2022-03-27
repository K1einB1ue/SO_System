using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace __SO__
{

    [CustomEditor(typeof(SO_Base), true)]
    public class SO_Base_Editor : Editor
    {
        private bool LoadNeed = true;

        public override void OnInspectorGUI() {
            SO_Base soBase = (SO_Base)target;
            base.OnInspectorGUI();
            if (soBase.UUID == null) {
                soBase.UUID = System.Guid.NewGuid().ToString("N");
            }
            if (GUI.changed) {
                SO.Save(soBase);
                LoadNeed = true;
                EditorUtility.SetDirty(soBase);
            }
            if (LoadNeed) {
                SO.Load(soBase);
                LoadNeed = false;
            }
        }
    }



    public static class SO {

        
        private static string[] GetFileSystemEntries(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false) {
            List<string> lst = new List<string>();
            try {
                foreach (string item in Directory.GetFileSystemEntries(dir)) {
                    try {
                        if (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline)) {
                            var lastDot = item.LastIndexOf('.');
                            var ext = item.Substring(lastDot);
                            if (!Regex.IsMatch(ext, ".meta", RegexOptions.IgnoreCase | RegexOptions.Multiline)) {
                                lst.Add(item);
                            }            
                        }
                        if (recurse && (File.GetAttributes(item) & FileAttributes.Directory) == FileAttributes.Directory) { lst.AddRange(GetFileSystemEntries(item, regexPattern, true)); }
                    } catch { if (throwEx) { throw; } }
                }
            } catch { if (throwEx) { throw; } }
            return lst.ToArray();
        }

        public static string SO_System_Path = null;
        public static SO_System SO_System {

            get {
                if (SO_System_Path == null) {
                    var files = GetFileSystemEntries(Application.dataPath, "SO_Config.json", true, true);
                    if (files.Length > 1) {
                        throw new Exception($"搜索到了{files.Length}个SO_Config.json文件.请保证项目中只有一个SO_Config.json");
                    }
                    else if (files.Length == 0) {
                        throw new Exception($"没有搜索到SO_Config.json文件.请保证项目中有一个SO_Config.json");
                    }
                    {
                        StreamReader file = null;
                        try {
                            file = File.OpenText(System.IO.Path.Combine(System.Environment.CurrentDirectory, files[0]));
                        }
                        catch {
                            throw new Exception("打开SO_Config.json文件失败");
                        }
                        if (file != null) {
                            using (JsonTextReader reader = new JsonTextReader(file)) {
                                JObject oV = (JObject)JToken.ReadFrom(reader);
                                try {
                                    SO_System_Path = (string)oV["SO_System_Path"].ToObject(typeof(string));
                                }
                                catch {
                                    throw new Exception("没有定义SO_System_Path在Resources中的路径");
                                }
                            }
                        }
                    }
                    return Resources.Load<SO_System>(SO_System_Path);
                }
                else {
                    return Resources.Load<SO_System>(SO_System_Path);
                }
            }
        }
    public static string SO_Static_Save_File_Path(string fileName) => System.IO.Path.Combine(Application.dataPath, "Resources", SO_System.SO_Static_File_Path, fileName + ".save");
    public static string SO_Variable_Save_File_Path(string fileName) => System.IO.Path.Combine(Application.dataPath, "Resources", SO_System.SO_Variable_File_Path, fileName + ".save");
    public static string SO_Static_Save_Path => Path.Combine(Application.dataPath, "Resources", SO_System.SO_Static_File_Path);
    public static string SO_Variable_Save_Path => Path.Combine(Application.dataPath, "Resources", SO_System.SO_Variable_File_Path);

    public static void RestoreJson(string fileName, JObject jObject) {
        FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs);
        string json = jObject.ToString();
        sw.WriteLine(json);
        fs.SetLength(json.Length);
        sw.Close();
        fs.Close();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [DidReloadScripts(0)]
    public static void Load_SO_System() {
        if (SO_System) {
            SO_System.Load();
            EditorUtility.SetDirty(SO_System);
        }
    }

    public static void Load(SO_Base SoBase) {
        var ClassObject = SO_System[SoBase.GetType().ToString()];
        if (ClassObject.StaticFields.Count != 0 && File.Exists(SO.SO_Static_Save_File_Path(SoBase.GetType().ToString()))) {
            using (StreamReader file = File.OpenText(SO.SO_Static_Save_File_Path(SoBase.GetType().ToString())))
            using (JsonTextReader reader = new JsonTextReader(file)) {
                JObject oV = (JObject)JToken.ReadFrom(reader);
                foreach (var StaticPart in ClassObject.StaticFields) {
                    SoBase.GetType().GetField(StaticPart.Name).SetValue(SoBase, oV[StaticPart.Name].ToObject(Type.GetType(StaticPart.Type)));
                }
            }
        }
        if (ClassObject.VariableFields.Count != 0 && File.Exists(SO.SO_Variable_Save_File_Path(SoBase.UUID))) {
            using (StreamReader file = File.OpenText(SO.SO_Variable_Save_File_Path(SoBase.UUID)))
            using (JsonTextReader reader = new JsonTextReader(file)) {
                JObject oV = (JObject)JToken.ReadFrom(reader);
                foreach (var VariablePart in ClassObject.VariableFields) {
                    SoBase.GetType().GetField(VariablePart.Name).SetValue(SoBase, oV[VariablePart.Name].ToObject(Type.GetType(VariablePart.Type)));
                }
            }
        }
    }


    public static void Save(SO_Base SoBase) {
        var ClassObject = SO_System[SoBase.GetType().ToString()];
        //公用量存储
        if (ClassObject.StaticFields.Count != 0) {
            JTokenWriter staticWriter = new JTokenWriter();
            staticWriter.WriteStartObject();
            foreach (var StaticPart in ClassObject.StaticFields) {
                staticWriter.WritePropertyName(StaticPart.Name);
                staticWriter.WriteValue(SoBase.GetType().GetField(StaticPart.Name).GetValue(SoBase));
            }
            staticWriter.WriteEndObject();
            JObject o = (JObject)staticWriter.Token;
#if SO_DEBUG
            Debug.Log("触发保存:" + SO.SO_Static_Save_File_Path(this.GetType().ToString()));
#endif
            SO.RestoreJson(SO.SO_Static_Save_File_Path(SoBase.GetType().ToString()), o);
        }

        //变量储存
        if (ClassObject.VariableFields.Count != 0) {
            JTokenWriter variableWriter = new JTokenWriter();
            variableWriter.WriteStartObject();
            foreach (var VariablePart in ClassObject.VariableFields) {
                variableWriter.WritePropertyName(VariablePart.Name);
                variableWriter.WriteValue(SoBase.GetType().GetField(VariablePart.Name).GetValue(SoBase));
            }
            variableWriter.WriteEndObject();
            JObject o = (JObject)variableWriter.Token;
#if SO_DEBUG
            Debug.Log("触发保存:" + SO.SO_Variable_Save_File_Path(UUID));
#endif
            SO.RestoreJson(SO.SO_Variable_Save_File_Path(SoBase.UUID), o);
        }
    }
}



    [CreateAssetMenu(fileName = "SO_System", menuName = "(用户禁用)/SO_System")]
    public class SO_System : ScriptableObject
    {
        [SerializeField]
        public string SO_Class_File_Path = "";
        public string SO_Static_File_Path = "";
        public string SO_Variable_File_Path = "";
        public List<SO_Class> SO_Classes;


        private bool SO_Version_Mark;
        public Dictionary<string, int> SO_Exist = new Dictionary<string, int>();

        private Dictionary<string, SO_Class> SO_Classes_Dictionary = null;



        public static Dictionary<Type, JToken> SO_FallBack_Mapping = new Dictionary<Type, JToken>() {
        {typeof(string), ""}
    };

        public object DefaultObject(Type FieldType) {
            try {
                return Activator.CreateInstance(FieldType);
            }
            catch {
                if (SO_FallBack_Mapping.TryGetValue(FieldType, out var value)) {
                    return value;
                }
                else {
                    throw new Exception($"无法被正常Json化:{FieldType}");
                }
            }
        }
        public SO_Class this[string Typename] {
            get {
                if (SO_Classes_Dictionary == null) {
                    SO_Classes_Dictionary = new Dictionary<string, SO_Class>();
                    foreach (var SO_Class in SO_Classes) {
                        SO_Classes_Dictionary.Add(SO_Class.Type, SO_Class);
                    }
                }
                if (SO_Classes_Dictionary.TryGetValue(Typename, out SO_Class value)) {
                    return value;
                }
                else {
                    throw new Exception($"未在SO_System中检测到对应类:{Typename}");
                }
            }
        }

        //在编辑器界面尝试去重载所有的SO_Class
        public void Load() {
            this.SO_Classes_Dictionary = null;
            CreateSO_Classes();
            CreateStaticSave();
        }

        private void CreateSO_Classes() {
            //版本标记
            SO_Version_Mark = !SO_Version_Mark;
            //先获得所有的SO_Classes
            List<SO_Class> tmp = new List<SO_Class>();
            SO_Classes ??= new List<SO_Class>();
            for (int i = 0; i < SO_Classes.Count; i++) {
                if (SO_Classes[i]) {
                    SO_Exist.Add(SO_Classes[i].Type, i);
                }
            }
            Type type = typeof(SO_Base);
            Assembly assembly = Assembly.GetAssembly(type);
            foreach (var child in assembly.GetTypes()) {
                object[] vs = child.GetCustomAttributes(typeof(SO_ClassAttribute), false);
                if (vs.Length > 0) {
                    if (!SO_Exist.TryGetValue(child.ToString(), out int value)) {
                        var temp = CreateInstance<SO_Class>();
                        temp.Type = child.ToString();
                        if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources", SO_Class_File_Path))) {
                            Debug.LogError("不存在指定路径!");
                            Debug.LogError($"开始创建路径:{Path.Combine(Application.dataPath, SO_Class_File_Path)}");
                            AssetDatabase.CreateFolder("Assets/Resources", SO_Class_File_Path);
                            AssetDatabase.Refresh();
                        }
                        AssetDatabase.CreateAsset(temp, Path.Combine("Assets/Resources", SO_Class_File_Path, temp.Type + ".asset"));
                        Debug.Log("创建了:" + Path.Combine("Assets/Resources", SO_Class_File_Path, temp.Type + ".asset"));
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        temp.Version_Mark = SO_Version_Mark;
                        tmp.Add(temp);
                    }
                    else {
                        if (value != -1) {
                            tmp.Add(SO_Classes[value]);
                            SO_Classes[value].Version_Mark = SO_Version_Mark;
                            SO_Exist[child.ToString()] = -1;
                        }
                    }
                }
            }
            foreach (var val in SO_Exist) {
                if (val.Value != -1) {
                    AssetDatabase.DeleteAsset(Path.Combine("Assets/Resources", SO_Class_File_Path, val.Key + ".asset"));
                    AssetDatabase.Refresh();
                }
            }
            foreach (var SOclass in FindObjectsOfType<SO_Class>()) {
                //foreach (var assetName in Directory.GetFiles(Path.Combine(Application.dataPath, "Resources", SO_File_Path), "*.asset")) {
                //SO_Class SOclass = Resources.Load<SO_Class>(Path.Combine(Application.dataPath, "Resources", SO_File_Path, assetName));
                if (SOclass.Version_Mark != SO_Version_Mark) {
                    AssetDatabase.DeleteAsset(Path.Combine("Assets/Resources", SO_Class_File_Path, SOclass.Type + ".asset"));
                    AssetDatabase.Refresh();
                }
            }
            SO_Classes = tmp;
            SO_Exist.Clear();


            foreach (var classObject in SO_Classes) {
                classObject.VariableFields.Clear();
                classObject.StaticFields.Clear();
                Type SO_class_type = Type.GetType(classObject.Type);
                var fields = SO_class_type.GetFields();
                foreach (var field in fields) {
                    var atts = field.GetCustomAttributes(typeof(SO_StaticAttribute), true);
                    if (atts.Length > 0) {
                        var temp = new FieldInfoContainer();
                        temp.Type = field.FieldType.ToString();
                        temp.Name = field.Name;
                        classObject.StaticFields.Add(temp);
                    }
                    atts = field.GetCustomAttributes(typeof(SO_VariableAttribute), true);
                    if (atts.Length > 0) {
                        var temp = new FieldInfoContainer();
                        temp.Type = field.FieldType.ToString();
                        temp.Name = field.Name;
                        classObject.VariableFields.Add(temp);
                    }
                }
                EditorUtility.SetDirty(classObject);
            }
        }

        private void CreateStaticSave() {
            //找到需要添加和删除的文件
            var fullNames = System.IO.Directory.GetFiles(SO.SO_Static_Save_Path, "*.save");
            List<string> fileNames = new List<string>();    //不带拓展名的文件名
            foreach (var fullName in fullNames) {
                var tmp = System.IO.Path.GetFileName(fullName);
                var index = tmp.LastIndexOf('.');
                var fileNameWithoutExt = tmp.Substring(0, index);
                fileNames.Add(fileNameWithoutExt);
            }

            Dictionary<string, int> table = new Dictionary<string, int>();  //现在文件目录下的表
            foreach (var fileName in fileNames) {
                table.Add(fileName, 1);
            }
            List<string> deleteList = new List<string>();
            List<string> existList = new List<string>();
            List<SO_Class> addList = new List<SO_Class>();
            foreach (var soClass in SO_Classes) {
                if (!table.TryGetValue(soClass.Type, out var value)) {
                    addList.Add(soClass);
                }
                else {
                    table[soClass.Type] = value - 1;
                }
            }

            foreach (var keyValue in table) {
                if (keyValue.Value != 0) {
                    deleteList.Add(keyValue.Key);
                }
                else {
                    existList.Add(keyValue.Key);
                }
            }

            //进行添加操作.
            JTokenWriter staticWriter = new JTokenWriter();
            foreach (var soClass in addList) {  //添加全新的文件,意味着它们不再需要进行文件校准
                staticWriter.WriteStartObject();
                foreach (var StaticPart in soClass.StaticFields) {
                    staticWriter.WritePropertyName(StaticPart.Name);
                    staticWriter.WriteValue(DefaultObject(Type.GetType(soClass.Type).GetField(StaticPart.Name).FieldType));
                }
                staticWriter.WriteEndObject();
                JObject o = (JObject)staticWriter.Token;
#if SO_DEBUG
            Debug.Log("添加新的Static文件:" + SO.SO_Static_Save_File_Path(soClass.Type));
#endif
                FileStream fs = new FileStream(SO.SO_Static_Save_File_Path(soClass.Type), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                string json = o.ToString();
                sw.WriteLine(json);
                fs.SetLength(json.Length);
                sw.Close();
                fs.Close();
            }
            //进行删除操作.
            foreach (var fileName in deleteList) {
#if SO_DEBUG
            Debug.Log("删除过时的Static文件:" + SO.SO_Static_Save_File_Path(fileName));
#endif
                System.IO.File.Delete(SO.SO_Static_Save_File_Path(fileName));
            }
            //进行文件校准(对已存在的文件)

            Dictionary<string, bool> fieldAdjustDic = new Dictionary<string, bool>();
            foreach (var fileName in existList) {
#if SO_DEBUG
            Debug.Log("更新已有的Static文件:" + SO.SO_Static_Save_File_Path(fileName));
#endif
                JObject oV = null;
                using (StreamReader file = File.OpenText(SO.SO_Static_Save_File_Path(fileName))) {
                    using (JsonTextReader reader = new JsonTextReader(file)) {
                        oV = (JObject)JToken.ReadFrom(reader);

                        fieldAdjustDic.Clear();
                        foreach (var StaticPart in SO.SO_System[fileName].StaticFields) {
                            if (oV.TryGetValue(StaticPart.Name, out var value)) {
                                try {
                                    value.ToObject(Type.GetType(StaticPart.Type));
                                }
                                catch { //不能正确转化类型时.
                                    Debug.Log($"修改了属性类型:{StaticPart.Name}于{fileName}.save");
                                    fieldAdjustDic.Add(StaticPart.Name, true);
                                }
                            }
                            else { //没有对应属性时
                                Debug.Log($"补充了属性:{StaticPart.Name}于{fileName}.save");
                                fieldAdjustDic.Add(StaticPart.Name, true);
                            }
                        }
                    }
                }
                if (fieldAdjustDic.Count != 0) {
                    var soClass = SO.SO_System[fileName];
                    staticWriter.WriteStartObject();
                    foreach (var StaticPart in soClass.StaticFields) {
                        staticWriter.WritePropertyName(StaticPart.Name);
                        if (fieldAdjustDic.TryGetValue(StaticPart.Name, out var value)) {
                            staticWriter.WriteValue(DefaultObject(Type.GetType(soClass.Type).GetField(StaticPart.Name).FieldType));
                        }
                        else {
                            staticWriter.WriteValue(oV[StaticPart.Name].ToObject(Type.GetType(StaticPart.Type)));
                        }
                    }
                    staticWriter.WriteEndObject();
                    JObject o = (JObject)staticWriter.Token;
                    SO.RestoreJson(SO.SO_Static_Save_File_Path(soClass.Type), o);
                }
            }
        }

    }

    [CustomEditor(typeof(SO_System))]
    public class SO_System_Editor : Editor
    {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            var system = target as SO_System;
            var SO_Class_File_Path = serializedObject.FindProperty(nameof(system.SO_Class_File_Path));
            var SO_Classes = serializedObject.FindProperty(nameof(system.SO_Classes));
            var SO_Static_File_Path = serializedObject.FindProperty(nameof(system.SO_Static_File_Path));
            var SO_Variable_File_Path = serializedObject.FindProperty(nameof(system.SO_Variable_File_Path));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(SO_Class_File_Path);
            EditorGUILayout.PropertyField(SO_Static_File_Path);
            EditorGUILayout.PropertyField(SO_Variable_File_Path);
            EditorGUILayout.PropertyField(SO_Classes);


            if (GUILayout.Button("生成", GUILayout.Width(200))) {
                SO.Load_SO_System();
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

    }
}