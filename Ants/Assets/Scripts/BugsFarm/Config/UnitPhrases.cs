using System.Collections.Generic;
using UnityEngine;


/*
	RegEx:
		^([^\r]+)
		"$1",
*/


[CreateAssetMenu(
	fileName	=						ScrObjs.AntPhrases,
	menuName	= ScrObjs.folder +		ScrObjs.AntPhrases,
	order		=						ScrObjs.AntPhrases_i
)]
public class UnitPhrases : ScriptableObject
{
	[TextArea]
	public List< string >	idle			= new List<string>()
	{
"Я еще маленький",
"...",
"Скоро я буду искать жертву",
"Не плохо, не плохо.",
"Еды мне хватит на долго",
"Не будтите во мне зло",
"А вообще я добрый",
"Я просто тут побуду.",
"Если меня не злить, то всё будет хорошо",
"Просто следите, чтоб еда была...",
"Пока всё хорошо",
"В бою мне не будет равных",
"Вот бы ещё вырасти.",
	};


	[TextArea]
	public List< string >	noFood			= new List<string>()
	{
"Еда закончилась",
"Еды нет, но я найду решение.",
"Вы не беспокоитесь?",
"Возможно, кто то погибнет...",
"И кого мне поймать первым?",
"Я выхожу на охоту.",
	};


	[TextArea]
	public List< string >	noWater			= new List<string>()
	{
	};


	[TextArea]
	public List< string >	awaken			= new List<string>()
	{
"Зачем меня разбудили?",
"Ой зря Вы это сдлали",
"Я сейчас буду очень зол",
"Я хотел спать",
	};


	[TextArea]
	public List< string >	training		= new List<string>()
	{
	};


	[TextArea]
	public List< string >	patrol			= new List<string>()
	{
	};


	[TextArea]
	public List< string >	queue			= new List<string>()
	{
	};


	[TextArea]
	public List< string >	mining			= new List<string>()
	{
	};


	[TextArea]
	public List< string >	digging			= new List<string>()
	{
	};
}

