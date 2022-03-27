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
    //在none时表现为,同一个类将会有不同的参数,但是继承了该参数的类,并不会调用同一值,如A继承B,A中的damage 与B中的damage是不同结算的.(需要在不同的类中声明)
    //在global时表现为,即使是完全不同的类,完全不相互继承,但仍会调用同名的global参数,如A与B,它们都声明了一个damage变量,都被标记为global,那么这些damage变量则会具有相同的值.(需要在不同的类中声明)
    //在inherited时表现为,如果相互继承,则同名参数具有相同的值.(只需要在基类声明出来)

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