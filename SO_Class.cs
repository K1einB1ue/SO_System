using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.ComponentModel;



[Serializable]
public class FieldInfoContainer {
    public string Name;
    public string Type;
}

public class SO_Class : ScriptableObject {
    //更新标记
    [HideInInspector]
    public bool Version_Mark;
    //Unique标识符
    [HideInInspector]
    public long UUID;
    //因为Type会发生改变.
    public string Type;
    //因为这些值是静态的所以可以使用单例存储.
    public List<FieldInfoContainer> StaticFields = new List<FieldInfoContainer>();
    //因为这些值是动态的所以每个对应的Object都需要生成一份文件存储.
    public List<FieldInfoContainer> VariableFields = new List<FieldInfoContainer>();

}



