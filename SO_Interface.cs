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



public abstract class SO_Base : MonoBehaviour
{

    public string UUID = null;

    protected virtual void SO_Enable() { }
    protected virtual void SO_Disable() { }
    protected void OnEnable() {
        __SO__.SO.Load(this);
        SO_Enable();
    }

    protected void OnDisable() {
        SO_Disable();
        __SO__.SO.Save(this);
    }

}

public enum SO_UniqueType
{
    none,
    global,
    inherited,
}

[AttributeUsage(AttributeTargets.Class)]
public class SO_ClassAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field)]
public class SO_StaticAttribute : Attribute
{
    //��noneʱ����Ϊ,ͬһ���ཫ���в�ͬ�Ĳ���,���Ǽ̳��˸ò�������,���������ͬһֵ,��A�̳�B,A�е�damage ��B�е�damage�ǲ�ͬ�����.(��Ҫ�ڲ�ͬ����������)
    //��globalʱ����Ϊ,��ʹ����ȫ��ͬ����,��ȫ���໥�̳�,���Ի����ͬ����global����,��A��B,���Ƕ�������һ��damage����,�������Ϊglobal,��ô��Щdamage������������ͬ��ֵ.(��Ҫ�ڲ�ͬ����������)
    //��inheritedʱ����Ϊ,����໥�̳�,��ͬ������������ͬ��ֵ.(ֻ��Ҫ�ڻ�����������)

    public string m_pastName = null;
    public SO_StaticAttribute(string pastName = null) {
        m_pastName = pastName;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SO_VariableAttribute : Attribute
{
    public string m_pastName = null;
    public SO_VariableAttribute(string pastName = null) {
        m_pastName = pastName;
    }
}