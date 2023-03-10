using System;

public static class EnumExtensions
{

	public static T GetAttribute<T>(this Enum value) where T : Attribute
	{
		//Получаем тип перечисления из текущего значения
		var type = value.GetType();
		//находим члена типа по его значению
		var memberInfo = type.GetMember(value.ToString());
		//получаем все атрибуты
		var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
		return attributes.Length > 0 ? (T)attributes[0] : null;
	}

}