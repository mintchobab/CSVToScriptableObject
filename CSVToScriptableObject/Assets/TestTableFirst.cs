using System;
using System.Collections.Generic;
using UnityEngine;

public class TestTableFirst : SingletonScriptableObject<TestTableFirst>
{
	[SerializeField]
	public List<TableData> datas = new List<TableData>();

	public TableData this[int index]
	{
		get
		{
			return datas.Find(x => x.ID == index);
		}
	}

	[Serializable]
	public class TableData
	{
		public int ID;
		public string Name;
		public string Desc;
		public string IconPath;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

