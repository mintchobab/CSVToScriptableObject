using UnityEngine;

public static class Tables
{
	public static TestTableFirst TestTableFirst;
	public static TestTableSecond TestTableSecond;
	public static TestTableThird TestTableThird;

	static Tables()
	{
		if (TestTableFirst == null)
			TestTableFirst = Load<TestTableFirst>();

		if (TestTableSecond == null)
			TestTableSecond = Load<TestTableSecond>();

		if (TestTableThird == null)
			TestTableThird = Load<TestTableThird>();

	}

	public static T Load<T>() where T : ScriptableObject
	{
		T[] asset = Resources.LoadAll<T>("");

		if (asset == null || asset.Length != 1)
			throw new System.Exception($"{nameof(Tables)} : Tables Load Error");

		return asset[0];
	}
}
