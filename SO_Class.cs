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
    //���±��
    [HideInInspector]
    public bool Version_Mark;
    //Unique��ʶ��
    [HideInInspector]
    public long UUID;
    //��ΪType�ᷢ���ı�.
    public string Type;
    //��Ϊ��Щֵ�Ǿ�̬�����Կ���ʹ�õ����洢.
    public List<FieldInfoContainer> StaticFields = new List<FieldInfoContainer>();
    //��Ϊ��Щֵ�Ƕ�̬������ÿ����Ӧ��Object����Ҫ����һ���ļ��洢.
    public List<FieldInfoContainer> VariableFields = new List<FieldInfoContainer>();

}



