﻿using UnityEngine;
using System.Collections;
using cn.vimfung.luascriptcore;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using cn.vimfung.luascriptcore.modules.oo;
using System.CodeDom;
using System;
using System.Reflection;

public class Sample : MonoBehaviour {

	/// <summary>
	/// 是否注册方法
	/// </summary>
	private bool _isRegMethod = false;

	/// <summary>
	/// 是否注册模块
	/// </summary>
	private bool _isRegModule = false;

	/// <summary>
	/// 是否注册类型
	/// </summary>
	private bool _isRegClass = false;

	/// <summary>
	/// 是否导入类型
	/// </summary>
	private bool _isClassImport = false;

	private bool _isCoroutineImport = false;

	public void Start()
	{
		LuaContext.currentContext.onException((errMessage) => {

			Debug.Log(errMessage);

		});
	}

	/// <summary>
	/// 解析脚本按钮点击
	/// </summary>
	public void evalScriptButtonClickedHandler ()
	{
		LuaValue retValue = LuaContext.currentContext.evalScript ("print(10);return 'Hello World';");
		Debug.Log (string.Format("{0}", retValue.toString()));
	}

	/// <summary>
	/// 注册方法按钮点击
	/// </summary>
	public void registerMethodButtonClickedHandler()
	{
		if (!_isRegMethod) 
		{
			LuaContext.currentContext.registerMethod("getDeviceInfo", (arguments) => {

				Dictionary<string, LuaValue> info = new Dictionary<string, LuaValue>();
				info.Add("productName", new LuaValue(Application.productName));
				return new LuaValue(info);
			});

			LuaContext.currentContext.registerMethod ("testReturnTuple", (arguments) =>
			{
				LuaTuple tuple = new LuaTuple();
				tuple.addRetrunValue("Hello");
				tuple.addRetrunValue(2017);
				tuple.addRetrunValue("World");
				tuple.addRetrunValue(111);

				return new LuaValue(tuple);
			});

			_isRegMethod = true;
		}

		LuaContext.currentContext.evalScriptFromFile (string.Format("{0}/main.lua", Application.streamingAssetsPath));
	}

	/// <summary>
	/// 调用lua方法按钮点击
	/// </summary>
	public void callLuaMethodButtonClickedHandler()
	{
		//加载Lua脚本
		LuaContext.currentContext.evalScriptFromFile("todo.lua");
		LuaValue retValue = LuaContext.currentContext.callMethod ("add", new List<LuaValue> (){ new LuaValue (1000.0), new LuaValue (24.0)});
		Debug.Log (string.Format ("result = {0}", retValue.toNumber ()));

		LuaValue funcValue = LuaContext.currentContext.callMethod ("getFunc", null);
		retValue = funcValue.toFunction ().invoke (new List<LuaValue> (){ new LuaValue (1000.0), new LuaValue (24.0)});
		Debug.Log (string.Format ("result = {0}", retValue.toNumber ()));

		LuaPointer ptr = new LuaPointer (funcValue.toFunction ());
		LuaContext.currentContext.callMethod ("printPointer", new List<LuaValue> (){ new LuaValue(ptr) });

		retValue = LuaContext.currentContext.callMethod ("testTuple", null);
		Debug.Log (string.Format ("result = {0}", retValue.toTuple ()));
	}

	/// <summary>
	/// 注册模块方法按钮点击
	/// </summary>
	public void registerModuleButtonClickedHandler()
	{
		if (!_isRegModule)
		{
			_isRegModule = true;
			LuaContext.currentContext.registerModule<LogModule> ();
		}

		LuaContext.currentContext.evalScript ("LogModule.writeLog('Hello World!'); local a = LogModule.test({1,2,3,4}); print(a);");
	}

	/// <summary>
	/// 注册类型按钮点击
	/// </summary>
	public void registerClassButtonClickedHandler()
	{
		if (!_isRegClass)
		{
			_isRegClass = true;
			LuaContext.currentContext.registerModule<Person> ();
		}
		LuaContext.currentContext.evalScript ("function Person.prototype:init() print('Person create'); end; local p = Person.createPerson(); print(p); p:setName('xxxx'); p:speak(); print(Person.printPerson(p));");
	}

	/// <summary>
	/// 全局变量操作按钮点击
	/// </summary>
	public void globalValueButtonClickedHandler ()
	{
		LuaContext.currentContext.setGlobal ("testVar", new LuaValue ("abc"));
		LuaValue retValue = LuaContext.currentContext.getGlobal ("testVar");
		Debug.Log (string.Format ("retValue = {0}", retValue.toString()));
	}

	/// <summary>
	/// 导入类型按钮点击事件
	/// </summary>
	public void importClassButtonClickedHandler ()
	{
		if (!_isClassImport)
		{
			_isClassImport = true;
			LuaContext.currentContext.registerModule<LuaClassImport> ();
			LuaClassImport.setIncludesClasses (LuaContext.currentContext, new List<Type> (){ typeof(Person), typeof(NativeData) });
		}

		LuaContext.currentContext.evalScript ("local Person = ClassImport('Person'); local NativeData = ClassImport('NativeData'); print(Person, NativeData); local d = NativeData.create(); d:setDataId('xxx'); print(d:dataId()); local p = NativeData.createPerson(); print(p); p:setName('xxxx'); p = Person.printPerson(p); print(p); print(p:name());");
	}

	public void retainAndReleaseButtonClickedHandler ()
	{
		LuaContext.currentContext.registerModule<Person> ();

		LuaContext.currentContext.evalScript ("local test = function() print('test func') end; test(); Person.retainHandler(test);");
		LuaContext.currentContext.evalScript ("print('-------------1'); Person.callHandler(); Person.releaseHandler();");
		LuaContext.currentContext.evalScript ("print('-------------2'); Person.callHandler();");
	}

	public void coroutineButtonClickedHandler ()
	{
		if (!_isCoroutineImport)
		{
			_isCoroutineImport = true;

			LuaContext.currentContext.registerModule<Person> ();
			LuaContext.currentContext.registerModule<LogModule> ();
			LuaContext.currentContext.registerModule<LuaClassImport> ();

			LuaClassImport.setIncludesClasses (LuaContext.currentContext, new List<Type> (){ typeof(Person), typeof(NativeData) });

			LuaContext.currentContext.registerMethod ("GetValue", (arguments) =>
			{
				return new LuaValue (1024);

			});

			LuaContext.currentContext.registerMethod ("GetPixel", (arguments) =>
			{

				LuaTuple tuple = new LuaTuple ();
				tuple.addRetrunValue (100);
				tuple.addRetrunValue (38);
				tuple.addRetrunValue (1002);

				return new LuaValue (tuple);

			});
		}

		LuaContext.currentContext.evalScriptFromFile ("coroutine.lua");
	}
}
