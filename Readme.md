#### 1.如何配置SO_System

>  在Unity中安装https://github.com/GlitchEnzo/NuGetForUnity

>  在UnityNuget中安装https://github.com/JamesNK/Newtonsoft.Json

```shell
git clone https://github.com/K1einB1ue/SO_System.git
```

> 右键Resources文件夹 Create/(用户禁用)/SO_System
>
>  配置SO_Config.json文件.它可以被放置在Asset文件夹下的任意位置.

```json
{
    "SO_System_Path": "<你所放置SO_System的位置(相对于Resources文件夹)>"
}
```

使用例:

```json
{
    "SO_System_Path": "Saves/SO_System"
}
```

![截图0](https://raw.githubusercontent.com/K1einB1ue/GitImgs/8fe50ea8e4a1ebb191956dd406218d0ab366420a/SO_System/J%24Q%40%5DOW%2437IIY3P%60D8%6099U2.png "SO_System")

> 更改SO_Class_File_Path以(保存/调用)SO_Classes(用于保存SO_Class类型,及其SO_Variable以及SO_Static内容物的类型).
>
> 更改SO_Static_File_Path以(保存/调用)SO_Class中的Static部分.
>
> 更改SO_Variable_File_Path以(保存/调用)SO_Class中的Variable部分.
>
> 点击生成键可以主动生成SO_Classes(一般情况下,在重新载入脚本时它会自动地生成.)

#### 2.如何使用SO_System

```c#
[SO_Class]
public class TestUse : SO_Base
{
    
    [SO_Static] public int tmp;//它将会在同样的类型中保持相同的量(但是在运行时是不行的)
    [SO_Static] public int damage;
    [SO_Variable] public int test;//它将会在不同的个体中有不同的量
    [SO_Variable] public int another;
   
	//SO系列的变量加载后的回调.
    protected override void SO_Enable() {
        //在GameObject.Enable之中完成了对SO变量的读取.
    }

    //SO系列的变量保存前的回调
    protected override void SO_Disable() {
		//在GameObject.Disable之中完成了对SO变量的存储.
    }

    void Start()
    {
        //code as normal here!!
    }

    // Update is called once per frame
    void Update()
    {
        //code as normal here!!
    }

}
```

